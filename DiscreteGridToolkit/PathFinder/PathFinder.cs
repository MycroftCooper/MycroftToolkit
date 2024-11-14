using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MycroftToolkit.DiscreteGridToolkit;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PathFinding {
    public class PathFinder : MonoBehaviour {
        public bool canDiagonallyPassByObstacle;
        private SourceMap _map;
        private readonly Dictionary<PathFinderAlgorithms, IPathFinderAlgorithm> _algorithms = new ();
        private readonly Dictionary<PathReprocesses, IPathReprocess> _reprocesses = new ();
        private readonly HeuristicFunctions.CommonHeuristicFunction _commonCommonHeuristicFunction = new(HeuristicTypes.Manhattan);
        
        [Button]
        public void SetPassableMap(bool[,] map) {
            if (isDebug) {
                map = TestMap;
            }
            _map = new SourceMap(map, canDiagonallyPassByObstacle);
            if(_algorithms.Count != 0) {
                foreach (var a in _algorithms.Values) {
                    a.InitMap(_map);
                }
            }
        }

        [Button]
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
        
        private Queue<PathFindingRequest> _requestQueue = new();
        private Queue<PathFindingRequest> _pathCache;
        public void FindPath(PathFindingRequest request) {
            if (!IsRequestValid(request)) {
                return;
            }

            if (!request.NeedHandleImmediately) {
                _requestQueue.Enqueue(request);
                return;
            }

            if (!request.CanUseCache) {
                var a = GetAlgorithm(request.Algorithm, request.HeuristicType);
                a.NeedBestSolution = request.NeedBestSolution;
                var resultPath = a.FindPath(request.StartPos, request.EndPos);
                request.ResultPath = resultPath;
                var p = GetReprocess(request.Reprocess);
                request.ReprocessedPath = p != null ? p.ReprocessPath(request.ResultPath, _map) : request.ResultPath;
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

        private IPathFinderAlgorithm GetAlgorithm(PathFinderAlgorithms algorithmType, string heuristicFunction) {
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
            
            if (string.IsNullOrEmpty(heuristicFunction)) {
                a.HeuristicFunction = null;
            } else {
                if (Enum.TryParse<HeuristicTypes>(heuristicFunction, out var heuristic)) {
                    _commonCommonHeuristicFunction.HeuristicType = heuristic;
                    a.HeuristicFunction = _commonCommonHeuristicFunction;
                } else {
                    // 反射获取启发式函数
                }
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
                _ => throw new ArgumentOutOfRangeException(nameof(reprocessType), reprocessType, null)
            };
            _reprocesses.Add(reprocessType ,p);
            return p;
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
        private void DebugGeneratorMaze(Vector2Int size, int seed) {
            size = new Vector2Int(101, 100);
            TestMap = new bool[size.x, size.y];
            _debugMazeGenerator = new MazeGenerator();
            TestMap = _debugMazeGenerator.GenerateMaze(size,seed, new Vector2Int(1,0), new Vector2Int(99,99));
            SetPassableMap(null);
        }

        [Button]
        private void DebugFindPath(Vector2Int start, Vector2Int end) {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            debugAlgorithm = PathFinderAlgorithms.JPS;
            PathFindingRequest request = new PathFindingRequest(start, end, debugAlgorithm, debugNeedBestSolution, 
                debugHeuristic.ToString(), debugPathReprocesses, false, true);
            FindPath(request);
            _debugRequest = request;
            _stopwatch.Stop();
            Debug.Log($"Pathfinder> DebugRequest completed in {_stopwatch.Elapsed.TotalMilliseconds} ms.\n\n" +
                      $"{_debugRequest}\n\n{_map}");
        }
        
        void OnDrawGizmos() {
            if (!isDebug || _map == null) {
                return;
            }

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

            DrawPath(_debugRequest);
        }
        
        private void DrawPath(PathFindingRequest path) {
            if (path?.ReprocessedPath == null || path.ReprocessedPath.Count == 0) {
                return;
            }
            Vector3 oPos = transform.position + new Vector3(0.5f, 0.5f);
            
            Gizmos.color = Color.blue;
            var p = path.StartPos.ToVec3() + oPos + new Vector3(0, 0, 0.2f);
            Gizmos.DrawCube(p, new Vector3(1, 1, 0.1f));
            Gizmos.color = new Color(0f, 0f, 0.5f);
            p = path.EndPos.ToVec3() + oPos + new Vector3(0, 0, 0.2f);
            Gizmos.DrawCube(p, new Vector3(1, 1, 0.1f));
            
            Debug.DrawLine(path.StartPos.ToVec3() + oPos, path.ResultPath[0].ToVec3() + oPos, Color.blue);
            for (int i = 0; i < path.ReprocessedPath.Count - 1; i++) {
                Vector3 startPos = oPos + new Vector3(path.ReprocessedPath[i].x, path.ReprocessedPath[i].y, 0.2f);
                Vector3 endPos = oPos + new Vector3(path.ReprocessedPath[i + 1].x, path.ReprocessedPath[i + 1].y, 0.2f);
                Debug.DrawLine(startPos, endPos, Color.blue);
            }
        }
        #endregion
    }
}