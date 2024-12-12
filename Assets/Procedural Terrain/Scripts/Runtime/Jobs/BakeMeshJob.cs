using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    [BurstCompile]
    public readonly struct BakeMeshJob : IJob
    {
        private readonly int meshId;

        public BakeMeshJob(Mesh mesh)
        {
            meshId = mesh.GetInstanceID();
        }

        void IJob.Execute()
        {
            Physics.BakeMesh(meshId, false);
        }
    }
}
