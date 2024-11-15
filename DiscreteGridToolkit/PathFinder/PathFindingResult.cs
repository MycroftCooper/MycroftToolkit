using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding {
    public class PathFindingRequest : IComparable<PathFindingRequest> {
        public readonly int Priority;
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
            PathFinderAlgorithms algorithm, bool needBestSolution, string heuristicType, PathReprocesses reprocess, int priority = 0,
            bool canUseCache = false, bool needHandleImmediately = false, Action<PathFindingRequest> pathFoundHandler = null) {
            StartPos = startPos;
            EndPos = endPos;
            Algorithm = algorithm;
            NeedBestSolution = needBestSolution;
            HeuristicType = heuristicType;
            Reprocess = reprocess;
            Priority = priority;
            CanUseCache = canUseCache;
            NeedHandleImmediately = needHandleImmediately;
            PathFoundHandler = pathFoundHandler;
        }
        
        public int CompareTo(PathFindingRequest other) {
            return other == null ? 1 : Priority.CompareTo(other.Priority); // 升序，小值排前
        }
        
        public override bool Equals(object obj) {
            return obj is PathFindingRequest other && 
                   Priority == other.Priority && 
                   StartPos.Equals(other.StartPos) && 
                   EndPos.Equals(other.EndPos);
        }

        public override int GetHashCode() => HashCode.Combine(Priority, StartPos, EndPos);
        
        public override string ToString() {
            return "PathFindingRequest {{\n" +
                   $"  Priority: {Priority},\tStartPos: {StartPos},\tEndPos: {EndPos},\n" +
                   $"  NeedBestSolution: {NeedBestSolution},\tAlgorithm: {Algorithm},\tHeuristicType: {HeuristicType},\n" +
                   $"  Reprocess: {Reprocess},\n" +
                   $"  NeedHandleImmediately: {NeedHandleImmediately},\tCanUseCache: {CanUseCache},\n" +
                   $"  ResultPathCount: {(ResultPath != null ? ResultPath.Count.ToString() : "null")}\t" +
                   $"ReprocessedPathCount: {(ReprocessedPath != null ? ReprocessedPath.Count.ToString() : "null")}\n" +
                   "}}";
        }
    }
}