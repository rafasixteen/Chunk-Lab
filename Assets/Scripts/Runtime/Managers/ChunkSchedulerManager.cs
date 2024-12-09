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

        public int Count => _queue.Count;

        public void ScheduleChunk(ChunkId chunkId, EChunkState desiredState)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(ScheduleChunk)))
            {
                if (desiredState != EChunkState.AwaitingLoading && desiredState != EChunkState.AwaitingUnloading)
                    throw new ArgumentException($"Cannot schedule a chunk to {desiredState}. Only {EChunkState.AwaitingLoading} or {EChunkState.AwaitingUnloading} are allowed.");

                // If the chunk is still in the queue it means it's still waiting
                // to be loaded or unloaded, so we just update it's state.
                if (_inQueue.Contains(chunkId))
                {
                    ChunkStateManager.SetState(chunkId, desiredState);
                    return;
                }

                // If we got here it means the chunk is not in the queue, and if it
                // doesn't have a state, it means it's a completely new chunk, so we
                // just add it to the queue and set it's state for later processing.
                if (!ChunkStateManager.HasState(chunkId))
                {
                    Enqueue(chunkId);
                    ChunkStateManager.SetState(chunkId, desiredState);
                    return;
                }

                EChunkState currentState = ChunkStateManager.GetState(chunkId);

                // And if we're here it means the chunk can be in either of the following states:
                // Loaded, Loading or Unloading.

                // If this chunk is Loading or Unloading we have to wait for it's current operation
                // (Loading or Unloading) to finish before we can schedule it to the desired state.

                // So we add it to the deferred states and once the current operation finishes
                // we'll schedule it to the current deferred state.
                if (currentState == EChunkState.Loading || currentState == EChunkState.Unloading)
                {
                    ChunkStateManager.SetDeferredState(chunkId, desiredState);
                    return;
                }

                // Chunk is already loaded and we want to load it again, so we just ignore it.
                if (currentState == EChunkState.Loaded && desiredState == EChunkState.AwaitingLoading)
                {
                    Debug.LogWarning($"Chunk {chunkId} is already loaded and we're trying to load it again.");
                    return;
                }

                Enqueue(chunkId);
                ChunkStateManager.SetState(chunkId, desiredState);
            }
        }

        public ChunkId Dequeue()
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(Dequeue)))
            {
                ChunkId chunkId = _queue.Dequeue();
                _inQueue.Remove(chunkId);
                return chunkId;
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

        private void Enqueue(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(Dispose)))
            {
                _queue.Enqueue(chunkId);
                _inQueue.Add(chunkId);
            }
        }
    }
}