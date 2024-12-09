using Unity.Mathematics;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class LayerC : Layer<LayerC, LayerC.Chunk>
    {
        public class Chunk : Chunk<Chunk, LayerC>
        {
            protected override void StartLoading()
            {
                Debug.Log($"[{Name}] StartLoading");
                FinishLoading();
            }

            protected override void StartUnloading()
            {
                Debug.Log($"[{Name}] StartUnloading");
                FinishUnloading();
            }
        }
    }

    public sealed class LeafLayer : Layer<LeafLayer, LeafLayer.Chunk>
    {
        private int3 _lastProcessedChunkCoords = new(int.MaxValue);

        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            LayerC layer = LayerManager.GetLayer<LayerC>();

            ChunkId dependencyId = new(layer.Id, chunkId.Coords, layer.Settings.ChunkSize);

            ChunkDependencyManager.AddDependency(dependencyId, chunkId);
        }

        internal void UpdatePosition(float3 worldPosition)
        {
            int3 currentChunkCoords = GetChunkCoords(worldPosition);

            if (!currentChunkCoords.Equals(_lastProcessedChunkCoords))
            {
                ChunkId chunkIdToLoad = ChunkIdFromCoords(currentChunkCoords);
                ChunkLabManager.ChunkSchedulerManager.ScheduleChunk(chunkIdToLoad, EChunkState.AwaitingLoading);

                if (!_lastProcessedChunkCoords.Equals(int.MaxValue))
                {
                    ChunkId chunkIdToUnload = ChunkIdFromCoords(_lastProcessedChunkCoords);
                    ChunkLabManager.ChunkSchedulerManager.ScheduleChunk(chunkIdToUnload, EChunkState.AwaitingUnloading);
                }

                _lastProcessedChunkCoords = currentChunkCoords;
            }
        }

        private ChunkId ChunkIdFromCoords(int3 chunkCoords)
        {
            return new(Id, chunkCoords, Settings.ChunkSize);
        }

        private int3 GetChunkCoords(float3 worldPosition)
        {
            int3 position = (int3)math.floor(worldPosition);
            return clmath.Div(position, Settings.ChunkSize);
        }

        public class Chunk : Chunk<Chunk, LeafLayer>
        {
            protected override void StartLoading()
            {
                FinishLoading();
            }

            protected override void StartUnloading()
            {
                FinishUnloading();
            }
        }
    }
}