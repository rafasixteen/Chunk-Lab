using Rafasixteen.JobManager;
using Rafasixteen.Runtime.ChunkLab;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class ChunkObject : MonoBehaviour
    {
        #region FIELDS

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;

        #endregion

        #region PROPERTIES

        public Mesh Mesh { get; private set; }

        public ChunkId Id { get; private set; }

        #endregion

        #region METHODS

        #region LIFECYCLE

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();

            Mesh = new();
        }

        private void OnDestroy()
        {
            Destroy(Mesh);
        }

        #endregion

        public void Initialize(ChunkId chunkId, Transform parent)
        {
            name = $"Coords: {chunkId.Coords} | Size: {chunkId.Size}";
            Id = chunkId;

            float3 position = new(chunkId.Coords * chunkId.Size);
            quaternion rotation = quaternion.identity;

            transform.SetPositionAndRotation(position, rotation);
            transform.SetParent(parent);
            SetActive(true);
        }

        public void ResetState()
        {
            name = $"Chunk";
            Id = default;

            float3 position = float3.zero;
            quaternion rotation = quaternion.identity;
            transform.SetPositionAndRotation(position, rotation);

            Mesh.Clear();
            SetMesh(null);
            SetCollider(null);
            SetActive(false);
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        public void SetMesh(Mesh mesh)
        {
            _meshFilter.sharedMesh = mesh;
        }

        public void SetCollider(Mesh mesh)
        {
            if (mesh == null)
            {
                _meshCollider.sharedMesh = null;
                return;
            }

            BakeMeshJob job = new(mesh);
            job.Schedule().SetCallback(() =>
            {
                _meshCollider.sharedMesh = mesh;
            });
        }

        #endregion
    }
}