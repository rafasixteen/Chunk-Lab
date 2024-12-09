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
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(SetState)}({chunkId}, {value})");
                _states[chunkId] = value;
                OnChunkStateChanged(chunkId, value);
            }
        }

        public void RemoveState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(RemoveState)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(RemoveState)}({chunkId})");
                _states.Remove(chunkId);
            }
        }

        public bool HasState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(HasState)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(HasState)}({chunkId})");
                return _states.ContainsKey(chunkId);
            }
        }

        public EChunkState GetDeferredState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(GetDeferredState)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(GetDeferredState)}({chunkId})");
                return _deferredStates[chunkId];
            }
        }

        public void SetDeferredState(ChunkId chunkId, EChunkState value)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(SetDeferredState)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(SetDeferredState)}({chunkId}, {value})");
                _deferredStates[chunkId] = value;
            }
        }

        public void RemoveDeferredState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(RemoveDeferredState)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(RemoveDeferredState)}({chunkId})");
                _deferredStates.Remove(chunkId);
            }
        }

        public bool HasDeferredState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(HasDeferredState)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(HasDeferredState)}({chunkId})");
                return _deferredStates.ContainsKey(chunkId);
            }
        }

        public bool AreChunksLoaded(NativeArray<ChunkId> chunkIds)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(AreChunksLoaded)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(AreChunksLoaded)}({ChunkLabLogger.ArrayToString(chunkIds)})");

                for (int i = 0; i < chunkIds.Length; i++)
                {
                    ChunkId chunkId = chunkIds[i];

                    if (!HasState(chunkId))
                        return false;

                    if (GetState(chunkIds[i]) != EChunkState.Loaded)
                        return false;
                }

                return true;
            }
        }

        public void Dispose()
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(Dispose)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(Dispose)}()");
                _states.Dispose();
                _deferredStates.Dispose();
            }
        }

        private void OnChunkStateChanged(ChunkId chunkId, EChunkState newState)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkStateChanged)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(OnChunkStateChanged)}({chunkId}, {newState})");

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
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(OnChunkLoaded)}({chunkId})");

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
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(OnChunkUnloaded)}({chunkId})");

                using (NativeArray<ChunkId> dependencies = ChunkDependencyManager.GetDependencies(chunkId, Allocator.Temp))
                {
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        ChunkId dependencyId = dependencies[i];
                        ChunkDependencyManager.RemoveDependency(chunkId, dependencyId);
                        ChunkSchedulerManager.ScheduleChunk(dependencyId, EChunkState.AwaitingUnloading);
                    }
                }

                using (NativeArray<ChunkId> dependents = ChunkDependencyManager.GetDependents(chunkId, Allocator.Temp))
                {
                    for (int i = 0; i < dependents.Length; i++)
                        ChunkDependencyManager.RemoveDependency(dependents[i], chunkId);
                }

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
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(OnChunkLoading)}({chunkId})");

                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.OnChunkLoadingInternal(chunkId);
            }
        }

        private void OnChunkUnloading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkUnloading)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(OnChunkUnloading)}({chunkId})");

                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.OnChunkUnloadingInternal(chunkId);
            }
        }

        private void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkAwaitingLoading)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(OnChunkAwaitingLoading)}({chunkId})");

                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.OnChunkAwaitingLoadingInternal(chunkId);
            }
        }

        private void OnChunkAwaitingUnloading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkAwaitingUnloading)))
            {
                ChunkLabLogger.Log($"{nameof(ChunkStateManager)}.{nameof(OnChunkAwaitingUnloading)}({chunkId})");

                LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                layer.OnChunkAwaitingUnloadingInternal(chunkId);
            }
        }
    }
}