using System;

namespace CScape.Models
{
    [Flags]
    public enum ServerStateFlags
    {
        None,
        PlayersFull,
        LoginDisabled
    }
}