using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorProjectExtension.ReferenceFinder {
    public class QuickReferenceFinderView : EditorWindow {
        [MenuItem("Assets/Find References", false)]
        private static void FindObjectReferences() {
            StartNewReferencesWindow(Selection.activeObject);
        }

        private static void StartNewReferencesWindow(Object targetObj) {
            if (targetObj == null) {
                return;
            }
            var window = CreateInstance<QuickReferenceFinderView>();  // 创建新窗口实例
            window.titleContent = new GUIContent($"Find {targetObj.name} References");  // 设置窗口标题
            window.Show();  // 显示新窗口
            window.FindObjectReferences(targetObj);
        }
        
        private QuickReferenceFinderCtrl _ctrl;
        private Object _findReferencesAfterLayout;
        private Vector2 _scrollPosition = Vector2.zero;

        private void OnGUI() {
            GUILayout.Space(5);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target object:");
            EditorGUILayout.ObjectField(_ctrl.TargetObject, typeof(Object), false);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("References found: " + _ctrl.ReferencesCount);
            if (GUILayout.Button("Clear", EditorStyles.miniButton)) {
                _ctrl.References.Clear();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            foreach (var reference in _ctrl.References) {
                OnItemGUI(reference);
            }

            EditorGUILayout.EndScrollView();

            if (_findReferencesAfterLayout == null) {
                return;
            }
            StartNewReferencesWindow(_findReferencesAfterLayout);
            _findReferencesAfterLayout = null;
        }

        private void OnItemGUI(Object obj) {
            var style = EditorStyles.miniButtonLeft;
            style.alignment = TextAnchor.MiddleLeft;

            if (obj == null) {
                return;
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(obj.name, style)) {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            if (GUILayout.Button("\u25B6", EditorStyles.miniButtonRight, GUILayout.MaxWidth(20))) {
                _findReferencesAfterLayout = obj;// Use "right arrow" unicode character 
            }

            GUILayout.EndHorizontal();
        }

        private void FindObjectReferences(Object targetObj) {
            _ctrl ??= new QuickReferenceFinderCtrl();
            _ctrl.References.Clear();
            _ctrl.FindObjectReferences(targetObj);
        }
    }
}