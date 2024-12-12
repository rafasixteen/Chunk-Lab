using System;
using Unity.Mathematics;
using UnityEngine;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    public static class ComputeShaderExtensions
    {
        public static void Dispatch(this ComputeShader computeShader, int kernelID, uint3 numberOfInvocations)
        {
            if (math.any(numberOfInvocations < 0))
                throw new ArgumentException("Number of invocations must be greater than or equal to zero.");

            uint3 threadGroupSize = new();
            computeShader.GetKernelThreadGroupSizes(kernelID, out threadGroupSize.x, out threadGroupSize.y, out threadGroupSize.z);
            int3 threadGroups = (int3)math.ceil(numberOfInvocations / threadGroupSize);

            computeShader.Dispatch(kernelID, threadGroups.x, threadGroups.y, threadGroups.z);
        }

        public static void SetInt3(this ComputeShader computeShader, string name, int3 value)
        {
            computeShader.SetInts(name, value.x, value.y, value.z);
        }
    }
}