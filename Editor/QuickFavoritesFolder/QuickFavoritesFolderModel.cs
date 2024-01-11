using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuickFavoritesFolder {
    [Serializable]
    public class FavoritesItemData {
        public string guid;
        public string name;
    }

    [Serializable]
    public class FavoritesGroupData {
        public string name;
        public List<FavoritesItemData> items;
    }
    
    public class QuickFavoritesFolderModel {
        public List<FavoritesGroupData> Groups;

        public void Load() {
            Groups = new List<FavoritesGroupData>();
        }

        public void Save() {
            
        }

        public bool AddItem(FavoritesItemData item, FavoritesGroupData group, int index = -1) {
            if (item == null) {
                Debug.LogError("QuickFavoritesFolder>AddItem>Item shouldn't be null!");
                return false;
            }
            if (group == null) {
                Debug.LogError("QuickFavoritesFolder>AddItem>Group shouldn't be null!");
                return false;
            }
            if (FindItem(group, item.guid) != null) {
                Debug.LogError($"QuickFavoritesFolder>AddItem>Item [{item.name}] with GUID {item.guid} already exists in group {group.name}!");
                return false;
            }

            if (index < -1 || index > group.items.Count) {
                Debug.LogError($"QuickFavoritesFolder>AddItem>Item Index[{index}] is not right!");
                return false;
            }

            index = index == -1 ? group.items.Count : index;
            group.items.Insert(index, item);

            Save();
            return true;
        }
        
        public bool AddItem(FavoritesItemData item, string targetGroup = "Default", int index = -1) {
            if (item == null) {
                Debug.LogError("QuickFavoritesFolder>AddItem>Item shouldn't be null!");
                return false;
            }
            var group = FindGroup(targetGroup) ?? AddNewGroup(targetGroup);
            return AddItem(item, group, index);
        }

        public bool RemoveItem(FavoritesItemData item, FavoritesGroupData group) {
            if (item == null) {
                Debug.LogError("QuickFavoritesFolder>RemoveItem>Item shouldn't be null!");
                return false;
            }
            if (group == null) {
                Debug.LogError("QuickFavoritesFolder>RemoveItem>Group shouldn't be null!");
                return false;
            }
            
            if (!group.items.Remove(item)) {
                Debug.LogError($"QuickFavoritesFolder>RemoveItem>Item[{item.name}] is not in group[{group.name}]");
                return false;
            }
            Save();
            return true;
        }

        public bool RemoveItem(string targetItemGuid, string targetGroupName) {
            if (string.IsNullOrEmpty(targetItemGuid)) {
                Debug.LogError("QuickFavoritesFolder>RemoveItem>Item guid shouldn't be null!");
                return false;
            }
            if (string.IsNullOrEmpty(targetGroupName)) {
                Debug.LogError("QuickFavoritesFolder>RemoveItem>Target group name shouldn't be null!");
                return false;
            }
            FavoritesGroupData group = Groups.Find(g => g.name == targetGroupName);
            if (group == null) {
                Debug.LogError($"QuickFavoritesFolder>RemoveItem>Group[{targetGroupName}] does not exist.");
                return false;
            }
            var targetItem = FindItem(group, targetItemGuid);
            if (targetItem == null) {
                Debug.LogError($"QuickFavoritesFolder>RemoveItem>Item[{targetItemGuid}] does not exist in group[{targetGroupName}].");
                return false;
            } 
            return RemoveItem(targetItem, group);
        }

        public FavoritesGroupData AddNewGroup(string newGroupName) {
            if (Groups.Any(g => g.name == newGroupName)) {
                Debug.LogError($"QuickFavoritesFolder>AddGroup>Group of the same name[{newGroupName}] already exists!");
                return null;
            }

            FavoritesGroupData group = new FavoritesGroupData {
                name = newGroupName,
                items = new List<FavoritesItemData>()
            };
            Groups.Add(group);
            Save();
            return group;
        }

        public bool DeleteGroup(string groupName) {
            FavoritesGroupData group = FindGroup(groupName);
            if (group == null) {
                Debug.LogError($"QuickFavoritesFolder>DeleteItem>The group[{groupName}] does not exist.");
                return false;
            }
            Groups.Remove(group);
            Save();
            return true;
        }

        public bool RenameGroup(string oldName, string newName) {
            FavoritesGroupData group = FindGroup(oldName);
            if (group == null) {
                Debug.LogError($"QuickFavoritesFolder>RenameGroup>The group[{oldName}] does not exist.");
                return false;
            }

            if (Groups.Any(g => g.name == newName)) {
                Debug.LogError($"QuickFavoritesFolder>RenameGroup>Group of the same name[{newName}] already exists!");
                return false;
            }

            group.name = newName;
            Save();
            return true;
        }
        
        public FavoritesGroupData FindGroup(string groupName) {
            FavoritesGroupData group = Groups.Find(g => g.name == groupName);
            return group;
        }

        public FavoritesItemData FindItem(string groupName, string itemGuid) {
            var group = FindGroup(groupName);
            if (group == null) {
                return null;
            }
            var item = FindItem(group, itemGuid);
            return item;
        }

        public FavoritesItemData FindItem(FavoritesGroupData group, string itemGuid) {
            var item = group.items.Find(i => i.guid == itemGuid);
            return item;
        }
    }
}