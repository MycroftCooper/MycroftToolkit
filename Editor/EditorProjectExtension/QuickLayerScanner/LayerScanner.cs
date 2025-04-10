using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace EditorProjectExtension.QuickLayerScanner {
    public class LayerScanResult {
        public string PrefabPath;
        public string PrefabName;
        public Dictionary<string, string> PrefabNodeLayerDict;
    }
    
    public static class LayerScanner {
        private static System.Diagnostics.Stopwatch stopwatch;
        
        public static void ScanAndExport(int mask) {
            targetLayerMask = mask;
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            GetAllPrefabPath();
            GetAllScenePath();
            totalResCount = prefabPaths.Count + scenePaths.Count;
            InitTargetLayerMask();
            
            completedCount = 0;
            resultQueue = new ConcurrentQueue<LayerScanResult>();
            scanResults = new List<LayerScanResult>();
            isScanning = true;
            
            EditorApplication.update += MonitorProgress;
            
            Task.Run(() => {
                Parallel.ForEach(prefabPaths, path => {
                    var result = ScanPrefab(path);
                    if (result != null)
                        resultQueue.Enqueue(result);
                    Interlocked.Increment(ref completedCount);
                });
                
                Parallel.ForEach(scenePaths, path => {
                    var result = ScanSceneFile(path);
                    if (result != null)
                        resultQueue.Enqueue(result);
                    Interlocked.Increment(ref completedCount);
                });

                isScanning = false;
            });
        }
        
        private static ConcurrentQueue<LayerScanResult> resultQueue = new();
        private static int completedCount;
        private static bool isScanning;
        private static int totalResCount;
        private static void MonitorProgress() {
            while (resultQueue.TryDequeue(out var result)) {
                scanResults.Add(result);
            }

            float progress = totalResCount > 0 ? (float)completedCount / totalResCount : 0f;
            bool cancel = EditorUtility.DisplayCancelableProgressBar("Layer Scanner", $"Scanning... {completedCount}/{totalResCount}", progress);
            if (cancel) {
                EditorApplication.update -= MonitorProgress;
                EditorUtility.ClearProgressBar();
                Debug.LogWarning("扫描已被用户取消");
            }
            
            if (!isScanning && resultQueue.IsEmpty) {
                EditorApplication.update -= MonitorProgress;
                EditorUtility.ClearProgressBar();
                ExportToCsv();
                stopwatch.Stop();
                Debug.Log($"扫描完成，耗时 {stopwatch.Elapsed.TotalSeconds:F2} 秒");
            }
        }


        private static List<string> prefabPaths = new List<string>();
        private static void GetAllPrefabPath() {
            prefabPaths.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            if (guids == null || guids.Length == 0) return;
            prefabPaths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".prefab")) // 保险起见
                .ToList();
        }
        
        private static List<string> scenePaths = new List<string>();
        private static void GetAllScenePath() {
            scenePaths.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            if (guids == null || guids.Length == 0) return;
            scenePaths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".unity")) // 保险起见
                .ToList();
        }

        private static List<LayerScanResult> scanResults = new List<LayerScanResult>();
        private static LayerScanResult ScanPrefab(string prefabPath) {
            try {
                string fullPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), prefabPath);
                if (!File.Exists(fullPath)) return null;

                string[] lines = File.ReadAllLines(fullPath);
                Dictionary<string, string> nodeLayers = new();

                string currentName = null;
                string currentLayer = null;
                bool inGameObjectBlock = false;

                foreach (string rawLine in lines) {
                    string line = rawLine.Trim();

                    if (line.StartsWith("--- !u!1 &")) {
                        // 新的 GameObject 块开始
                        currentName = null;
                        currentLayer = null;
                        inGameObjectBlock = true;
                        continue;
                    }

                    if (!inGameObjectBlock) continue;
                    if (line.StartsWith("m_Name:")) {
                        currentName = line.Substring("m_Name:".Length).Trim();
                    }
                    else if (line.StartsWith("m_Layer:")) {
                        currentLayer = line.Substring("m_Layer:".Length).Trim();
                    }

                    // 名称和 Layer 都拿到了，就处理
                    if (string.IsNullOrEmpty(currentName) || string.IsNullOrEmpty(currentLayer)) continue;
                    if (int.TryParse(currentLayer, out int layerIndex)) {
                        if (CompareLayer(layerIndex)) {
                            string layerName = layerIndexToName.TryGetValue(layerIndex, out var name) ? name : $"Layer{layerIndex}";
                            nodeLayers.TryAdd(currentName, layerName);
                        }
                    }

                    // 当前 GameObject 处理完毕，退出该块
                    inGameObjectBlock = false;
                }
                
                if (nodeLayers.Count == 0) return null;


                return new LayerScanResult {
                    PrefabPath = prefabPath,
                    PrefabName = Path.GetFileNameWithoutExtension(prefabPath),
                    PrefabNodeLayerDict = nodeLayers
                };
            }catch (Exception ex) {
                Debug.LogError($"[SCAN ERROR] {prefabPath} 失败: {ex.Message}");
                return null;
            }
        }
        
        private static LayerScanResult ScanSceneFile(string scenePath) {
            string fullPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), scenePath);
            if (!File.Exists(fullPath)) return null;

            string[] lines = File.ReadAllLines(fullPath);
            Dictionary<string, string> nodeLayers = new();

            string currentName = null;
            string currentLayer = null;
            bool inGameObject = false;

            foreach (var rawLine in lines) {
                string line = rawLine.Trim();

                if (line.StartsWith("--- !u!1 &")) {
                    inGameObject = true;
                    currentName = null;
                    currentLayer = null;
                    continue;
                }

                if (!inGameObject) continue;
                if (line.StartsWith("m_Name:")) {
                    currentName = line.Substring("m_Name:".Length).Trim();
                }else if (line.StartsWith("m_Layer:")) {
                    currentLayer = line.Substring("m_Layer:".Length).Trim();
                }

                if (string.IsNullOrEmpty(currentName) || string.IsNullOrEmpty(currentLayer)) continue;
                if (int.TryParse(currentLayer, out int layerIndex) && CompareLayer(layerIndex)) {
                    string layerName = layerIndexToName.TryGetValue(layerIndex, out var name) ? name : $"Layer{layerIndex}";
                    nodeLayers.TryAdd(currentName, layerName);
                }
                inGameObject = false;
            }

            if (nodeLayers.Count == 0) return null;

            return new LayerScanResult {
                PrefabPath = scenePath,
                PrefabName = Path.GetFileNameWithoutExtension(scenePath),
                PrefabNodeLayerDict = nodeLayers
            };
        }
        
        private static int targetLayerMask;
        private static readonly Dictionary<int, string> layerIndexToName = new();

        private static void InitTargetLayerMask() {
            layerIndexToName.Clear();
            for (int i = 0; i < 32; i++) {
                string name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name)) {
                    layerIndexToName[i] = name;
                }
            }
        }
        
        private static bool CompareLayer(int targetLayer) => (targetLayerMask & (1 << targetLayer)) != 0;

        private static void ExportToCsv() {
            string outputPath = Path.Combine(Application.dataPath, "../LayerScanResult.csv");
            using var writer = new StreamWriter(outputPath);
            writer.WriteLine("PrefabPath,PrefabName,GameObjectName,Layer");

            lock (scanResults) {
                foreach (var result in scanResults) {
                    foreach (var kv in result.PrefabNodeLayerDict) {
                        writer.WriteLine($"\"{result.PrefabPath}\",\"{result.PrefabName}\",\"{kv.Key}\",\"{kv.Value}\"");
                    }
                }
            }

            Debug.Log($"CSV 导出完成，共记录 {scanResults.Sum(r => r.PrefabNodeLayerDict.Count)} 条");
            EditorUtility.RevealInFinder(outputPath);
        }
    }
    
    public class LayerScannerWindow : EditorWindow {
        private static readonly int MaxLayers = 32;
        private static readonly bool[] layerSelections = new bool[MaxLayers];
        private static readonly string[] layerNames = new string[MaxLayers];

        [MenuItem("Tools/Layer Scanner")]
        public static void ShowWindow() {
            InitLayerNames();
            GetWindow<LayerScannerWindow>("Layer Scanner Tool");
        }

        private void OnGUI() {
            GUILayout.Label("选择你要扫描的 Layer：", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            for (int i = 0; i < MaxLayers; i++) {
                if (!string.IsNullOrEmpty(layerNames[i])) {
                    layerSelections[i] = EditorGUILayout.ToggleLeft($"{layerNames[i]} ({i})", layerSelections[i]);
                }
            }

            EditorGUILayout.Space();
            if (!GUILayout.Button("开始扫描并导出 CSV")) return;

            int layerMask = 0;
            List<string> selectedNames = new();
            for (int i = 0; i < MaxLayers; i++) {
                if (layerSelections[i]) {
                    layerMask |= (1 << i);
                    selectedNames.Add(layerNames[i]);
                }
            }

            if (layerMask == 0) {
                EditorUtility.DisplayDialog("提示", "你需要至少选择一个 Layer 才能开始扫描！", "好");
                return;
            }

            Debug.Log($"▶ 开始扫描目标 Layer: {string.Join(", ", selectedNames)}");
            LayerScanner.ScanAndExport(layerMask); // 你已有逻辑的入口函数
        }

        private static void InitLayerNames() {
            for (int i = 0; i < MaxLayers; i++) {
                layerNames[i] = LayerMask.LayerToName(i);
                layerSelections[i] = false;
            }
        }
    }
}