using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Domain;
using Cascade.Solver;

namespace Cascade.Application
{
    // Application-thread owner. Workers only read immutable level/goal definitions.
    public sealed class PuzzlePreparation : IDisposable
    {
        readonly Dictionary<string, SearchResult> cache = new Dictionary<string, SearchResult>();
        readonly int stateBudget;
        CancellationTokenSource cancellation;
        Task<SearchResult> task;
        string key;
        public SearchResult Result { get; private set; }
        public bool IsPending => task != null;
        public bool IsReady => Result?.Status == SearchStatus.Solved;
        public bool HasFailed { get; private set; }
        public PuzzlePreparation(int stateBudget = PuzzleSolver.DefaultStateBudget) { this.stateBudget = stateBudget; }
        public void Begin(LevelDefinition level, PuzzleGoal goal)
        {
            Cancel(); Result = null; HasFailed = false;
            key = level.PuzzleId + ":" + level.ContentHash + ":" + level.RulesVersion + ":" + goal.RequiredStampBits;
            if (cache.TryGetValue(key, out var found)) { Result = found; return; }
            cancellation = new CancellationTokenSource(); var token = cancellation.Token;
            task = Task.Run(() => PuzzleSolver.FindShortest(new PuzzleResolver(level), goal: goal,
                stateBudget: stateBudget, cancellationToken: token));
        }
        public bool Poll()
        {
            if (task == null || !task.IsCompleted) return false;
            var finished = task; task = null; cancellation.Dispose(); cancellation = null;
            if (finished.IsCanceled || finished.IsFaulted) {
                var ignored = finished.Exception; HasFailed = true; return true;
            }
            Result = finished.Result; HasFailed = !IsReady;
            if (IsReady) {
                if (cache.Count >= 32) cache.Clear();
                cache[key] = Result;
            }
            return true;
        }
        public void Cancel()
        {
            cancellation?.Cancel(); cancellation?.Dispose(); cancellation = null;
            if (task != null) task.ContinueWith(t => { var ignored = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            task = null;
        }
        public void Dispose() { Cancel(); cache.Clear(); }
    }
}
