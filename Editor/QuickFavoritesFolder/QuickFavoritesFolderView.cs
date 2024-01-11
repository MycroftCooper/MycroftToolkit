using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace QuickFavoritesFolder {
    public enum SortOption { CustomOrder, Name, Size, FileType }
    
    [Serializable]
    public class FavoritesItemView {
        public Object obj;
        public string guid;
        public string name;
        public string groupName;
        public long size;
        public string type;
        public long lastAccessTime;
        public long lastModifiedTime;
    }

    [Serializable]
    public class FavoritesGroupView {
        public string name;
        public List<FavoritesItemView> items;
    }
    
    public class QuickFavoritesFolderView : EditorWindow {
        public static QuickFavoritesFolderCtrl Ctrl;
        
        public static SortOption SelectedSortOption = SortOption.CustomOrder;
        public static bool IsOrderReverse; 
        private bool _isShowFileSize;
        private bool _isShowFileType;
        private bool _isShowLastAccessTime;
        private bool _isShowLastModifiedTime;
        private string _searchString = "";
        
        public readonly Dictionary<string, bool> IsGroupFoldout = new Dictionary<string, bool>();
        
        [MenuItem("Window/QuickFavoritesFolder")]
        public static void ShowWindow() {
            Ctrl ??= new QuickFavoritesFolderCtrl();
            Ctrl.UpdateGroups(SelectedSortOption);
            GetWindow<QuickFavoritesFolderView>("QuickFavoritesFolder");
        }

        private void OnGUI() {
            GUILayout.BeginHorizontal();
            SelectedSortOption = (SortOption)EditorGUILayout.EnumPopup("Sort By", SelectedSortOption);
            IsOrderReverse = EditorGUILayout.ToggleLeft("Is Order Reverse", IsOrderReverse, GUILayout.Width(120));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _isShowFileType = EditorGUILayout.ToggleLeft("Show File Type", _isShowFileType);
            _isShowFileSize = EditorGUILayout.ToggleLeft("Show File Size", _isShowFileSize);
            _isShowLastAccessTime = EditorGUILayout.ToggleLeft("Show Last Access Time", _isShowLastAccessTime);
            _isShowLastModifiedTime = EditorGUILayout.ToggleLeft("Show Last Modified Time", _isShowLastModifiedTime);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            _searchString = EditorGUILayout.TextField("Search", _searchString);
            if (GUILayout.Button("X", GUILayout.Width(20))) {
                OnCancelSearchBtnClick();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
            
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag Objects Here to Create New Group");
            OnItemDrop(dropArea, null);

            OnColumnHeadGUI();

            for (var index = 0; index < Ctrl.Groups.Count; index++) {
                var group = Ctrl.Groups[index];
                OnGroupGUI(group);
            }
        }

        private void OnColumnHeadGUI() {
            if (!_isShowFileSize && !_isShowFileType && !_isShowLastAccessTime && !_isShowLastModifiedTime) return;
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(10));
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("", GUILayout.Width(20));// 空出来对齐拖拽按钮
            GUILayout.Label("Item", EditorStyles.boldLabel);
            if (_isShowFileType) {
                GUILayout.Label(" | Type", EditorStyles.boldLabel, GUILayout.Width(100));
            }
            if (_isShowFileSize) {
                GUILayout.Label(" | Size", EditorStyles.boldLabel, GUILayout.Width(80));
            }
            if (_isShowLastAccessTime) {
                GUILayout.Label(" | Last Access", EditorStyles.boldLabel, GUILayout.Width(120));
            }
            if (_isShowLastModifiedTime) {
                GUILayout.Label(" | Last Modified", EditorStyles.boldLabel, GUILayout.Width(120));
            }

            // 留出空间以对齐“Delete”按钮
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.EndHorizontal();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
        }
        
        private void OnCancelSearchBtnClick() {
            _searchString = "";
        }
        
        #region Group相关
        private void OnGroupGUI(FavoritesGroupView group) {
            if (!IsGroupFoldout.ContainsKey(group.name)) {
                IsGroupFoldout[group.name] = true;
            }
            
            Rect groupRect = EditorGUILayout.BeginVertical();// 开始渲染分组区域
            
            GUILayout.BeginHorizontal();
            if (_groupBeingRenamed == group.name) {
                // 如果这个组正在被重命名，显示文本框
                _newGroupName = EditorGUILayout.TextField(_newGroupName, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("OK", GUILayout.Width(80)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
                    RenameGroup(group, _newGroupName);
                }
            } else {
                // 显示分组的Foldout
                IsGroupFoldout[group.name] = EditorGUILayout.Foldout(IsGroupFoldout[group.name], group.name, true);
                if (_groupBeingRenamed == null) {
                    // 显示重命名按钮
                    if (GUILayout.Button("Rename", GUILayout.Width(80))) {
                        _groupBeingRenamed = group.name; // 激活重命名状态
                    }
                }
            }
            
            if (GUILayout.Button("X", GUILayout.Width(20))) {
                OnDeleteGroupBtnClick(group);
            }
            GUILayout.EndHorizontal();
            
            // 绘制一个透明的背景框，使其覆盖整个分组区域
            if (Event.current.type == EventType.Repaint) {
                GUI.Box(groupRect, GUIContent.none, EditorStyles.helpBox);
            }

            if (IsGroupFoldout[group.name]) {
                if (group.items.Count == 0) {
                    GUILayout.Space(50); // 提供足够的空间用于拖拽
                } else {
                    for (var index = 0; index < group.items.Count; index++) {
                        var item = group.items[index];
                        OnInsertGUI(group, index);
                        OnItemGUI(item);
                    }
                    OnInsertGUI(group, group.items.Count);
                }
                
            }
            OnItemDrop(groupRect, group);
            
            EditorGUILayout.EndVertical();// 结束分组区域渲染
            
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
        }

        private void OnInsertGUI(FavoritesGroupView group, int index) {
            var itemRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(2));
            if (Event.current.type == EventType.Repaint && DragAndDrop.visualMode == DragAndDropVisualMode.Move && itemRect.Contains(Event.current.mousePosition)) {
                // 绘制一个视觉反馈
                Handles.DrawLine(new Vector2(itemRect.x, itemRect.yMax), new Vector2(itemRect.xMax, itemRect.yMax));
            }
            OnItemDrop(itemRect, group, index);
        }
        
        private string _groupBeingRenamed;
        private string _newGroupName;
        private void RenameGroup(FavoritesGroupView group, string newName) {
            _groupBeingRenamed = null; // 重命名完成
            _newGroupName = ""; // 重置临时变量
            
            if (string.IsNullOrEmpty(newName)) return;
            
            bool isFoldOut = IsGroupFoldout[group.name];
            IsGroupFoldout.Remove(group.name);
            Ctrl.RenameGroup(group, newName);
            IsGroupFoldout.Add(group.name, isFoldOut);
        }
        
        private void OnDeleteGroupBtnClick(FavoritesGroupView group) {
            if (EditorUtility.DisplayDialog("Confirm Delete Group", 
                    $"Are you sure you want to delete the group '{group.name}'?", "Delete", "Cancel")) {
                Ctrl.DeleteGroup(group);
            }
        }
        #endregion

        #region Item相关
        private void OnItemGUI(FavoritesItemView item) {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(item.guid));
            if (obj == null) return;
            
            GUILayout.BeginHorizontal();

            GUIStyle dragHandleStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 10, // 设置固定宽度
                fixedHeight = EditorGUIUtility.singleLineHeight // 设置固定高度
            };
            Rect labelRect = GUILayoutUtility.GetRect(new GUIContent("="), dragHandleStyle);
            EditorGUI.LabelField(labelRect, "=");
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Pan);
            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition)) {
                OnItemDrag(item);
            }
            
            EditorGUILayout.ObjectField(obj, typeof(Object), false);
            
            OnItemInfoGUI(item);
            
            if (GUILayout.Button("X", GUILayout.Width(20))) {
                OnRemoveItemBtnClick(item);
            }
            
            GUILayout.EndHorizontal();
        }

        private void OnItemInfoGUI(FavoritesItemView item) {
            if (_isShowFileType) {
                GUILayout.Label(" | " +item.type, GUILayout.Width(100));
            }
            if (_isShowFileSize) {
                GUILayout.Label( " | " +FormatSize(item.size), GUILayout.Width(80));
            }
            if (_isShowLastAccessTime) {
                GUILayout.Label(" | " +FormatDateTime(new DateTime(item.lastAccessTime)), GUILayout.Width(120));
            }
            if (_isShowLastModifiedTime) {
                GUILayout.Label(" | " +FormatDateTime(new DateTime(item.lastModifiedTime)), GUILayout.Width(120));
            }
        }

        private void OnItemDrag(FavoritesItemView item) {// 拖拽处理
            var dragRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type != EventType.MouseDown || !dragRect.Contains(Event.current.mousePosition)) return;
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new [] { item.obj };
            DragAndDrop.SetGenericData("DraggingItem", item);
            DragAndDrop.StartDrag(item.name);
            Event.current.Use();
        }
        
        private void OnItemDrop(Rect dropArea, FavoritesGroupView group, int index = -1) {
            Event currentEvent = Event.current;
            EventType currentEventType = currentEvent.type;

            if (!dropArea.Contains(currentEvent.mousePosition))
                return;

            string groupName = group == null ? "Default" : group.name;
            int targetIndex = index;
            switch (currentEventType) {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    currentEvent.Use();
                    break;
                case EventType.DragPerform:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.AcceptDrag();
                    currentEvent.Use();
                    if (DragAndDrop.GetGenericData("DraggingItem") is FavoritesItemView dragData) {
                        if (index != -1 && group != null) {
                            targetIndex = GetInsertionIndex(dropArea, group, dragData, index);
                        }
                        if (groupName != dragData.groupName) {
                            Ctrl.ChangeItemGroup(dragData, groupName, targetIndex);
                        } else {
                            Ctrl.ChangeItemOrderInGroup(group, dragData, targetIndex, false);
                        }
                        
                    } else {
                        if (index != -1 && group != null) {
                            targetIndex = GetInsertionIndex(dropArea, group, null, index);
                        }

                        for (int i = DragAndDrop.objectReferences.Length -1; i >= 0; i--) {
                            var draggedObject = DragAndDrop.objectReferences[i];
                            Ctrl.AddItem(draggedObject, groupName, targetIndex); // 添加到指定分组
                        }
                    }
                    break;
            }
        }
        
        private int GetInsertionIndex(Rect itemRect, FavoritesGroupView group, FavoritesItemView draggedItem, int currentIndex) {
            var mousePositionY = Event.current.mousePosition.y;// 获取鼠标位置
            var halfHeight = itemRect.height / 2;// 基于鼠标位置判断应该插入到当前索引之前还是之后
            var insertionIndex = mousePositionY > (itemRect.y + halfHeight) ? currentIndex + 1 : currentIndex;
            insertionIndex = Mathf.Clamp(insertionIndex, 0, group.items.Count);// 确保新的索引不会超出列表范围
            if (draggedItem != null && group.items.Contains(draggedItem) && group.items.IndexOf(draggedItem) < currentIndex) {// 如果拖拽的项原来就在当前项之前，需要调整新的索引
                insertionIndex--;
            }
            return insertionIndex;
        }
        
        private void OnRemoveItemBtnClick(FavoritesItemView item) {
            if (EditorUtility.DisplayDialog("Confirm Delete Item", 
                    $"Are you sure you want to remove the item '{item.name}'?", "Remove", "Cancel")) {
                Ctrl.RemoveItem(item);
            }
        }
        #endregion
        
        private string FormatSize(long bytes) {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1) {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string FormatDateTime(DateTime dateTime) {
            return dateTime.ToString("g"); // 使用短日期格式和短时间格式
        }
    }
}

