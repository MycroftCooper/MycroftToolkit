using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class PathFindingRequest {
        public readonly Vector2Int StartPos;
        public readonly Vector2Int EndPos;

        public readonly bool NeedBestSolution;
        public readonly PathFinderAlgorithms Algorithm;
        public readonly string HeuristicType;
        public readonly PathReprocesses Reprocess;

        public readonly bool NeedHandleImmediately;
        public readonly bool CanUseCache;

        public bool IsFound => ResultPath is { Count: > 0 };
        public bool IsFinished => ReprocessedPath != null;
        public List<Vector2Int> ResultPath { get; set; }
        public List<Vector2Int> ReprocessedPath {
            get => _reprocessedPath;
            set {
                _reprocessedPath = value;
                PathFoundHandler?.Invoke(this);
            }
        }
        private List<Vector2Int> _reprocessedPath;
        
        public readonly Action<PathFindingRequest> PathFoundHandler;

        public PathFindingRequest(Vector2Int startPos, Vector2Int endPos, 
            PathFinderAlgorithms algorithm, bool needBestSolution, string heuristicType, PathReprocesses reprocess, 
            bool canUseCache = false, bool needHandleImmediately = false, Action<PathFindingRequest> pathFoundHandler = null) {
            StartPos = startPos;
            EndPos = endPos;
            Algorithm = algorithm;
            NeedBestSolution = needBestSolution;
            HeuristicType = heuristicType;
            Reprocess = reprocess;
            CanUseCache = canUseCache;
            NeedHandleImmediately = needHandleImmediately;
            PathFoundHandler = pathFoundHandler;
        }
    }
}