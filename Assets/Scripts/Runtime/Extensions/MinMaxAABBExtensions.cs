using Unity.Burst;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;

namespace Rafasixteen.Runtime.ChunkLab
{
    [BurstCompile]
    public static class MinMaxAABBExtensions
    {
        public static MinMaxAABB GetDivided(this MinMaxAABB bounds, int3 amount)
        {
            int3 min = clmath.Div((int3)bounds.Min, amount);
            int3 max = clmath.DivUp((int3)bounds.Max, amount);
            return new(min, max);
        }

        public static MinMaxAABB GetExpanded(this MinMaxAABB bounds, int3 amount)
        {
            int3 min = (int3)bounds.Min - amount;
            int3 max = (int3)bounds.Max + amount;
            return new(min, max);
        }
    }
}