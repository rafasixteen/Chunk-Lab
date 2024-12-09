using Rafasixteen.Runtime.ChunkLab;
using UnityEngine;

namespace Rafasixteen
{
    public class LayerA : Layer<LayerA, LayerA.Chunk>
    {
        public class Chunk : Chunk<Chunk, LayerA>
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

    public class LayerB : Layer<LayerB, LayerB.Chunk>
    {
        public class Chunk : Chunk<Chunk, LayerB>
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
}