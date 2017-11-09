using System;

namespace CScape.Models.Game.Entity
{
    [Flags]
    public enum SystemMessageFlags : ulong
    {
        None = 0,
        Normal = 0,
        Debug = 1,
    }
}