using System;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;

namespace Rafasixteen.Runtime.ChunkLab
{
    public abstract class ChunkBase
    {
        private string _cachedName = default;

        public string Name => _cachedName ??= GetType().Name;

        public ChunkId Id { get; internal set; }

        public int3 Coords => Id.Coords;

        public int3 Size => Id.Size;

        public int3 Position => Coords * Size;

        public MinMaxAABB Bounds => new(Position, Position + Size);

        public EChunkState State
        {
            get => Layer.ChunkLabManager.ChunkStateManager.GetState(Id);
            private set => Layer.ChunkLabManager.ChunkStateManager.SetState(Id, value);
        }

        internal LayerBase Layer { get; set; }

        internal void StartLoadingInternal()
        {
            using (ProfilerUtility.StartSample(Name, nameof(StartLoadingInternal)))
            {
                if (State == EChunkState.Loaded || State == EChunkState.Loading)
                    throw new InvalidOperationException($"Cannot start loading Chunk {Id}, as it is already {State}.");

                if (State == EChunkState.Unloading)
                    throw new InvalidOperationException($"Cannot start loading Chunk {Id} while it is unloading.");

                State = EChunkState.Loading;
                StartLoading();
            }
        }

        internal void StartUnloadingInternal()
        {
            using (ProfilerUtility.StartSample(Name, nameof(StartUnloadingInternal)))
            {
                if (State == EChunkState.Unloaded || State == EChunkState.Unloading)
                    throw new InvalidOperationException($"Cannot start unloading Chunk {Id}, as it is already {State}.");

                if (State == EChunkState.Loading)
                    throw new InvalidOperationException($"Cannot start unloading Chunk {Id} while it is loading.");

                State = EChunkState.Unloading;
                StartUnloading();
            }
        }

        protected void FinishLoading()
        {
            using (ProfilerUtility.StartSample(Name, nameof(FinishLoading)))
            {
                if (State != EChunkState.Loading)
                    throw new InvalidOperationException($"Cannot finish loading Chunk {Id}, as it is not in a loading state.");

                State = EChunkState.Loaded;
            }
        }

        protected void FinishUnloading()
        {
            using (ProfilerUtility.StartSample(Name, nameof(FinishUnloading)))
            {
                if (State != EChunkState.Unloading)
                    throw new InvalidOperationException($"Cannot finish unloading Chunk {Id}, as it is not in an unloading state.");

                State = EChunkState.Unloaded;
            }
        }

        protected abstract void StartLoading();

        protected abstract void StartUnloading();
    }
}