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
        public static QuickReferenceFinderConfig Config; 
        
        public static List<string> Search(string searchPattern, string pathToSearch, List<string> fileExtensions, string rootPath = null) {
            if (Config == null) {
                Config = Resources.Load<QuickReferenceFinderConfig>("QuickReferenceFinderConfig");
            }
            if (Config == null) {
                Debug.LogError("Need QuickReferenceFinderConfig!");
                return null;
            }
            
            using Process process = new Process();
            List<string> extensionsWithoutDots = new List<string>();
            foreach (string extension in fileExtensions) {
                extensionsWithoutDots.Add(extension.Replace(".", ""));
            }

            string folder = "win";
#if UNITY_EDITOR_OSX
            folder = "mac";
#endif

            var exts = string.Join(",", extensionsWithoutDots);
            var args = $"\"{searchPattern}\" \"{pathToSearch}\" -g \"*.{{{exts}}}\" --follow --files-with-matches --no-text --fixed-strings";
            var fileName = $"{Application.dataPath}/{Config.GetRipGrepPath()}/{folder}/rg";
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
            process.OutputDataReceived += (sender, e) => {
                if (e.Data == null) {
                    outputWaitHandle.Set();
                }
                else {
                    output.AppendLine(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) => {
                if (e.Data == null) {
                    errorWaitHandle.Set();
                }
                else {
                    error.AppendLine(e.Data);
                }
            };

            Debug.Log($"执行命令: {fileName} {args}");
            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            var timeout = 10000;
            if (process.WaitForExit(timeout) &&
                outputWaitHandle.WaitOne(timeout) &&
                errorWaitHandle.WaitOne(timeout)) {

                if (error.Length > 0) {
                    Debug.LogError($"Error: {output}");
                    return new List<string>();
                }

                var result = output.ToString();
                string[] lines = result.Split(new char[] { '\r', '\n' },
                    System.StringSplitOptions.RemoveEmptyEntries);

                List<string> relativePaths = new List<string>();
                string rootPathClean = rootPath == null ? pathToSearch : rootPath;
                rootPathClean = rootPathClean.Replace("/", "\\");
                foreach (string line in lines) {
                    string relativePath = GetRelativePath(rootPathClean, line).Replace("\\", "/");
                    relativePaths.Add(relativePath);
                }

                return relativePaths;
            }

            Debug.LogError("Ripgrep Timeout");
            return new List<string>();
        }

        public static string GetRelativePath(string fromPath, string toPath) {
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) {
                return toPath;
            } // 不同的路径根

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase)) {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}