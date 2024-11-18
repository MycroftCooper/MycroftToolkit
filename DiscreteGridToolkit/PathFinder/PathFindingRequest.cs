using System;
using System.Collections.Generic;
using MycroftToolkit.QuickCode.FrameTask;
using UnityEngine;

namespace PathFinding {
    public class PathFindingRequest {
        public readonly Vector2Int StartPos;
        public readonly Vector2Int EndPos;

        public readonly bool NeedBestSolution;
        public readonly PathFinderAlgorithms Algorithm;
        public readonly HeuristicTypes HeuristicType;
        public readonly PathReprocesses Reprocess;
        
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
            PathFinderAlgorithms algorithm, bool needBestSolution, HeuristicTypes heuristicType, PathReprocesses reprocess,
            bool canUseCache = false, Action<PathFindingRequest> pathFoundHandler = null) {
            StartPos = startPos;
            EndPos = endPos;
            Algorithm = algorithm;
            NeedBestSolution = needBestSolution;
            HeuristicType = heuristicType;
            Reprocess = reprocess;
            CanUseCache = canUseCache;
            PathFoundHandler = pathFoundHandler;
        }
        
        public override bool Equals(object obj) {
            return obj is PathFindingRequest other && 
                   StartPos.Equals(other.StartPos) && 
                   EndPos.Equals(other.EndPos);
        }

        public override int GetHashCode() => HashCode.Combine(StartPos, EndPos);
        
        public override string ToString() {
            return "PathFindingRequest {{\n" +
                   $"  StartPos: {StartPos},\tEndPos: {EndPos},\n" +
                   $"  NeedBestSolution: {NeedBestSolution},\tAlgorithm: {Algorithm},\tHeuristicType: {HeuristicType},\n" +
                   $"  Reprocess: {Reprocess},\n" +
                   $"  CanUseCache: {CanUseCache},\n" +
                   $"  ResultPathCount: {(ResultPath != null ? ResultPath.Count.ToString() : "null")}\t" +
                   $"ReprocessedPathCount: {(ReprocessedPath != null ? ReprocessedPath.Count.ToString() : "null")}\n" +
                   "}}";
        }
    }

    public class PathFindingFrameTask : FrameTask {
        public PathFinder Finder;
        public PathFindingRequest Request;

        public PathFindingFrameTask(PathFinder finder, PathFindingRequest request, float priority = 0) : base(priority) {
            Finder = finder;
            Request = request;
        }

        protected override void Execute() {
            Finder.ExecuteRequest(Request);
            SetCompleted();
        }
    }
}