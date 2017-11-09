using System;

namespace CScape.Core.Network.Entity
{
    [Flags]
    public enum NpcFlag
    {
        Animation = 0x10,
        PrimaryHit = 8,
        ParticleEffect = 0x80,
        InteractingEntity = 0x20,
        Text = 1,
        SecondaryHit = 0x40,
        Definition = 2,
        FacingCoordinate = 4
    }
}