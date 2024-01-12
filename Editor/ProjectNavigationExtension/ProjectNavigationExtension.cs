using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace QuickFavorites.Navigation {
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
        }

        public void Back() {
            if (_backStack.Count == 0) {
                return;
            }
            string targetPath = _backStack.First.Value;
            ProjectNavigationExtension.SetProjectWindowPath(Window, targetPath);
        }
    }
    
    [InitializeOnLoad]
    public static class ProjectNavigationExtension {
        public static int StackSize = 64;
        public static Dictionary<EditorWindow, ProjectNavigation> WindowDict;
        private static readonly Type ProjectWindowType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        
        static ProjectNavigationExtension() {
            WindowDict = new Dictionary<EditorWindow, ProjectNavigation>();
            EditorApplication.update += TrackProjectWindowPaths;
        }
        
        private static void TrackProjectWindowPaths() {
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
        
        // 定义前进快捷键
        [MenuItem("Edit/ProjectNavigationExtension/Forward &RIGHT", false, 1)]
        private static void Forward() {
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
        [MenuItem("Edit/ProjectNavigationExtension/Back _&LEFT", false, 0)]
        private static void Back() {
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
            
            var methodInfo = ProjectWindowType.GetMethod("GetActiveFolderPath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null) {
                Debug.LogError("Unity source code has changed, reflection failed!");
                return null;
            }
            string path = methodInfo.Invoke(projectWindow, null) as string;
            return path;
        }

        public static void SetProjectWindowPath(EditorWindow projectWindow, string path) {
            if (projectWindow == null) {
                Debug.LogError("ProjectWindow cant be null");
                return;
            }
            var showFolderContentsMethod = ProjectWindowType.GetMethod("ShowFolderContents", BindingFlags.NonPublic | BindingFlags.Instance);
            if (showFolderContentsMethod == null) {
                Debug.LogError("Unity source code has changed, reflection failed!");
                return;
            }
            // 获取文件夹的实例ID
            var folderInstanceId = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path).GetInstanceID();
            showFolderContentsMethod.Invoke(projectWindow, new object[] { folderInstanceId, false });
        }
    }
}