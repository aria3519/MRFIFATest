using System;
using Unity.Collections;

namespace Jisu.Utils
{
    public static class XORShift
    {
        private const uint SEED_X = 123456789u;
        private const uint SEED_Y = 362436069u;
        private const uint SEED_Z = 521288629u;
        private const uint SEED_W = 88675123u;

        private const float denominator = 1 / (float)(uint.MaxValue);

        private static uint x, y, z, w;

        static XORShift()
        {
            x = SEED_X;
            y = SEED_Y;
            z = SEED_Z;
            w = (uint)Guid.NewGuid().GetHashCode();
        }

        public static uint NativeNext
        {
            get
            {
                uint t = x ^ (x << 11);

                x = y;
                y = z;
                z = w;
                w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));

                return w;
            }
        }

        /// <summary>
        /// Return float in Range [0,1]
        /// </summary>
        public static float Next => NativeNext * denominator;
    }
}