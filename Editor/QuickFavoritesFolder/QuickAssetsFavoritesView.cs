using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace QuickFavorites.Assets {
    public enum SortOption { CustomOrder, Name, Size, FileType, LastAccessTime, LastModifiedTime}
    
    [Flags] // 允许枚举值组合
    public enum AssetFilterOptions {
        None = 0,
        Everything = ~0, // 所有选项
        AnimationClip = 1 << 0,
        AudioClip = 1 << 1,
        AudioMixer = 1 << 2,
        ComputeShader = 1 << 3,
        Font = 1 << 4,
        GUISkin = 1 << 5,
        Material = 1 << 6,
        Mesh = 1 << 7,
        Model = 1 << 8,
        PhysicMaterial = 1 << 9,
        Prefab = 1 << 10,
        Scene = 1 << 11,
        Script = 1 << 12,
        Shader = 1 << 13,
        Sprite = 1 << 14,
        Texture = 1 << 15,
        VideoClip = 1 << 16,
    }
    
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
    
    public class QuickAssetsFavoritesView : EditorWindow {
        public static QuickAssetsFavoritesCtrl Ctrl => ctrl ??= new QuickAssetsFavoritesCtrl();
        private static QuickAssetsFavoritesCtrl ctrl;
        
        public static AssetFilterOptions FilterOptions = AssetFilterOptions.Everything;
        public static SortOption SelectedSortOption = SortOption.CustomOrder;
        private static SortOption currentSortOption = SortOption.CustomOrder;
        
        public static bool IsOrderReverse; 
        private static bool isLock;
        
        private bool _isShowFileSize;
        private bool _isShowFileType;
        private bool _isShowLastAccessTime;
        private bool _isShowLastModifiedTime;
        private static readonly GUILayoutOption ToggleWidth = GUILayout.Width(120);
        
        private static string searchString = "";
        
        private static bool CanDragItem => !isLock && SelectedSortOption == SortOption.CustomOrder;
        public readonly Dictionary<string, bool> IsGroupFoldout = new Dictionary<string, bool>();
        
        [MenuItem("Window/QuickAssetsFavorites")]
        public static void ShowWindow() {
            ctrl ??= new QuickAssetsFavoritesCtrl();
            GetWindow<QuickAssetsFavoritesView>("QuickAssetsFavorites");
        }
        
        [MenuItem("Assets/Add To QuickAssetsFavorites")]
        private static void AddToQuickFavorites() {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return;
            Ctrl.AddItem(selectedObject, "Default");
        }

        // 该选项是否可用的逻辑
        [MenuItem("Assets/Add To QuickAssetsFavorites", true)]
        private static bool AddToQuickFavoritesValidate() {
            return Selection.activeObject != null;
        }
        
        private void Awake() {
            minSize = new Vector2(500, 400);
        }

        private void OnGUI() {
            GUILayout.BeginHorizontal();
            SelectedSortOption = (SortOption)EditorGUILayout.EnumPopup("Sort By", SelectedSortOption);
            IsOrderReverse = EditorGUILayout.ToggleLeft("IsReverse", IsOrderReverse, GUILayout.Width(80));
            isLock = EditorGUILayout.ToggleLeft("IsLock", isLock, GUILayout.Width(80));
            GUILayout.EndHorizontal();

            UpdateSort();
            
            GUILayout.BeginHorizontal();
            _isShowFileType = EditorGUILayout.ToggleLeft("Type", _isShowFileType, ToggleWidth);
            _isShowFileSize = EditorGUILayout.ToggleLeft("Size", _isShowFileSize, ToggleWidth);
            _isShowLastAccessTime = EditorGUILayout.ToggleLeft("LastAccessTime", _isShowLastAccessTime, ToggleWidth);
            _isShowLastModifiedTime = EditorGUILayout.ToggleLeft("LastModifiedTime", _isShowLastModifiedTime, ToggleWidth);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            searchString = EditorGUILayout.TextField("Search", searchString);
            if (GUILayout.Button("X", GUILayout.Width(20))) {
                OnCancelSearchBtnClick();
            }
            GUILayout.EndHorizontal();
            FilterOptions = (AssetFilterOptions)EditorGUILayout.EnumFlagsField("Filter Type", FilterOptions);
            
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
            
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag Objects Here to Create New Group");
            OnItemDrop(dropArea, null);

            OnTableHeadGUI();

            for (var index = 0; index < Ctrl.Groups.Count; index++) {
                OnInsertGroupGUI(index);
                var group = Ctrl.Groups[index];
                OnGroupGUI(group);
            }
            OnInsertGroupGUI(Ctrl.Groups.Count);
        }

        private void OnTableHeadGUI() {
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
            searchString = "";
        }
        
        #region Group相关
        private void OnGroupGUI(FavoritesGroupView group) {
            if (!IsGroupFoldout.ContainsKey(group.name)) {
                IsGroupFoldout.Add(group.name, true);
            }
            
            Rect groupRect = EditorGUILayout.BeginVertical();// 开始渲染分组区域
            
            GUILayout.BeginHorizontal();

            if (!isLock) {
                GUIStyle dragHandleStyle = new GUIStyle(GUI.skin.label) {
                    alignment = TextAnchor.MiddleCenter,
                    fixedWidth = 10, // 设置固定宽度
                    fixedHeight = EditorGUIUtility.singleLineHeight // 设置固定高度
                };
                Rect labelRect = GUILayoutUtility.GetRect(new GUIContent("≡"), dragHandleStyle);
                EditorGUI.LabelField(labelRect, "≡");
                EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Pan);
                if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition)) {
                    OnGroupDrag(group);
                }
            }

            if (_groupBeingRenamed == group.name && !isLock) {
                // 如果这个组正在被重命名，显示文本框
                _newGroupName = EditorGUILayout.TextField(_newGroupName, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("OK", GUILayout.Width(80)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
                    RenameGroup(group, _newGroupName);
                }
            } else {
                // 显示分组的Foldout
                IsGroupFoldout[group.name] = EditorGUILayout.Foldout(IsGroupFoldout[group.name], group.name, true);
                if (_groupBeingRenamed == null && !isLock) {
                    // 显示重命名按钮
                    if (GUILayout.Button("Rename", GUILayout.Width(80))) {
                        _groupBeingRenamed = group.name; // 激活重命名状态
                    }
                }
            }

            if (!isLock) {
                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    OnDeleteGroupBtnClick(group);
                }
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
                    for (var index = IsOrderReverse? group.items.Count - 1 : 0; 
                         IsOrderReverse? index >= 0 : index < group.items.Count; 
                         index = IsOrderReverse? index - 1: index + 1) {
                        var item = group.items[index];
                        OnInsertItemGUI(group, index);
                        OnItemGUI(item);
                    }
                    OnInsertItemGUI(group, group.items.Count);
                }
                
            }
            OnItemDrop(groupRect, group);
            
            EditorGUILayout.EndVertical();// 结束分组区域渲染
            
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
        }

        private void OnInsertGroupGUI(int index) {
            var groupRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(4));
            if (Event.current.type == EventType.Repaint && DragAndDrop.visualMode == DragAndDropVisualMode.Move && groupRect.Contains(Event.current.mousePosition)) {
                Handles.DrawLine(new Vector2(groupRect.x, groupRect.yMax), new Vector2(groupRect.xMax, groupRect.yMax));
            }
            OnGroupDrop(groupRect, index);
        }

        private void OnGroupDrag(FavoritesGroupView group) {// 拖拽处理
            var dragRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type != EventType.MouseDown || !dragRect.Contains(Event.current.mousePosition)) return;
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = null;
            DragAndDrop.SetGenericData("DraggingGroup", group);
            DragAndDrop.StartDrag(group.name);
            Event.current.Use();
        }
        
        private void OnGroupDrop(Rect dropArea, int index = -1) {
            Event currentEvent = Event.current;
            EventType currentEventType = currentEvent.type;

            if (!dropArea.Contains(currentEvent.mousePosition))
                return;
            switch (currentEventType) {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    currentEvent.Use();
                    break;
                case EventType.DragPerform:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.AcceptDrag();
                    currentEvent.Use();
                    FavoritesGroupView group = DragAndDrop.GetGenericData("DraggingGroup") as FavoritesGroupView;
                    int targetIndex = index;
                    if (index != -1 && group != null) {
                        targetIndex = GetInsertionIndex(dropArea, Ctrl.Groups,group, index);
                    }
                    Ctrl.ChangeGroupOrderInGroup(group, targetIndex, false); // 添加到指定分组
                    break;
            }
        }

        private void UpdateSort() {
            if (currentSortOption == SelectedSortOption) {
                return;
            }
            for (int i = 0; i < Ctrl.Groups.Count; i++) {
                Ctrl.SortGroup(Ctrl.Groups[i], SelectedSortOption);
            }
            currentSortOption = SelectedSortOption;
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
            if (!IsItemCanShow(item, FilterOptions)) {
                return;
            }
            GUILayout.BeginHorizontal();

            if (CanDragItem) {
                GUIStyle dragHandleStyle = new GUIStyle(GUI.skin.label) {
                    alignment = TextAnchor.MiddleCenter,
                    fixedWidth = 10, // 设置固定宽度
                    fixedHeight = EditorGUIUtility.singleLineHeight // 设置固定高度
                };
                Rect labelRect = GUILayoutUtility.GetRect(new GUIContent("≡"), dragHandleStyle);
                EditorGUI.LabelField(labelRect, "≡");
                EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Pan);
                if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition)) {
                    OnItemDrag(item);
                }
            }
            
            EditorGUILayout.ObjectField(item.obj, typeof(Object), false);
            
            OnItemInfoGUI(item);

            if (!isLock) {
                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    OnRemoveItemBtnClick(item);
                }
            }
            
            GUILayout.EndHorizontal();
        }

        private void OnItemInfoGUI(FavoritesItemView item) {
            if (_isShowFileType) {
                GUILayout.Label(" | " + item.type, GUILayout.Width(100));
            }
            if (_isShowFileSize) {
                GUILayout.Label( " | " + FormatSize(item.size), GUILayout.Width(80));
            }
            if (_isShowLastAccessTime) {
                GUILayout.Label(" | " + FormatDateTime(new DateTime(item.lastAccessTime)), GUILayout.Width(120));
            }
            if (_isShowLastModifiedTime) {
                GUILayout.Label(" | " + FormatDateTime(new DateTime(item.lastModifiedTime)), GUILayout.Width(120));
            }
        }
        
        private void OnInsertItemGUI(FavoritesGroupView group, int index) {
            var itemRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(2));
            if (Event.current.type == EventType.Repaint && DragAndDrop.visualMode == DragAndDropVisualMode.Move && itemRect.Contains(Event.current.mousePosition)) {
                // 绘制一个视觉反馈
                Handles.DrawLine(new Vector2(itemRect.x, itemRect.yMax), new Vector2(itemRect.xMax, itemRect.yMax));
            }
            OnItemDrop(itemRect, group, index);
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
                            targetIndex = GetInsertionIndex(dropArea, group.items, dragData, index, IsOrderReverse);
                        }
                        if (groupName != dragData.groupName) {
                            Ctrl.ChangeItemGroup(dragData, groupName, targetIndex);
                        } else {
                            Ctrl.ChangeItemOrderInGroup(group, dragData, targetIndex, false);
                        }
                        
                    } else {
                        if (index != -1 && group != null) {
                            targetIndex = GetInsertionIndex(dropArea, group.items, null, index, IsOrderReverse);
                        }

                        for (int i = DragAndDrop.objectReferences.Length -1; i >= 0; i--) {
                            var draggedObject = DragAndDrop.objectReferences[i];
                            Ctrl.AddItem(draggedObject, groupName, targetIndex); // 添加到指定分组
                        }
                    }
                    break;
            }
        }
        
        private int GetInsertionIndex<T>(Rect rect, List<T> list, T item, int currentIndex, bool isReverse = false) {
            if (isReverse) {
                currentIndex = list.Count - currentIndex;
            }
            var mousePositionY = Event.current.mousePosition.y;// 获取鼠标位置
            var halfHeight = rect.height / 2;// 基于鼠标位置判断应该插入到当前索引之前还是之后
            var insertionIndex = mousePositionY > (rect.y + halfHeight) ? currentIndex + 1 : currentIndex;
            insertionIndex = Mathf.Clamp(insertionIndex, 0, list.Count);// 确保新的索引不会超出列表范围
            if (item != null && list.Contains(item) && list.IndexOf(item) < currentIndex) {// 如果拖拽的项原来就在当前项之前，需要调整新的索引
                insertionIndex--;
            }
            insertionIndex = Mathf.Clamp(insertionIndex, 0, list.Count);// 确保新的索引不会超出列表范围
            return insertionIndex;
        }
        
        private void OnRemoveItemBtnClick(FavoritesItemView item) {
            if (EditorUtility.DisplayDialog("Confirm Delete Item", 
                    $"Are you sure you want to remove the item '{item.name}'?", "Remove", "Cancel")) {
                Ctrl.RemoveItem(item);
            }
        }
        
        private static string FormatSize(long bytes) {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1) {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string FormatDateTime(DateTime dateTime) {
            return dateTime.ToString("g"); // 使用短日期格式和短时间格式
        }

        private static bool IsItemCanShow(FavoritesItemView itemView, AssetFilterOptions filterOptions) {
            if (!string.IsNullOrEmpty(searchString) && !itemView.name.Contains(searchString)) {
                return false;
            }
            
            // 如果选择“All”，则显示所有资源
            if (filterOptions == AssetFilterOptions.Everything) return true;
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GUIDToAssetPath(itemView.guid));
            if (assetType == typeof(AnimationClip) && filterOptions.HasFlag(AssetFilterOptions.AnimationClip)) return true;
            if (assetType == typeof(AudioClip) && filterOptions.HasFlag(AssetFilterOptions.AudioClip)) return true;
            if (assetType == typeof(AudioMixer) && filterOptions.HasFlag(AssetFilterOptions.AudioMixer)) return true;
            if (assetType == typeof(ComputeShader) && filterOptions.HasFlag(AssetFilterOptions.ComputeShader)) return true;
            if (assetType == typeof(Font) && filterOptions.HasFlag(AssetFilterOptions.Font)) return true;
            if (assetType == typeof(GUISkin) && filterOptions.HasFlag(AssetFilterOptions.GUISkin)) return true;
            if (assetType == typeof(Material) && filterOptions.HasFlag(AssetFilterOptions.Material)) return true;
            if (assetType == typeof(Mesh) && filterOptions.HasFlag(AssetFilterOptions.Mesh)) return true;
            if (itemView.type == ".fbx" && filterOptions.HasFlag(AssetFilterOptions.Model)) return true;
            if (assetType == typeof(PhysicMaterial) && filterOptions.HasFlag(AssetFilterOptions.PhysicMaterial)) return true;
            if (assetType == typeof(SceneAsset) && filterOptions.HasFlag(AssetFilterOptions.Scene)) return true;
            if (assetType == typeof(MonoScript) && filterOptions.HasFlag(AssetFilterOptions.Script)) return true;
            if (assetType == typeof(Shader) && filterOptions.HasFlag(AssetFilterOptions.Shader)) return true;
            if (assetType == typeof(Sprite) && filterOptions.HasFlag(AssetFilterOptions.Sprite)) return true;
            if (assetType == typeof(Texture) && filterOptions.HasFlag(AssetFilterOptions.Texture)) return true;
            if (assetType == typeof(VideoClip) && filterOptions.HasFlag(AssetFilterOptions.VideoClip)) return true;
            return false;
        }

        #endregion
    }
}

