using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlayQuickFramework.UI.RedDotSystem {
    public interface IRedDotUI {
        public string RedDotKey { get; }
        public void SetRedDotState(bool active, int dotCount);
    }

    public interface IRedDotDataSource {
        public string RedDotKey { get; }
        public Action<string, int> OnRedDotCountChanged { get; }
    }
    
    public class RedDotManager: MonoBehaviour {
        public static RedDotManager Instance { get; private set; }
        
        private void Awake() {
            if (Instance == null) {

                DontDestroyOnLoad(gameObject);
                _redDotTree = new RedDotTree();
                _redDotTree.Init();
            }else {
                Destroy(gameObject);
            }
        }

        #region UI相关
        private Dictionary<string, IRedDotUI> redDotUIDict = new Dictionary<string, IRedDotUI>();
        
        public void BindRedDotUI(IRedDotUI redDotUI) {
            
        }

        public void UnBindRedDotUI(string key) {
            
        }
        #endregion

        #region 数据源相关
        private Dictionary<string, IRedDotDataSource> redDotDataSourceDict = new Dictionary<string, IRedDotDataSource>();
        
        public void BindRedDotDataSource(IRedDotDataSource redDotDataSource) {
            
        }

        public void UnBindRedDotDataSource(string key) {
            
        }

        private void OnRedDotCountChangedHandler(string redDotKey, int redDotCount) {
            if (!_redDotTree.TryGetNode(redDotKey, out var node)) {
                Debug.LogError($"RedDotManager>OnRedDotCountChangedHandler> 未找到节点：{redDotKey}!");
                return;
            }
            var changedNodes = _redDotTree.SetDotNodeSelfDotCount(redDotKey, selfDotCount);
        }
        #endregion
       
        
        private RedDotTree _redDotTree;
        
        public void RefreshAll() {
            var needUpdate = _redDotTree.Refresh();
            foreach (var node in needUpdate) {
                redDotUIDict.TryGetValue(node.Key, out var redDotUI);
                redDotUI?.SetRedDotState(node.IsEnable, node.TotalDotCount);
            }
        }

        #region Debug相关
        public void LogTree() {
            Debug.Log(_redDotTree.ToString());
        }
        #endregion

    }
}