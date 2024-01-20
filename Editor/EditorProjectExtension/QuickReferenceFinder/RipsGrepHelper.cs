using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EditorProjectExtension.ReferenceFinder {
    public static class RipsGrepHelper {
        public static QuickReferenceFinderConfig Config {
            get {
                if (_config != null) return _config;
                _config = Resources.Load<QuickReferenceFinderConfig>("QuickReferenceFinderConfig");
                if (_config != null) return _config;
                Debug.LogError("Need QuickReferenceFinderConfig!");
                return null;
            }
        }
        private static QuickReferenceFinderConfig _config;
        
        public static readonly List<string> ExtensionNameList = new List<string> {
            "prefab",
            "asset",
            "unity",
            "anim",
            "controller",
            "overrideController",
            "spriteatlas",
            "mat"
        };

        public static List<string> Search(string searchPattern, string searchPath) {
            string fileName = GetRipGrepPath();
            string args = GetRipGrepArguments(searchPattern, searchPath);
            
            Process process = new Process();
            List<string> relativePaths = new List<string>();
            try {
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                // https://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why
                using AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
                using AutoResetEvent errorWaitHandle = new AutoResetEvent(false);
                process.OutputDataReceived += (_, e) => {
                    if (e.Data == null) {
                        outputWaitHandle.Set();
                    } else {
                        output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (_, e) => {
                    if (e.Data == null) {
                        errorWaitHandle.Set();
                    } else {
                        error.AppendLine(e.Data);
                    }
                };

                if (!process.Start()) {
                    Debug.LogError("Failed to start ripGrep process.");
                    return null;
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                int timeout = Config.rgSearchTimeOutLimit;
                if (process.WaitForExit(timeout) &&
                    outputWaitHandle.WaitOne(timeout) &&
                    errorWaitHandle.WaitOne(timeout)) {

                    if (error.Length > 0) {
                        Debug.LogError($"Error: {output}");
                        return null;
                    }

                    var result = output.ToString();
                    string[] lines = result.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines) {
                        string relativePath = GetRelativePath(searchPath, line);
                        relativePaths.Add(relativePath);
                    }
                    return relativePaths;
                }
            } catch (Exception ex) {
                Debug.LogError("RipGrep process failed: " + ex.Message);
            } finally {
                process.Close();
                process.Dispose();
            }
            Debug.LogError("RipGrep Timeout");
            return relativePaths;
        }

        private static string GetRipGrepPath() {
            string platformFolder;
            switch (Application.platform) {
                case RuntimePlatform.WindowsEditor:
                    platformFolder = "win";
                    break;
                case RuntimePlatform.OSXEditor:
                    platformFolder = "mac";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var fileName = Path.Combine(Application.dataPath, Config.GetRipGrepPath(), platformFolder, "rg");
            return fileName;
        }

        private static string GetRipGrepArguments(string searchPattern, string pathToSearch) {
            var extensionStr = string.Join(",", ExtensionNameList);
            var args = $"\"{searchPattern}\" \"{pathToSearch}\" -g \"*.{{{extensionStr}}}\" --follow --files-with-matches --no-text --fixed-strings";
            return args;
        }

        public static string GetRelativePath(string projectRootPath, string fullPath) {
            Uri projectRootUri = new Uri(projectRootPath);
            Uri fullUri = new Uri(fullPath);

            if (projectRootUri.Scheme != fullUri.Scheme) {
                Debug.LogError($"{projectRootPath}与{fullPath}不在同一路径根下!");
                return fullPath;  // 不同的路径根
            }

            Uri relativeUri = projectRootUri.MakeRelativeUri(fullUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // 替换目录分隔符
            relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            
            // 寻找项目根路径中最后一个目录分隔符的位置
            int lastSeparatorIndex = projectRootPath.LastIndexOf(Path.DirectorySeparatorChar);
            if (lastSeparatorIndex != -1) {
                string lastPart = projectRootPath.Substring(lastSeparatorIndex + 1);
                string toRemove = lastPart + Path.DirectorySeparatorChar;
                if (relativePath.StartsWith(toRemove)) {
                    relativePath = relativePath.Substring(toRemove.Length);
                }
            }
            
            return relativePath;
        }
    }
}