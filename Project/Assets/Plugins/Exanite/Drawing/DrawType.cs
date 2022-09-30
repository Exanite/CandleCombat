using System;

namespace Exanite.Drawing
{
    [Serializable]
    [Flags]
    public enum DrawType
    {
        Wire = 1 << 0,
        Solid = 1 << 1,
        WireAndSolid = Wire | Solid,
    }
}