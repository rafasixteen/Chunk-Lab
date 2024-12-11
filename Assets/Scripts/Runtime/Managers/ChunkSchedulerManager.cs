using System;
using Unity.Collections;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class ChunkSchedulerManager : IDisposable
    {
        private NativeQueue<ChunkId> _queue;
        private NativeHashSet<ChunkId> _inQueue;

        public ChunkSchedulerManager()
        {
            _queue = new(Allocator.Persistent);
            _inQueue = new(1, Allocator.Persistent);
        }

        public ChunkStateManager ChunkStateManager { get; set; }

        public ChunkDependencyManager ChunkDependencyManager { get; set; }

        public int Count => _queue.Count;

        public void ScheduleChunk(ChunkId chunkId, EChunkState desiredState)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(ScheduleChunk)))
            {
                ThrowIfNotAnyAwaitingState(desiredState);

                if (IsNewChunk(chunkId))
                {
                    // A chunk can be AwaitingLoading and be set to AwaitingUnloading, when that happens,
                    // that chunk may have dependencies registered, but those dependencies may have not
                    // been loaded yet. That happens if a new chunk is scheduled to be unloaded, and once
                    // that happens we can just ignore it here.
                    if (desiredState == EChunkState.AwaitingUnloading)
                        return;

                    ChunkStateManager.SetState(chunkId, desiredState);
                    Enqueue(chunkId);
                    return;
                }

                EChunkState currentState = ChunkStateManager.GetState(chunkId);

                if (desiredState == EChunkState.AwaitingLoading)
                    HandleTransitionToAwaitingLoading(chunkId, currentState);
                else
                    HandleTransitionToAwaitingUnloading(chunkId, currentState);
            }
        }

        private void HandleTransitionToAwaitingUnloading(ChunkId chunkId, EChunkState currentState)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(HandleTransitionToAwaitingUnloading)))
            {
                switch (currentState)
                {
                    case EChunkState.Loaded:
                        if (!ChunkDependencyManager.HasDependents(chunkId))
                        {
                            ChunkStateManager.SetState(chunkId, EChunkState.AwaitingUnloading);
                            Enqueue(chunkId);
                        }
                        else
                        {
                            ChunkLabLogger.LogWarning($"Chunk {chunkId} cannot transition from {currentState} to {EChunkState.AwaitingUnloading} because it has dependents. Ensure all dependents are unloaded or restructured.");
                        }
                        break;
                    case EChunkState.AwaitingLoading:
                        using (NativeArray<ChunkId> dependencies = ChunkDependencyManager.GetDependencies(chunkId, Allocator.Temp))
                        {
                            for (int i = 0; i < dependencies.Length; i++)
                            {
                                ChunkId dependencyId = dependencies[i];
                                ChunkDependencyManager.RemoveDependency(dependencyId, chunkId);
                                ScheduleChunk(dependencyId, EChunkState.AwaitingUnloading);
                            }
                        }

                        using (NativeArray<ChunkId> dependents = ChunkDependencyManager.GetDependents(chunkId, Allocator.Temp))
                        {
                            for (int i = 0; i < dependents.Length; i++)
                                ChunkDependencyManager.RemoveDependency(dependents[i], chunkId);
                        }

                        ChunkStateManager.RemoveState(chunkId);
                        RemoveFromQueue(chunkId);
                        break;
                    case EChunkState.AwaitingUnloading:
                        // Still don't know why this is happening.
                        // This temporary fix is from 11/12/2024.
                        Enqueue(chunkId);
                        break;
                    default:
                        throw new InvalidOperationException($"Chunk {chunkId} cannot transition from {currentState} to {EChunkState.AwaitingUnloading}.");
                }
            }
        }

        private void HandleTransitionToAwaitingLoading(ChunkId chunkId, EChunkState currentState)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(HandleTransitionToAwaitingLoading)))
            {
                switch (currentState)
                {
                    case EChunkState.AwaitingLoading:
                        Enqueue(chunkId);
                        break;
                    case EChunkState.AwaitingUnloading:
                        ChunkStateManager.SetState(chunkId, EChunkState.AwaitingLoading);
                        break;
                    case EChunkState.Loaded:
                        // This happens when a chunk with some dependencies that are already loaded
                        // it will still need for it's other dependencies to load, and it will schedule
                        // all dependencies, even the ones that are already loaded, when that happens,
                        // we catch that here and can just ignore it.
                        break;
                    default:
                        throw new InvalidOperationException($"Chunk {chunkId} cannot transition from {currentState} to {EChunkState.AwaitingLoading}.");
                }
            }
        }

        public bool TryDequeue(out ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(TryDequeue)))
            {
                while (_queue.Count > 0)
                {
                    chunkId = _queue.Dequeue();

                    if (_inQueue.Remove(chunkId))
                        return true;
                }

                chunkId = default;
                return false;
            }
        }

        public void Dispose()
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(Dispose)))
            {
                _queue.Dispose();
                _inQueue.Dispose();
            }
        }

        private bool IsNewChunk(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(IsNewChunk)))
            {
                return !_inQueue.Contains(chunkId) && !ChunkStateManager.HasState(chunkId);
            }
        }

        private void Enqueue(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(Enqueue)))
            {
                if (_inQueue.Add(chunkId))
                {
                    _queue.Enqueue(chunkId);
                    ChunkLabLogger.Log($"Enqueued {chunkId} to {ChunkStateManager.GetState(chunkId)}.");
                }
            }
        }

        private void RemoveFromQueue(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(RemoveFromQueue)))
            {
                if (_inQueue.Remove(chunkId))
                    ChunkLabLogger.Log($"Chunk {chunkId} removed from the queue.");
                else
                    ChunkLabLogger.LogWarning($"Chunk {chunkId} could not be removed from the queue because it was never in the queue.");
            }
        }

        private void ThrowIfNotAnyAwaitingState(EChunkState state)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(ThrowIfNotAnyAwaitingState)))
            {
                if (state != EChunkState.AwaitingLoading && state != EChunkState.AwaitingUnloading)
                    throw new ArgumentException($"Cannot schedule a chunk to {state}. Only {EChunkState.AwaitingLoading} or {EChunkState.AwaitingUnloading} are allowed.");
            }
        }
    }
}