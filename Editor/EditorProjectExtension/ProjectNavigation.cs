using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorProjectExtension.Navigation {
    public class ProjectNavigation {
        public EditorWindow Window;
        public string CurrentPath { get; private set; }
        private readonly LinkedList<string> _backStack;
        private readonly LinkedList<string> _forwardStack;

        public ProjectNavigation(EditorWindow window) {
            Window = window;
            CurrentPath = ProjectNavigationExtension.GetProjectWindowPath(window);
            _backStack = new LinkedList<string>();
            _forwardStack = new LinkedList<string>();
        }

        public void OnWindowPathChange(string newPath) {
            if (string.IsNullOrEmpty(newPath)) {
                Debug.LogError("Path cant be null!");
                return;
            }
            if (newPath == CurrentPath) {
                return;
            }
            if (_backStack.Count != 0 && newPath == _backStack.First.Value) {
                _backStack.RemoveFirst();
                _forwardStack.AddFirst(CurrentPath);
            }else if (_forwardStack.Count != 0 && newPath == _forwardStack.First.Value) {
                _forwardStack.RemoveFirst();
                _backStack.AddFirst(CurrentPath);
            } else {
                _backStack.AddFirst(CurrentPath);
                _forwardStack.Clear();
            }
            CurrentPath = newPath;

            while (_forwardStack.Count > ProjectNavigationExtension.StackSize) {
                _forwardStack.RemoveLast();
            }

            while (_backStack.Count > ProjectNavigationExtension.StackSize) {
                _backStack.RemoveLast();
            }
        }

        public void Forward() {
            if (_forwardStack.Count == 0) {
                return;
            }
            string targetPath = _forwardStack.First.Value;
            ProjectNavigationExtension.SetProjectWindowPath(Window, targetPath);
            OnWindowPathChange(targetPath);
        }

        public void Back() {
            if (_backStack.Count == 0) {
                return;
            }
            string targetPath = _backStack.First.Value;
            ProjectNavigationExtension.SetProjectWindowPath(Window, targetPath);
            OnWindowPathChange(targetPath);
        }
    }
    
    [InitializeOnLoad]
    public static class ProjectNavigationExtension {
        public const int StackSize = 64;
        public static Dictionary<EditorWindow, ProjectNavigation> WindowDict;
        
        private static readonly Type ProjectWindowType;
        private static readonly MethodInfo GetActiveFolderPathMethod;
        private static readonly MethodInfo ShowFolderContents;
        
        static ProjectNavigationExtension() {
            ProjectWindowType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            GetActiveFolderPathMethod = ProjectWindowType.GetMethod("GetActiveFolderPath", BindingFlags.NonPublic | BindingFlags.Instance);
            ShowFolderContents = ProjectWindowType.GetMethod("ShowFolderContents", BindingFlags.NonPublic | BindingFlags.Instance);
            IsEnable = EditorPrefs.GetBool("ProjectNavigationExtensionEnable");
        }

        #region 开关相关
        private static bool isEnable;
        public static bool IsEnable {
            get => isEnable;
            set {
                if (value == isEnable) {
                    return;
                }

                isEnable = value;
                if (isEnable) {
                    WindowDict = new Dictionary<EditorWindow, ProjectNavigation>();
                    EditorApplication.update += TrackProjectWindowPaths;
                } else {
                    WindowDict.Clear();
                    WindowDict = null;
                    EditorApplication.update -= TrackProjectWindowPaths;
                }
                EditorPrefs.SetBool("ProjectNavigationExtensionEnable", value);
            }
        }
        
        [MenuItem("Edit/ProjectNavigationExtension/Enable")]
        private static void SetEnable() => IsEnable = true;
        
        [MenuItem("Edit/ProjectNavigationExtension/Disable")]
        private static void SetDisable()  => IsEnable = false;

        [MenuItem("Edit/ProjectNavigationExtension/Enable", true)]
        private static bool CanEnable() => !IsEnable;
        
        [MenuItem("Edit/ProjectNavigationExtension/Disable", true)]
        private static bool CanDisable()  => IsEnable;
        #endregion
        
        // 定义前进快捷键
        [MenuItem("Edit/ProjectNavigationExtension/Forward &RIGHT", false, 0)]
        private static void Forward() {
            if (!isEnable) {
                return;
            }
            var projectWindow = GetActiveProjectWindow();
            if (projectWindow == null) {
                return;
            }
            WindowDict.TryGetValue(projectWindow, out ProjectNavigation navigation);
            if (navigation == null) {
                Debug.LogError($"{projectWindow} is unbind!");
                return;
            }
            navigation.Forward();
        }
        
        // 定义后退快捷键
        [MenuItem("Edit/ProjectNavigationExtension/Back _&LEFT", false, 1)]
        private static void Back() {
            if (!isEnable) {
                return;
            }
            var projectWindow = GetActiveProjectWindow();
            if (projectWindow == null) {
                return;
            }
            WindowDict.TryGetValue(projectWindow, out ProjectNavigation navigation);
            if (navigation == null) {
                Debug.LogError($"{projectWindow} is unbind!");
                return;
            }
            navigation.Back();
        }
        
        private static float lastCheckTime;
        private const float CHECK_INTERVAL = 0.3f;
        private static void TrackProjectWindowPaths() {
            if (Time.realtimeSinceStartup - lastCheckTime < CHECK_INTERVAL) {
                return;
            }
            lastCheckTime = Time.realtimeSinceStartup;
            var projectWindows = Resources.FindObjectsOfTypeAll(ProjectWindowType);
            foreach (var o in projectWindows) {
                var window = (EditorWindow)o;
                WindowDict.TryGetValue(window, out ProjectNavigation navigation);
                if (navigation == null) {
                    navigation = new ProjectNavigation(window);
                    WindowDict.Add(window, navigation);
                }
                string currentPath = GetProjectWindowPath(window);
                navigation.OnWindowPathChange(currentPath);
            }
        }

        private static EditorWindow GetActiveProjectWindow() {
            var focusedWindow = EditorWindow.focusedWindow;
            if (focusedWindow != null && focusedWindow.GetType() == ProjectWindowType) {
                return focusedWindow; // 如果当前有焦点的窗口是Project窗口，返回它
            }
            return null;
        }

        public static string GetProjectWindowPath(EditorWindow projectWindow) {
            if (projectWindow == null) {
                Debug.LogError("ProjectWindow cant be null");
                return null;
            }
            
            if (GetActiveFolderPathMethod == null) {
                Debug.LogError("Unity source code has changed, reflection failed!");
                return null;
            }
            string path = GetActiveFolderPathMethod.Invoke(projectWindow, null) as string;
            return path;
        }

        public static void SetProjectWindowPath(EditorWindow projectWindow, string path) {
            if (projectWindow == null) {
                Debug.LogError("ProjectWindow cant be null");
                return;
            }
            
            if (ShowFolderContents == null) {
                Debug.LogError("Unity source code has changed, reflection failed!");
                return;
            }
            // 获取文件夹的实例ID
            var folderInstanceId = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path).GetInstanceID();
            ShowFolderContents.Invoke(projectWindow, new object[] { folderInstanceId, false });
        }
    }
}