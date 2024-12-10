using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    [BurstCompile]
    public static class clmath
    {
        #region DIVISON

        public static int Div(int x, int divisor)
        {
            return (x - (((x % divisor) + divisor) % divisor)) / divisor;
        }

        public static int DivUp(int x, int divisor)
        {
            return (x - (((x % divisor) - divisor) % divisor)) / divisor;
        }

        public static Vector3Int Div(Vector3Int vector, int divisor)
        {
            return new(Div(vector.x, divisor), Div(vector.y, divisor), Div(vector.z, divisor));
        }

        public static Vector3Int Div(Vector3Int vector, Vector3Int divisor)
        {
            return new(Div(vector.x, divisor.x), Div(vector.y, divisor.y), Div(vector.z, divisor.z));
        }

        public static Vector3Int DivUp(Vector3Int vector, int divisor)
        {
            return new(DivUp(vector.x, divisor), DivUp(vector.y, divisor), DivUp(vector.z, divisor));
        }

        public static Vector3Int DivUp(Vector3Int vector, Vector3Int divisor)
        {
            return new(DivUp(vector.x, divisor.x), DivUp(vector.y, divisor.y), DivUp(vector.z, divisor.z));
        }

        public static int3 Div(int3 vector, int divisor)
        {
            return new(Div(vector.x, divisor), Div(vector.y, divisor), Div(vector.z, divisor));
        }

        public static int3 Div(int3 vector, int3 divisor)
        {
            return new(Div(vector.x, divisor.x), Div(vector.y, divisor.y), Div(vector.z, divisor.z));
        }

        public static int3 DivUp(int3 vector, int divisor)
        {
            return new(DivUp(vector.x, divisor), DivUp(vector.y, divisor), DivUp(vector.z, divisor));
        }

        public static int3 DivUp(int3 vector, int3 divisor)
        {
            return new(DivUp(vector.x, divisor.x), DivUp(vector.y, divisor.y), DivUp(vector.z, divisor.z));
        }

        #endregion

        #region MODULO

        public static int Mod(int x, int period)
        {
            return ((x % period) + period) % period;
        }

        public static Vector3Int Mod(Vector3Int vector, int period)
        {
            return new(Mod(vector.x, period), Mod(vector.y, period), Mod(vector.z, period));
        }

        public static Vector3Int Mod(Vector3Int vector, Vector3Int period)
        {
            return new(Mod(vector.x, period.x), Mod(vector.y, period.y), Mod(vector.z, period.z));
        }

        public static int3 Mod(int3 vector, int period)
        {
            return new(Mod(vector.x, period), Mod(vector.y, period), Mod(vector.z, period));
        }

        public static int3 Mod(int3 vector, int3 period)
        {
            return new(Mod(vector.x, period.x), Mod(vector.y, period.y), Mod(vector.z, period.z));
        }

        #endregion

        #region ROUNDING

        public static int RoundToPeriod(int x, int period)
        {
            x += period / 2;
            return x - (((x % period) + period) % period);
        }

        public static Vector3Int RoundToPeriod(Vector3Int vector, int period)
        {
            return new(RoundToPeriod(vector.x, period), RoundToPeriod(vector.y, period), RoundToPeriod(vector.z, period));
        }

        public static Vector3Int RoundToPeriod(Vector3Int vector, Vector3Int period)
        {
            return new(RoundToPeriod(vector.x, period.x), RoundToPeriod(vector.y, period.y), RoundToPeriod(vector.z, period.z));
        }

        public static int3 RoundToPeriod(int3 vector, int period)
        {
            return new(RoundToPeriod(vector.x, period), RoundToPeriod(vector.y, period), RoundToPeriod(vector.z, period));
        }

        public static int3 RoundToPeriod(int3 vector, int3 period)
        {
            return new(RoundToPeriod(vector.x, period.x), RoundToPeriod(vector.y, period.y), RoundToPeriod(vector.z, period.z));
        }

        #endregion
    }
}