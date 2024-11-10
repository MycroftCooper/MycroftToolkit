using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MycroftToolkit.DiscreteGridToolkit;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PathFinding {
    public class PathFinder : MonoBehaviour {
        private SourceMap _map;
        public Dictionary<PathFinderAlgorithms, IPathFinderAlgorithm> Algorithms = new ();
        public Dictionary<PathReprocesses, IPathReprocess> Reprocesses = new ();
        
        [Button]
        public void SetPassableMap(bool[,] map) {
            if (isDebug) {
                map = TestMap;
            }
            _map = new SourceMap(map);
            if(Algorithms.Count != 0) {
                foreach (var a in Algorithms.Values) {
                    a.InitMap(_map);
                }
            }
        }

        [Button]
        public void UpdatePassableMap(RectInt bounds, bool passable) {
            _map.UpdateMap(bounds, passable);
            if(Algorithms.Count != 0) {
                foreach (var a in Algorithms.Values) {
                    a.UpdateMap(bounds, passable);
                }
            }
        }
        
        private Queue<PathFindingRequest> _requestQueue = new();
        private Queue<PathFindingRequest> _pathCache;
        public void FindPath(PathFindingRequest request) {
            if (request == null) {
                Debug.LogError("PathFinder: request is null");
                return;
            }
            if (!_map.IsPassable(request.StartPos.x, request.StartPos.y) || 
                !_map.IsPassable(request.EndPos.x, request.EndPos.y)) {
                Debug.LogError($"PathFinder: StartPos{request.StartPos} or EndPos{request.EndPos.x} is not passable or out of range");
                return;
            }

            if (!request.NeedHandleImmediately) {
                _requestQueue.Enqueue(request);
                return;
            }

            if (!request.CanUseCache) {
                var a = GetAlgorithm(request.Algorithm);
                var resultPath = a.FindPath(request.StartPos, request.EndPos);
                request.ResultPath = resultPath;
                var p = GetReprocess(request.Reprocess);
                request.ReprocessedPath = p != null ? p.ReprocessPath(request.ResultPath, _map) : request.ResultPath;
            }
        }

        private IPathFinderAlgorithm GetAlgorithm(PathFinderAlgorithms algorithmType) {
            if (Algorithms.TryGetValue(algorithmType, out var a)) {
                return a;
            }

            a = algorithmType switch {
                PathFinderAlgorithms.JPS => new JPS(),
                PathFinderAlgorithms.JPSPlus => new JPSPlus(),
                PathFinderAlgorithms.AStar => new AStart(),
                _ => throw new ArgumentOutOfRangeException(nameof(algorithmType), algorithmType, null)
            };
            a.InitMap(_map);
            Algorithms.Add(algorithmType, a);
            return a;
        }

        private IPathReprocess GetReprocess(PathReprocesses reprocessType) {
            if (reprocessType == PathReprocesses.None) {
                return null;
            }
            if (Reprocesses.TryGetValue(reprocessType, out var p)) {
                return p;
            }

            p = reprocessType switch {
                PathReprocesses.Default => new DefaultPathSmooth(),
                _ => throw new ArgumentOutOfRangeException(nameof(reprocessType), reprocessType, null)
            };
            Reprocesses.Add(reprocessType ,p);
            return p;
        }

        #region Debug相关
        [ShowInInspector]
        public bool[,] TestMap = {
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true, false, false, false,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true, false,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true, false,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  },
            { true,  true,  true,  true,  true,  true,  true,  true,  true,  true  }
        };
        public bool isDebug;
        public PathFinderAlgorithms debugAlgorithm;
        public PathReprocesses debugPathReprocesses;
        private PathFindingRequest _debugRequest;
        private Stopwatch _stopwatch;

        [Button]
        private void DebugFindPath(Vector2Int start, Vector2Int end) {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            PathFindingRequest request = new PathFindingRequest(start, end, debugAlgorithm, debugPathReprocesses, false, true);
            FindPath(request);
            _debugRequest = request;
            _stopwatch.Stop();
            Debug.Log($"Pathfinding completed in {_stopwatch.ElapsedMilliseconds} ms.");
        }
        
        void OnDrawGizmos() {
            if (!isDebug || _map == null) {
                return;
            }

            IPathFinderAlgorithm a = null;
            if (_debugRequest != null) {
                a = GetAlgorithm(_debugRequest.Algorithm);
            }

            Gizmos.color = Color.gray; // 默认颜色设置为灰色
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
            if (path == null || path.ReprocessedPath == null || path.ReprocessedPath.Count == 0) {
                return;
            }
            Vector3 oPos = transform.position + new Vector3(0.5f, 0.5f);
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