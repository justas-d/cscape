using System;

namespace CScape.Core.Network.Entity
{
    [Flags]
    public enum PlayerFlag
    {
        ForcedMovement = 0x400,
        ParticleEffect = 0x100,
        Animation = 8,
        ForcedText = 4,
        Chat = 0x80,
        InteractEnt = 0x1,
        Appearance = 0x10,
        FacingCoordinate = 0x2,
        PrimaryHit = 0x20,
        SecondaryHit = 0x200,
    }
}