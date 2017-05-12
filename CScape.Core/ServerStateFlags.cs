using System;

namespace CScape.Core
{
    [Flags]
    public enum ServerStateFlags
    {
        None,
        PlayersFull,
        LoginDisabled
    }
}