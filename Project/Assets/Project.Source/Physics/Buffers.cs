using UnityEngine;

namespace Exanite.Arpg.Utilities
{
    public static class Buffers
    {
        public const int LargeCacheCapacity = 40;

        // Should be safe since Physics queries will never be multi-threaded
        // and results can be copied out if persistence is needed
        public static readonly Collider[] ColliderSingle;
        public static readonly Collider[] ColliderLarge;

        static Buffers()
        {
            ColliderSingle = new Collider[1];
            ColliderLarge = new Collider[LargeCacheCapacity];
        }
    }
}