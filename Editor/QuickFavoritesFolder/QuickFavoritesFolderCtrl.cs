using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuickFavoritesFolder {
    public class QuickFavoritesFolderCtrl {
        public static QuickFavoritesFolderModel Model;
        public List<FavoritesGroupView> Groups;
        
        public QuickFavoritesFolderCtrl() {
            Model = new QuickFavoritesFolderModel();
            Model.Load();
        }

        #region Group操作
        public static FavoritesGroupView GroupDataToView(FavoritesGroupData data) {
            if (data == null) {
                return null;
            }
            FavoritesGroupView view = new FavoritesGroupView {
                name = data.name,
                items = new List<FavoritesItemView>()
            };
            foreach (var item in data.items) {
                var itemView = ItemDataToView(item);
                if (itemView == null) continue;
                itemView.groupName = view.name;
                view.items.Add(itemView);
            }
            return view;
        }
        
        public static FavoritesGroupData GroupViewToData(FavoritesGroupView view) {
            if (view == null) {
                return null;
            }
            FavoritesGroupData data = new FavoritesGroupData {
                name = view.name,
                items = new List<FavoritesItemData>()
            };
            foreach (var item in view.items) {
                var itemData = ItemViewToData(item);
                if (itemData != null) {
                    data.items.Add(itemData);
                }
            }
            return data;
        }
        
        public void UpdateGroups(SortOption sortOption) {
            Groups = new List<FavoritesGroupView>();
            foreach (var groupData in Model.Groups) {
                var groupView = GroupDataToView(groupData);
                SortGroup(groupView, sortOption);
                Groups.Add(groupView);
            }
        }

        public FavoritesGroupView FindGroupView(string groupName) {
            var group = Groups.Find(g => g.name == groupName);
            return group;
        }
        
        public FavoritesGroupView AddGroup(string groupName) {
            if (Groups.Any(g => g.name == groupName)) {
                Debug.LogError($"QuickFavoritesFolder>AddGroup>Group of the same name[{groupName}] already exists!");
                return null;
            }

            var newGroupData = Model.AddNewGroup(groupName);
            if (newGroupData == null) {
                return null;
            }

            FavoritesGroupView newGroupView = GroupDataToView(newGroupData);
            Groups.Add(newGroupView);
            return newGroupView;
        }
        
        public void RenameGroup(FavoritesGroupView group, string newName) {
            if (string.IsNullOrEmpty(newName)) return;
            if (!Model.RenameGroup(group.name, newName)) return;
            group.name = newName;
            foreach (var item in group.items) {
                item.groupName = newName;
            }
        }
        
        public void DeleteGroup(FavoritesGroupView group) {
            if (Model.DeleteGroup(group.name)) {
                Groups.Remove(group);
            }
        }
        #endregion

        #region Item操作
        public static FavoritesItemView ItemDataToView(FavoritesItemData data) {
            if (data == null) {
                return null;
            }
            string assetPath = AssetDatabase.GUIDToAssetPath(data.guid);
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            FavoritesItemView view = ObjectToItemView(obj);
            return view;
        }
        
        public static FavoritesItemData ItemViewToData(FavoritesItemView view) {
            if (view == null) {
                return null;
            }

            FavoritesItemData data = new FavoritesItemData {
                guid = view.guid,
                name = view.name
            };
            return data;
        }

        public static FavoritesItemData ObjectToItemData(Object obj) {
            if (obj == null) {
                return null;
            }
            
            string path = AssetDatabase.GetAssetPath(obj);
            string objGuid = AssetDatabase.AssetPathToGUID(path);

            var data = new FavoritesItemData() {
                guid = objGuid,
                name = obj.name
            };
            return data;
        }

        public static FavoritesItemView ObjectToItemView(Object obj) {
            if (obj == null) {
                return null;
            }
            string path = AssetDatabase.GetAssetPath(obj);
            string objGuid = AssetDatabase.AssetPathToGUID(path);
            
            // 获取文件信息
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists) {
                Debug.LogError("File does not exist: " + path);
                return null;
                
            } 
            var view = new FavoritesItemView {
                obj = obj,
                guid = objGuid,
                name = obj.name,
                size = fileInfo.Length,
                type = fileInfo.Extension,
                lastAccessTime = fileInfo.LastAccessTimeUtc.Ticks,
                lastModifiedTime = fileInfo.LastWriteTimeUtc.Ticks
            };
            return view;
        }

        private void SortGroup(FavoritesGroupView group, SortOption sortOption) {
            
        }
        
        public void AddItem(Object obj, string groupName, int index = -1) {
            if (obj == null) {
                Debug.LogError("QuickFavoritesFolder>AddItem>Added Items cannot be null!");
                return;
            }
            var targetGroupView = FindGroupView(groupName) ?? AddGroup(groupName);
            string path = AssetDatabase.GetAssetPath(obj);
            string objGuid = AssetDatabase.AssetPathToGUID(path);
            if (targetGroupView.items.Any(i => i.guid == objGuid)) {
                Debug.LogError($"QuickFavoritesFolder>AddItem>Item [{obj.name}] with GUID {objGuid} already exists in group {groupName}!");
                return;
            }
            
            FavoritesItemData itemData = ObjectToItemData(obj);
            bool isSuccesses = Model.AddItem(itemData, groupName, index);
            if (!isSuccesses) {
                return;
            }
            FavoritesItemView itemView = ItemDataToView(itemData);
            itemView.groupName = groupName;
            index = index == -1 ? targetGroupView.items.Count : index;
            targetGroupView.items.Insert(index, itemView);
        }

        public void ChangeItemOrderInGroup(FavoritesGroupView group, FavoritesItemView item, int newOrder, bool isReverse) {
            if (item == null) {
                Debug.LogError("QuickFavoritesFolder>ChangeItemOrder>Item cannot be null!");
                return;
            }
            if (group == null) {
                Debug.LogError($"QuickFavoritesFolder>ChangeItemOrder>Group[{item.groupName}] not exist!");
                return;
            }

            FavoritesGroupData groupData = Model.FindGroup(group.name);
            FavoritesItemData itemData = Model.FindItem(groupData, item.guid);
            bool isSuccesses = Model.RemoveItem(itemData, groupData);
            if (!isSuccesses) {
                return;
            }
            isSuccesses = Model.AddItem(itemData, groupData, newOrder);
            if (!isSuccesses) {
                return;
            }

            group.items.Remove(item);
            newOrder = newOrder == -1 ? group.items.Count : newOrder;
            group.items.Insert(newOrder, item);
        }

        public void ChangeItemGroup(FavoritesItemView item, string targetGroupName, int index = -1) {
            if (item == null) {
                Debug.LogError("QuickFavoritesFolder>ChangeItemGroup>Item cannot be null!");
                return;
            }
            var targetGroupView = FindGroupView(targetGroupName) ?? AddGroup(targetGroupName);
            if (targetGroupView.items.Any(i => i.guid == item.guid)) {
                Debug.LogError($"QuickFavoritesFolder>ChangeItemGroup>Item [{item.name}] with GUID {item.guid} already exists in group {targetGroupName}!");
                return;
            }
            var oldGroupView = FindGroupView(item.groupName);

            FavoritesItemData targetItemData = Model.FindItem(item.groupName, item.guid);
            FavoritesGroupData targetGroupData = Model.FindGroup(targetGroupName);
            FavoritesGroupData oldGroupData = Model.FindGroup(item.groupName);
            bool isSuccesses = Model.AddItem(targetItemData, targetGroupData, index);
            if (!isSuccesses) {
                return;
            }
            isSuccesses = Model.RemoveItem(targetItemData, oldGroupData);
            if (!isSuccesses) {
                return;
            }
            
            index = index == -1 ? targetGroupView.items.Count : index;
            targetGroupView.items.Insert(index, item);
            oldGroupView.items.Remove(item);
            item.groupName = targetGroupName;
            SortGroup(targetGroupView, QuickFavoritesFolderView.SelectedSortOption);
        }
        
        public void RemoveItem(FavoritesItemView item) {
            if (item == null) {
                Debug.LogError("QuickFavoritesFolder>AddItem>Remove Item cannot be null!");
                return;
            }
            bool isSuccesses = Model.RemoveItem(item.guid, item.groupName);
            if (!isSuccesses) {
                return;
            }
            var groupView = FindGroupView(item.groupName);
            groupView.items.Remove(item);
        }
        #endregion
    }
}