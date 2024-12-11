using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    [DisallowMultipleComponent, DefaultExecutionOrder(-10)]
    public class ChunkLabManager : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private LayerGraphData _layerGraphData;
        [SerializeField] private Transform _generationSource;
        [SerializeField, Range(1, 1024)] private int _maxChunksPerFrame = 32;

        [SerializeField] private bool _enableVisualizer;
        [SerializeField] private bool _enableLogger;

        #endregion

        #region PROPERTIES

        public ChunkDependencyManager ChunkDependencyManager { get; private set; }
        public ChunkStateManager ChunkStateManager { get; private set; }
        public ChunkSchedulerManager ChunkSchedulerManager { get; private set; }
        public ChunkProcessingManager ChunkProcessingManager { get; private set; }
        public LayerManager LayerManager { get; private set; }

        #endregion

        #region METHODS

        #region LIFECYCLE

        private void Start()
        {
            InitializeManagers();

            for (int i = 0; i < LayerManager.Count; i++)
                LayerManager[i].ChunkLabManager = this;

            for (int i = 0; i < LayerManager.Count; i++)
                LayerManager[i].OnStartInternal();
        }

        private void Update()
        {
            LayerManager.LeafLayer.UpdatePosition(_generationSource.position);
            ChunkProcessingManager.Process(_maxChunksPerFrame);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < LayerManager.Count; i++)
                LayerManager[i].OnDestroyInternal();

            DisposeManagers();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !_enableVisualizer)
                return;

            for (int i = 0; i < LayerManager.Count; i++)
                LayerManager[i].OnDrawGizmosInternal();
        }

        private void OnValidate()
        {
            ChunkLabLogger.SetActive(_enableLogger);
        }

        #endregion

        public void SetGenerationSource(Transform transform)
        {
            _generationSource = transform;
        }

        private void InitializeManagers()
        {
            ChunkDependencyManager = new();
            ChunkStateManager = new();
            ChunkSchedulerManager = new();
            ChunkProcessingManager = new();
            LayerManager = new(_layerGraphData);

            ChunkDependencyManager.ChunkStateManager = ChunkStateManager;
            ChunkDependencyManager.LayerManager = LayerManager;

            ChunkStateManager.ChunkSchedulerManager = ChunkSchedulerManager;
            ChunkStateManager.ChunkDependencyManager = ChunkDependencyManager;
            ChunkStateManager.LayerManager = LayerManager;

            ChunkSchedulerManager.ChunkStateManager = ChunkStateManager;
            ChunkSchedulerManager.ChunkDependencyManager = ChunkDependencyManager;

            ChunkProcessingManager.ChunkSchedulerManager = ChunkSchedulerManager;
            ChunkProcessingManager.ChunkStateManager = ChunkStateManager;
            ChunkProcessingManager.ChunkDependencyManager = ChunkDependencyManager;
            ChunkProcessingManager.LayerManager = LayerManager;
        }

        private void DisposeManagers()
        {
            ChunkDependencyManager.Dispose();
            ChunkStateManager.Dispose();
            ChunkSchedulerManager.Dispose();
            //ChunkProcessingManager.Dispose();
            //LayerManager.Dispose();
        }

        #endregion
    }
}