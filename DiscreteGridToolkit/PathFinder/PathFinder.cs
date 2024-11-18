using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MycroftToolkit.DiscreteGridToolkit;
using MycroftToolkit.QuickCode.FrameTask;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PathFinding {
    public class PathFinder : FrameTaskScheduler<PathFindingFrameTask> {
        #region 地图处理相关
        public bool canDiagonallyPassByObstacle;
        private SourceMap _map;
        
        public void SetPassableMap(bool[,] map) {
            _map = new SourceMap(map, canDiagonallyPassByObstacle);
            if(_algorithms.Count != 0) {
                foreach (var a in _algorithms.Values) {
                    a.InitMap(_map);
                }
            }
        }
        
        public void UpdatePassableMap(RectInt bounds, bool passable) {
            if (!_map.IsInBounds(bounds.min.x, bounds.min.y) || !_map.IsInBounds(bounds.max.x, bounds.max.y)) {
                Debug.LogError($"Bounds{bounds} is not in Map bounds!");
                return;
            }
            _map.UpdateMap(bounds, passable);
            if(_algorithms.Count != 0) {
                foreach (var a in _algorithms.Values) {
                    a.UpdateMap(bounds, passable);
                }
            }
        }
        #endregion

        #region 算法相关
        public bool useLineOfSightFirstCheck;
        private readonly Dictionary<PathFinderAlgorithms, IPathFinderAlgorithm> _algorithms = new ();
        private readonly Dictionary<PathReprocesses, IPathReprocess> _reprocesses = new ();
        private readonly HeuristicFunctions.CommonHeuristicFunction _commonHeuristicFunction = new(HeuristicTypes.Manhattan);
        
        private bool FirstCheck(PathFindingRequest request) {
            if (!useLineOfSightFirstCheck || !_map.IsLineOfSight(request.StartPos, request.EndPos)) return false;
            request.ResultPath = new List<Vector2Int>{request.EndPos};
            request.ReprocessedPath = new List<Vector2Int>(request.ResultPath);
            return true;
        }

        private IPathFinderAlgorithm GetAlgorithm(PathFinderAlgorithms algorithmType, HeuristicTypes heuristicFunction) {
            if (_algorithms.TryGetValue(algorithmType, out var a)) {
                return a;
            }
            
            a = algorithmType switch {
                PathFinderAlgorithms.JPS => new JPS(),
                PathFinderAlgorithms.JPSPlus => new JPSPlus(),
                PathFinderAlgorithms.AStar => new AStart(),
                PathFinderAlgorithms.Dijkstra => new Dijkstra(),
                PathFinderAlgorithms.BFS => new BFS(),
                PathFinderAlgorithms.DFS => new DFS(),
                _ => throw new ArgumentOutOfRangeException(nameof(algorithmType), algorithmType, null)
            };
            
            if (heuristicFunction == HeuristicTypes.None) {
                a.HeuristicFunction = null;
            } else {
                _commonHeuristicFunction.HeuristicType = heuristicFunction;
                a.HeuristicFunction = _commonHeuristicFunction;
            }
            
            a.InitMap(_map);
            _algorithms.Add(algorithmType, a);
            return a;
        }

        private IPathReprocess GetReprocess(PathReprocesses reprocessType) {
            if (reprocessType == PathReprocesses.None) {
                return null;
            }
            if (_reprocesses.TryGetValue(reprocessType, out var p)) {
                return p;
            }

            p = reprocessType switch {
                PathReprocesses.Default => new DefaultPathSmooth(),
                PathReprocesses.Theta => new Theta(),
                _ => throw new ArgumentOutOfRangeException(nameof(reprocessType), reprocessType, null)
            };
            _reprocesses.Add(reprocessType ,p);
            return p;
        }
        #endregion
        
        private Queue<PathFindingRequest> _pathCache;
        public void AddFindPathRequest(PathFindingRequest request, int priority = 1) {
            if (!IsRequestValid(request)) return;
            if (useLineOfSightFirstCheck && FirstCheck(request)) return;
            
            float h = HeuristicFunctions.CalculateHeuristic(request.HeuristicType, request.StartPos, request.EndPos);
            float mapMaxH = HeuristicFunctions.CalculateHeuristic(request.HeuristicType, Vector2Int.zero, _map.Size - Vector2Int.one);
            float p = priority + h / mapMaxH;
            
            var newTask = new PathFindingFrameTask(this, request, p);
            AddTask(newTask);
        }

        public void ExecuteRequest(PathFindingRequest request) {
            if (request.CanUseCache) {
                // todo:使用缓存加速寻路
            }
            else {
                var a = GetAlgorithm(request.Algorithm, request.HeuristicType);
                a.NeedBestSolution = request.NeedBestSolution;
                var resultPath = a.FindPath(request.StartPos, request.EndPos);
                request.ResultPath = resultPath;
                var p = GetReprocess(request.Reprocess);
                request.ReprocessedPath = p != null ? p.ReprocessPath(request.ResultPath, _map) : new List<Vector2Int>(request.ResultPath);
            }
        }

        private bool IsRequestValid(PathFindingRequest request) {
            if (request == null) {
                Debug.LogError("PathFinder: request is null");
                return false;
            }
            if (request.StartPos == request.EndPos) {
                Debug.LogError($"PathFinder: StartPos cant equal EndPos{request.EndPos.x}");
                return false;
            }
            if (!_map.IsInBounds(request.StartPos.x, request.StartPos.y) ||
                !_map.IsInBounds(request.EndPos.x, request.EndPos.y)) {
                Debug.LogError($"PathFinder: StartPos{request.StartPos} or EndPos{request.EndPos.x} is out of range");
                return false;
            }
            if (!_map.IsPassable(request.StartPos.x, request.StartPos.y, false) || 
                !_map.IsPassable(request.EndPos.x, request.EndPos.y, false)) {
                Debug.LogError($"PathFinder: StartPos{request.StartPos} or EndPos{request.EndPos.x} is not passable");
                return false;
            }
            return true;
        }

        #region Debug相关
        public bool isDebug;
        [ShowInInspector] public bool[,] TestMap;
        public bool debugNeedBestSolution = true;
        public PathFinderAlgorithms debugAlgorithm = PathFinderAlgorithms.JPS;
        public PathReprocesses debugPathReprocesses = PathReprocesses.None;
        public HeuristicTypes debugHeuristic = HeuristicTypes.Manhattan;
        private PathFindingRequest _debugRequest;
        private Stopwatch _stopwatch;
        private MazeGenerator _debugMazeGenerator;

        [Button]
        private void DebugInitTestMap(Vector2Int size) {
            TestMap = new bool[size.x, size.y];
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    TestMap[x, y] = true;
                }
            }
            SetPassableMap(TestMap);
        }

        [Button]
        private void DebugUpdateTestMap() {
            SetPassableMap(TestMap);
        }

        [Button]
        private void DebugGeneratorMaze(Vector2Int size, int seed) {
            size = new Vector2Int(101, 100);
            TestMap = new bool[size.x, size.y];
            _debugMazeGenerator = new MazeGenerator();
            TestMap = _debugMazeGenerator.GenerateMaze(size,seed, new Vector2Int(1,0), new Vector2Int(99,99));
            SetPassableMap(TestMap);
        }

        [Button]
        private void DebugFindPath(Vector2Int start, Vector2Int end) {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            PathFindingRequest request = new PathFindingRequest(start, end, 
                debugAlgorithm, debugNeedBestSolution, debugHeuristic, debugPathReprocesses);
            ExecuteRequest(request);
            _debugRequest = request;
            _stopwatch.Stop();
            Debug.Log($"Pathfinder> DebugRequest completed in {_stopwatch.Elapsed.TotalMilliseconds} ms.\n" +
                      $"{_debugRequest}\n{_map}");
        }

        [Button]
        private void DebugSplitFrameTest(Vector2Int start, Vector2Int end, int times) {
            for (int i = 0; i < times; i++) {
                PathFindingRequest request = new PathFindingRequest(start, end, 
                    debugAlgorithm, debugNeedBestSolution, debugHeuristic, debugPathReprocesses, false, 
                    r=> Debug.Log($"Pathfinder> DebugRequest completed!\n{r}"));
                AddFindPathRequest(request);
            }
            Debug.Log($"Pathfinder> All {times} DebugRequest completed!");
        }

        void OnDrawGizmos() {
            if (!isDebug) return;
            DrawMap();
            if (_debugRequest == null) return;
            DrawPath(_debugRequest.ResultPath, Color.blue, Vector3.zero);
            DrawPath(_debugRequest.ReprocessedPath, Color.green, new Vector3(0.1f, 0f, 0.1f));
        }

        private void DrawMap() {
            if(_map == null) return;
            IPathFinderAlgorithm a = null;
            if (_debugRequest != null) {
                a = GetAlgorithm(_debugRequest.Algorithm, _debugRequest.HeuristicType);
            }
            Gizmos.color = Color.gray;
            Vector3 oPos = transform.position + new Vector3(0.5f, 0.5f);
            // 遍历 passableMap 并绘制格子
            for (int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    Vector3 position = oPos + new Vector3(x, y, 0); // 将网格位置映射到世界空间
                    Gizmos.color = _map.PassableMap[x, y] ? Color.gray : Color.red;
                    Gizmos.DrawCube(position, new Vector3(1, 1, 0.1f));
                    Gizmos.color = Color.black; // 边框颜色设置为黑色
                    Gizmos.DrawWireCube(position, Vector3.one);
                    a?.OnDebugDrawGizmos(oPos, new Vector2Int(x, y));
                }
            }
        }
        
        private void DrawPath(List<Vector2Int> path, Color color, Vector3 offset) {
            if (path == null || path.Count == 0) {
                return;
            }
            Vector3 oPos = transform.position + new Vector3(0.5f, 0.5f);
            
            Gizmos.color = color;
            var p = path[0].ToVec3() + oPos + new Vector3(0, 0, 0.2f);
            Gizmos.DrawCube(p, new Vector3(0.9f, 0.9f, 0.1f));
            p = path[^1].ToVec3() + oPos + new Vector3(0, 0, 0.2f);
            Gizmos.DrawCube(p, new Vector3(0.9f, 0.9f, 0.1f));
            
            for (int i = 0; i < path.Count - 1; i++) {
                Vector3 startPos = oPos + new Vector3(path[i].x, path[i].y, 0.2f);
                Vector3 endPos = oPos + new Vector3(path[i + 1].x, path[i + 1].y, 0.2f);
                Debug.DrawLine(startPos + offset, endPos + offset, color);
            }
        }
        #endregion
    }
}