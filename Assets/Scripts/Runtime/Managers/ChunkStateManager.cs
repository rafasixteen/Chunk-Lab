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
                EChunkState oldState = !_states.ContainsKey(chunkId) ? EChunkState.None : _states[chunkId];
                _states[chunkId] = value;
                OnChunkStateChanged(chunkId, oldState, value);
            }
        }

        public void RemoveState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(RemoveState)))
            {
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
                _deferredStates[chunkId] = value;
            }
        }

        public void RemoveDeferredState(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(RemoveDeferredState)))
            {
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

        public bool AreChunksLoaded(NativeArray<ChunkId> chunkIds)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(AreChunksLoaded)))
            {
                for (int i = 0; i < chunkIds.Length; i++)
                {
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
                _states.Dispose();
                _deferredStates.Dispose();
            }
        }

        private void OnChunkStateChanged(ChunkId chunkId, EChunkState oldState, EChunkState newState)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkStateChanged)))
            {
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
            }
        }

        private void OnChunkUnloaded(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkUnloaded)))
            {
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
                LayerManager.GetLayer(chunkId.LayerId).ReleaseChunk(chunkId);

                if (HasDeferredState(chunkId))
                {
                    ChunkSchedulerManager.ScheduleChunk(chunkId, GetDeferredState(chunkId));
                    RemoveDeferredState(chunkId);
                }
            }
        }

        private void OnChunkLoading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkLoading)))
            {

            }
        }

        private void OnChunkUnloading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkUnloading)))
            {

            }
        }

        private void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkAwaitingLoading)))
            {

            }
        }

        private void OnChunkAwaitingUnloading(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(OnChunkAwaitingUnloading)))
            {

            }
        }
    }
}