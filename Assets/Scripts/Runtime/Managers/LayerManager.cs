using System.Collections.Generic;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class LayerManager
    {
        private Dictionary<LayerId, LayerBase> _layerInstances;

        public LayerManager()
        {

        }

        public LayerBase GetLayer(LayerId layerId)
        {
            return _layerInstances[layerId];
        }
    }
}
