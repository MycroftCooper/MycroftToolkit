using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class PathFindingRequest {
        public readonly Vector2Int StartPos;
        public readonly Vector2Int EndPos;

        public readonly PathFinderAlgorithms Algorithm;
        public readonly PathReprocesses Reprocess;
        public readonly string HeuristicType;

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
            PathFinderAlgorithms algorithm, PathReprocesses reprocess, string heuristicType,
            bool canUseCache = false, bool needHandleImmediately = false, Action<PathFindingRequest> pathFoundHandler = null) {
            StartPos = startPos;
            EndPos = endPos;
            Algorithm = algorithm;
            Reprocess = reprocess;
            HeuristicType = heuristicType;
            CanUseCache = canUseCache;
            NeedHandleImmediately = needHandleImmediately;
            PathFoundHandler = pathFoundHandler;
        }
    }
}