using System;
using Unity.Mathematics;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    internal sealed class LeafLayer
    {
        private readonly LayerBase _layer;

        private int3 _lastProcessedChunkCoords = new(int.MaxValue);

        public LeafLayer(LayerBase layer)
        {
            _layer = layer;
        }

        internal void UpdatePosition(float3 worldPosition)
        {
            int3 currentChunkCoords = GetChunkCoords(worldPosition);

            if (!currentChunkCoords.Equals(_lastProcessedChunkCoords))
            {
                ChunkId chunkIdToLoad = ChunkIdFromCoords(currentChunkCoords);
                _layer.ChunkSchedulerManager.ScheduleChunk(chunkIdToLoad, EChunkState.AwaitingLoading);

                if (!_lastProcessedChunkCoords.Equals(int.MaxValue))
                {
                    ChunkId chunkIdToUnload = ChunkIdFromCoords(_lastProcessedChunkCoords);
                    _layer.ChunkSchedulerManager.ScheduleChunk(chunkIdToUnload, EChunkState.AwaitingUnloading);
                }

                _lastProcessedChunkCoords = currentChunkCoords;
            }
        }

        private ChunkId ChunkIdFromCoords(int3 chunkCoords)
        {
            return new(_layer.Id, chunkCoords, _layer.Settings.ChunkSize);
        }

        private int3 GetChunkCoords(float3 worldPosition)
        {
            int3 position = (int3)math.floor(worldPosition);
            return clmath.Div(position, _layer.Settings.ChunkSize);
        }
    }
}