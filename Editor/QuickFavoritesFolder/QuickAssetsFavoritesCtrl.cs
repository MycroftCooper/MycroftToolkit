using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuickFavorites.Assets {
    public class QuickAssetsFavoritesCtrl {
        public static QuickAssetsFavoritesModel Model;
        public List<FavoritesGroupView> Groups;
        
        public QuickAssetsFavoritesCtrl() {
            Model = new QuickAssetsFavoritesModel();
            Model.Load();
            UpdateGroups(QuickAssetsFavoritesView.SelectedSortOption);
        }

        #region Group操作
        public static FavoritesGroupView GroupDataToView(FavoritesGroupData data) {
            if (data == null) {
                return null;
            }
            FavoritesGroupView view = new FavoritesGroupView {
                Name = data.name,
                Items = new List<FavoritesItemView>()
            };
            foreach (var item in data.items) {
                var itemView = ItemDataToView(item);
                if (itemView == null) continue;
                itemView.GroupName = view.Name;
                view.Items.Add(itemView);
            }
            return view;
        }
        
        public static FavoritesGroupData GroupViewToData(FavoritesGroupView view) {
            if (view == null) {
                return null;
            }
            FavoritesGroupData data = new FavoritesGroupData {
                name = view.Name,
                items = new List<FavoritesItemData>()
            };
            foreach (var item in view.Items) {
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
            var group = Groups.Find(g => g.Name == groupName);
            return group;
        }
        
        public void SortGroup(FavoritesGroupView group, SortOption sortOption) {
            switch (sortOption) {
                case SortOption.CustomOrder:
                    var itemViews = new List<FavoritesItemView>();
                    var oldItemViewsDict = group.Items.ToDictionary(item => item.Guid, item => item);
                    var groupData = Model.FindGroup(group.Name);
                    var itemDataList = groupData.items;
                    foreach (var itemData in itemDataList) {
                        string guid = itemData.guid;
                        var targetItemView = oldItemViewsDict[guid];
                        itemViews.Add(targetItemView);
                    }
                    group.Items = itemViews;
                    break;
                case SortOption.Name:
                    group.Items = group.Items.OrderBy(item => item.Name).ToList();
                    break;
                case SortOption.Size:
                    group.Items = group.Items.OrderBy(item => item.Size).ToList();
                    break;
                case SortOption.FileType:
                    group.Items = group.Items.OrderBy(item => item.Type).ToList();
                    break;
                case SortOption.Note:
                    group.Items = group.Items.OrderBy(item => item.Note).ToList();
                    break;
                case SortOption.LastAccessTime:
                    group.Items = group.Items.OrderBy(item => item.LastAccessTime).ToList();
                    break;
                case SortOption.LastModifiedTime:
                    group.Items = group.Items.OrderBy(item => item.LastModifiedTime).ToList();
                    break;
            }
        }
        
        public FavoritesGroupView AddGroup(string groupName) {
            if (Groups.Any(g => g.Name == groupName)) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>AddGroup", 
                    $"Group of the same name[{groupName}] already exists!", "OK");
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
            if (!Model.RenameGroup(group.Name, newName)) return;
            group.Name = newName;
            foreach (var item in group.Items) {
                item.GroupName = newName;
            }
        }
        
        public void DeleteGroup(FavoritesGroupView group) {
            if (Model.DeleteGroup(group.Name)) {
                Groups.Remove(group);
            }
        }
        
        public void ChangeGroupOrderInGroup(FavoritesGroupView group, int newOrder) {
            if (group == null) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>ChangeItemOrder", "Group cant be null!", "OK");
                return;
            }
            newOrder = newOrder == -1 ? Model.Groups.Count : newOrder;
            FavoritesGroupData groupData = Model.FindGroup(group.Name);
            bool isSuccesses = Model.ChangeGroupOrder(groupData, newOrder);
            if (!isSuccesses) {
                return;
            }
            Groups.Remove(group);
            Groups.Insert(newOrder, group);
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
            view.Note = data.note;
            return view;
        }
        
        public static FavoritesItemData ItemViewToData(FavoritesItemView view) {
            if (view == null) {
                return null;
            }

            FavoritesItemData data = new FavoritesItemData {
                guid = view.Guid,
                name = view.Name,
                note = view.Note
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
                name = obj.name,
                note = ""
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
            bool isDirectory = fileInfo.Attributes == FileAttributes.Directory;
            if (!isDirectory && !fileInfo.Exists) {
                Debug.LogError("File does not exist: " + path);
                return null;
            } 
            var view = new FavoritesItemView {
                Obj = obj,
                Guid = objGuid,
                Name = obj.name,
                Size = isDirectory? -1 : fileInfo.Length,
                Type = isDirectory? "directory" : fileInfo.Extension,
                LastAccessTime = fileInfo.LastAccessTimeUtc.Ticks,
                LastModifiedTime = fileInfo.LastWriteTimeUtc.Ticks
            };
            return view;
        }
        
        public void AddItem(Object obj, string groupName, int index = -1) {
            if (obj == null) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>AddItem", 
                    "Added Items cannot be null!", "OK");
                return;
            }
            var targetGroupView = FindGroupView(groupName) ?? AddGroup(groupName);
            string path = AssetDatabase.GetAssetPath(obj);
            string objGuid = AssetDatabase.AssetPathToGUID(path);
            if (targetGroupView.Items.Any(i => i.Guid == objGuid)) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>AddItem", 
                    $"Item [{obj.name}] with GUID {objGuid} already exists in group {groupName}!", "OK");
                return;
            }
            
            FavoritesItemData itemData = ObjectToItemData(obj);
            bool isSuccesses = Model.AddItem(itemData, groupName, index);
            if (!isSuccesses) {
                return;
            }
            FavoritesItemView itemView = ItemDataToView(itemData);
            itemView.GroupName = groupName;
            index = index == -1 ? targetGroupView.Items.Count : index;
            targetGroupView.Items.Insert(index, itemView);
            SortGroup(targetGroupView, QuickAssetsFavoritesView.SelectedSortOption);
        }

        public void ChangeItemOrderInGroup(FavoritesGroupView group, FavoritesItemView item, int newOrder) {
            if (item == null) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>ChangeItemOrder", 
                    "Item cannot be null!", "OK");
                return;
            }
            if (group == null) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>ChangeItemOrder", 
                    $"Group[{item.GroupName}] not exist!", "OK");
                return;
            }

            FavoritesGroupData groupData = Model.FindGroup(group.Name);
            FavoritesItemData itemData = Model.FindItem(groupData, item.Guid);
            bool isSuccesses = Model.RemoveItem(itemData, groupData);
            if (!isSuccesses) {
                return;
            }
            isSuccesses = Model.AddItem(itemData, groupData, newOrder);
            if (!isSuccesses) {
                return;
            }

            group.Items.Remove(item);
            newOrder = newOrder == -1 ? group.Items.Count : newOrder;
            group.Items.Insert(newOrder, item);
        }

        public void ChangeItemGroup(FavoritesItemView item, string targetGroupName, int index = -1) {
            if (item == null) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>ChangeItemGroup", 
                    "Item cannot be null!", "OK");
                return;
            }
            var targetGroupView = FindGroupView(targetGroupName) ?? AddGroup(targetGroupName);
            if (targetGroupView.Items.Any(i => i.Guid == item.Guid)) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>ChangeItemGroup", 
                    $"Item [{item.Name}] with GUID {item.Guid} already exists in group {targetGroupName}!", "OK");
                return;
            }
            var oldGroupView = FindGroupView(item.GroupName);

            FavoritesItemData targetItemData = Model.FindItem(item.GroupName, item.Guid);
            FavoritesGroupData targetGroupData = Model.FindGroup(targetGroupName);
            FavoritesGroupData oldGroupData = Model.FindGroup(item.GroupName);
            bool isSuccesses = Model.AddItem(targetItemData, targetGroupData, index);
            if (!isSuccesses) {
                return;
            }
            isSuccesses = Model.RemoveItem(targetItemData, oldGroupData);
            if (!isSuccesses) {
                return;
            }
            
            index = index == -1 ? targetGroupView.Items.Count : index;
            targetGroupView.Items.Insert(index, item);
            oldGroupView.Items.Remove(item);
            item.GroupName = targetGroupName;
            SortGroup(targetGroupView, QuickAssetsFavoritesView.SelectedSortOption);
        }

        public void ChangeItemNote(FavoritesItemView item, string newNoteStr) {
            if (item == null) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>ChangeItemNote", 
                    "Item cannot be null!", "OK");
                return;
            }
            if (item.Note == newNoteStr) {
                return;
            }
            newNoteStr ??= "";
            FavoritesItemData itemData = Model.FindItem(item.GroupName, item.Guid);
            bool isSuccesses = Model.ChangeItemNote(itemData, newNoteStr);
            if (!isSuccesses) {
                return;
            }

            item.Note = newNoteStr;
        }
        
        public void RemoveItem(FavoritesItemView item) {
            if (item == null) {
                EditorUtility.DisplayDialog("QuickFavoritesFolder>RemoveItem", 
                    "Item cannot be null!", "OK");
                return;
            }
            bool isSuccesses = Model.RemoveItem(item.Guid, item.GroupName);
            if (!isSuccesses) {
                return;
            }
            var groupView = FindGroupView(item.GroupName);
            groupView.Items.Remove(item);
        }
        #endregion
    }
}