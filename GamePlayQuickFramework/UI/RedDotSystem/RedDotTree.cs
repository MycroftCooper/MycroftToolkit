using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamePlayQuickFramework.UI.RedDotSystem {
    public class RedDotNode {
        public readonly string Key;
        public readonly string SubKey;
            
        public int SelfDotCount { get; private set; }
        public int SubDotCount { get; private set; }
        public int TotalDotCount => SelfDotCount + SubDotCount;
        public bool IsEnable => TotalDotCount > 0;
        
        public RedDotNode Parent;
        public bool IsRoot => Parent == null;
        public readonly List<RedDotNode> Children;

        public RedDotNode(string key, int selfDotCount = 0) {
            Key = key;
            var index = key.LastIndexOf('/');
            SubKey = index >= 0 ? key[(index + 1)..] : key;
            SelfDotCount = selfDotCount;
            SubDotCount = 0;

            Parent = null;
            Children = new List<RedDotNode>();
        }

        public void AddChild(RedDotNode child) {
            if (child == null) {
                Debug.LogError($"RedDotNode>AddChild>{Key}> child cant be null!");
                return;
            }
            if (Children.Contains(child)) {
                Debug.LogError($"RedDotNode>AddChild>{Key}> child{child.Key} already exists!");
                return;
            }
            child.Parent = this;
            Children.Add(child);
        }

        public bool SetSelfDotCount(int selfDotCount) {
            if(SelfDotCount == selfDotCount) return false;
            SelfDotCount = selfDotCount;
            return true;
        }

        public bool Refresh() {
            int newSubCount = 0;
            foreach (var child in Children) {
                newSubCount += child.TotalDotCount;
            }
            if(newSubCount == SubDotCount) return false;
            SubDotCount = newSubCount;
            return true;
        }
        
        public override string ToString() {
            return $"[RedDotNode: Key={Key}, Self={SelfDotCount}, Sub={SubDotCount}, Total={TotalDotCount}, IsEnable={IsEnable}, ChildrenCount={Children.Count}]";
        }
    }
    
    public class RedDotTree {
        private RedDotNode root;
        private readonly Dictionary<string, RedDotNode> _nodes = new Dictionary<string, RedDotNode>();

        public void Init() {
            root = new RedDotNode("Root");
            _nodes.Add("Root", root);
        }
        
        public List<RedDotNode> Refresh() {
            // 按照路径中'/'的数量排序，层级越深的节点排在前面
            var nodesSorted = _nodes.Values.OrderByDescending(n => n.Key.Count(c => c == '/')).ToList();
            return nodesSorted.Where(node => node.Refresh()).ToList();
        }
        
        public bool Contains(string key) => _nodes.ContainsKey(key);
        public bool TryGetNode(string key, out RedDotNode node) => _nodes.TryGetValue(key, out node);
        public RedDotNode GetNode(string key) => _nodes.GetValueOrDefault(key);
        
        public RedDotNode AddNode(string key, int selfDotCount = 0) {
            if (_nodes.TryGetValue(key, out var node)) {
                Debug.LogError($"RedDotNode>AddNode>{key}> key already exists!");
                return node;
            }
            
            string[] parts = key.Split('/');
            string currentPath = "";
            RedDotNode parent = null;
            for (int i = 0; i < parts.Length; i++) {
                currentPath = i == 0 ? parts[i] : currentPath + "/" + parts[i];
                if (!TryGetNode(currentPath, out var node1)) {
                    var newNode = new RedDotNode(currentPath);
                    parent?.AddChild(newNode);
                    _nodes[currentPath] = newNode;
                    parent = newNode;
                } else {
                    parent = node1;
                }
            }
            _nodes[key].SetSelfDotCount(selfDotCount);
            return _nodes[key];
        }
        
        public List<RedDotNode> SetDotNodeSelfDotCount(string key, int selfDotCount) {
            if (!TryGetNode(key, out var node)) {
                Debug.LogError($"RedDotTree>UpdateNodeState> 未找到节点：{key}");
                return null;
            }
            if (!node.SetSelfDotCount(selfDotCount)) {
                return null;
            }

            var changedList = new List<RedDotNode> { node };
            
            RedDotNode parent = node.Parent;
            while (parent != null) {
                parent.Refresh();
                changedList.Add(parent);
                parent = parent.Parent;
            }
            return changedList;
        }
        
        public override string ToString() {
            var sb = new System.Text.StringBuilder();
    
            // 第一部分：按层级输出树形结构（仅显示 SubKey）
            sb.AppendLine("Tree Structure (SubKey):");
            // 使用字典中所有节点，按层级深度排序，层级越深缩进越多
            var nodesSorted = _nodes.Values
                .OrderBy(n => n.Key.Count(c => c == '/'))
                .ThenBy(n => n.Key)
                .ToList();
            foreach (var node in nodesSorted) {
                int indentLevel = node.Key.Count(c => c == '/');
                string indent = new string(' ', indentLevel * 2);
                sb.AppendLine($"{indent}{node.SubKey}");
            }
    
            // 第二部分：输出所有节点详细信息
            sb.AppendLine("\nNode Details:");
            foreach (var node in nodesSorted) {
                sb.AppendLine(node.ToString());
            }
    
            return sb.ToString();
        }
    }
}