using System;
using Unity.Collections;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class ChunkStateManager : IDisposable
    {
        private NativeHashMap<ChunkId, EChunkState> _states;
        private NativeHashMap<ChunkId, EChunkState> _deferredStates;

        public ChunkStateManager()
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(ChunkStateManager)))
            {
                _states = new(1, Allocator.Persistent);
                _deferredStates = new(1, Allocator.Persistent);
            }
        }

        public ChunkSchedulerManager ChunkSchedulerManager { get; set; }
        public ChunkDependencyManager ChunkDependencyManager { get; set; }
        public LayerManager LayerManager { get; set; }

        public EChunkState GetState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(GetState)))
            {
                return _states[chunkId];
            }
        }

        public void SetState(ChunkId chunkId, EChunkState value)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(SetState)))
            {
                _states[chunkId] = value;
                OnChunkStateChanged(chunkId, value);
            }
        }

        public void RemoveState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(RemoveState)))
            {
                ChunkLabLogger.Log($"Removing state of {chunkId}.");
                _states.Remove(chunkId);
            }
        }

        public bool HasState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(HasState)))
            {
                return _states.ContainsKey(chunkId);
            }
        }

        public EChunkState GetDeferredState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(GetDeferredState)))
            {
                return _deferredStates[chunkId];
            }
        }

        public void SetDeferredState(ChunkId chunkId, EChunkState value)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(SetDeferredState)))
            {
                ChunkLabLogger.Log($"Setting deferred state of {chunkId} to {value}.");
                _deferredStates[chunkId] = value;
            }
        }

        public void RemoveDeferredState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(RemoveDeferredState)))
            {
                ChunkLabLogger.Log($"Removing deferred state of {chunkId}.");
                _deferredStates.Remove(chunkId);
            }
        }

        public bool HasDeferredState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(HasDeferredState)))
            {
                return _deferredStates.ContainsKey(chunkId);
            }
        }

        public NativeArray<ChunkId> GetChunkIdsOfLayer(LayerId layerId, Allocator allocator)
        {
            NativeList<ChunkId> chunkIds = new(0, allocator);

            using NativeArray<ChunkId> keyArray = _states.GetKeyArray(Allocator.Temp);

            for (int i = 0; i < keyArray.Length; i++)
            {
                ChunkId chunkId = keyArray[i];

                if (chunkId.LayerId == layerId)
                    chunkIds.Add(chunkId);
            }

            return chunkIds.AsArray();
        }

        public void Dispose()
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(Dispose)))
            {
                _states.Dispose();
                _deferredStates.Dispose();
            }
        }

        private void OnChunkStateChanged(ChunkId chunkId, EChunkState newState)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkStateChanged)))
            {
                ChunkLabLogger.Log($"Chunk state of {chunkId} changed to {newState}.");

                switch (newState)
                {
                    case EChunkState.Loaded:
                        OnChunkLoaded(chunkId);
                        break;
                    case EChunkState.Unloaded:
                        OnChunkUnloaded(chunkId);
                        break;
                    case EChunkState.Loading:
                        OnChunkLoading(chunkId);
                        break;
                    case EChunkState.Unloading:
                        OnChunkUnloading(chunkId);
                        break;
                    case EChunkState.AwaitingLoading:
                        OnChunkAwaitingLoading(chunkId);
                        break;
                    case EChunkState.AwaitingUnloading:
                        OnChunkAwaitingUnloading(chunkId);
                        break;
                }
            }
        }

        private void OnChunkLoaded(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkLoaded)))
            {
                if (HasDeferredState(chunkId))
                {
                    ChunkSchedulerManager.ScheduleChunk(chunkId, GetDeferredState(chunkId));
                    RemoveDeferredState(chunkId);
                }

                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.OnChunkLoadedInternal(chunkId);
            }
        }

        private void OnChunkUnloaded(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkUnloaded)))
            {
                //ChunkSchedulerManager.ScheduleChunkDependenciesOf(chunkId, EChunkState.AwaitingUnloading);
                //ChunkDependencyManager.RemoveAllDependencies(chunkId);

                using (NativeArray<ChunkId> dependencies = ChunkDependencyManager.GetDependencies(chunkId, Allocator.Temp))
                {
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        ChunkId dependencyId = dependencies[i];
                        ChunkDependencyManager.RemoveDependency(dependencyId, chunkId);
                        ChunkSchedulerManager.ScheduleChunk(dependencyId, EChunkState.AwaitingUnloading);
                    }
                }

                ChunkDependencyManager.RemoveAllDependents(chunkId);

                RemoveState(chunkId);

                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.ReleaseChunk(chunkId);

                if (HasDeferredState(chunkId))
                {
                    ChunkSchedulerManager.ScheduleChunk(chunkId, GetDeferredState(chunkId));
                    RemoveDeferredState(chunkId);
                }

                layer.OnChunkUnloadedInternal(chunkId);
            }
        }

        private void OnChunkLoading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkLoading)))
            {
                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.OnChunkLoadingInternal(chunkId);
            }
        }

        private void OnChunkUnloading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkUnloading)))
            {
                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.OnChunkUnloadingInternal(chunkId);
            }
        }

        private void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkAwaitingLoading)))
            {
                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.OnChunkAwaitingLoadingInternal(chunkId);
            }
        }

        private void OnChunkAwaitingUnloading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkAwaitingUnloading)))
            {
                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.OnChunkAwaitingUnloadingInternal(chunkId);
            }
        }
    }
}