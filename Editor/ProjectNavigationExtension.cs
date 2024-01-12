using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace QuickFavorites.Navigation {
    public class ProjectNavigationData {
        public EditorWindow Window;
        private static Stack<string> backStack = new Stack<string>();
        private static Stack<string> forwardStack = new Stack<string>();
    }
    
    public class ProjectNavigationExtension {
        public Dictionary<EditorWindow, ProjectNavigationData> Data;
        public const int  StackSize = 999;
        private static readonly Type ProjectWindowType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");

        // 定义后退快捷键
        [MenuItem("Edit/ProjectNavigationExtension/Back _&LEFT", false, 0)]
        private static void Back() {
            Debug.Log("Back");
            Debug.LogError(GetActiveProjectWindow());
            var projectWindow = GetActiveProjectWindow();
            Debug.LogError(GetProjectWindowPath(projectWindow));
        }

        // 定义前进快捷键
        [MenuItem("Edit/ProjectNavigationExtension/Forward &RIGHT", false, 1)]
        private static void Forward() {
            Debug.Log("Forward");
            Debug.LogError(GetActiveProjectWindow());
            
            var projectWindow = GetActiveProjectWindow();
            SetProjectWindowPath(projectWindow, "Assets");
        }

        public static EditorWindow GetActiveProjectWindow() {
            var focusedWindow = EditorWindow.focusedWindow;
            if (focusedWindow != null && focusedWindow.GetType() == ProjectWindowType) {
                return focusedWindow; // 如果当前有焦点的窗口是Project窗口，返回它
            }
            return null;
        }

        public static string GetProjectWindowPath(EditorWindow projectWindow) {
            if (projectWindow == null) {
                return null;
            }

            var projectWindowType = projectWindow.GetType();
            // 使用反射来获取当前路径
            var methodInfo = projectWindowType.GetMethod("GetActiveFolderPath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null) return null;
            string path = methodInfo.Invoke(projectWindow, null) as string;
            return path;
        }

        public static void SetProjectWindowPath(EditorWindow projectWindow, string path) {
            if (projectWindow == null || string.IsNullOrEmpty(path)) {
                return;
            }

            var projectWindowType = projectWindow.GetType();
            // 使用反射来设置当前路径
            MethodInfo methodInfo = projectWindowType.GetMethod(
                "SetFolderSelection",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(string[]), typeof(bool) },
                null
            );
            if (methodInfo == null) return;
            object[] parameters = new object[] { new[] { path }, false };
            methodInfo.Invoke(projectWindow, parameters);
        }
    }
}