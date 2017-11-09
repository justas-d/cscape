using System;

namespace CScape.Core.Network.Entity.Utility
{
    public static class FlagHelpers
    {
        public static NpcFlag ToNpc(this FlagType flag)
        {
            switch (flag)
            {
                case FlagType.Damage:
                    return NpcFlag.PrimaryHit;
                        
                case FlagType.FacingDir:
                    return NpcFlag.FacingCoordinate;
                        
                case FlagType.InteractingEntity:
                    return NpcFlag.InteractingEntity;
                    
                case FlagType.DefinitionChange:
                    return NpcFlag.Definition;
                
                case FlagType.ParticleEffect:
                    return NpcFlag.ParticleEffect;
                    
                case FlagType.Animation:
                    return NpcFlag.Animation;
                    
                case FlagType.OverheadText:
                    return NpcFlag.Text;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
            }

        }

        public static PlayerFlag ToPlayer(this FlagType flag)
        {
            switch (flag)
            {
                case FlagType.Damage:
                    return PlayerFlag.PrimaryHit;
                    
                case FlagType.FacingDir: return PlayerFlag.FacingCoordinate;
                    
                case FlagType.InteractingEntity:
                    return PlayerFlag.InteractEnt;
                    
                case FlagType.Appearance:
                    return PlayerFlag.Appearance;
                    
                case FlagType.ChatMessage:
                    return PlayerFlag.Chat;
                    
                case FlagType.ForcedMovement:
                    return PlayerFlag.ForcedMovement;
                    
                case FlagType.ParticleEffect:
                    return PlayerFlag.ParticleEffect;
                    
                case FlagType.Animation:
                    return PlayerFlag.Animation;

                case FlagType.OverheadText:
                    return PlayerFlag.ForcedText;

                default:
                    throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
            }

        }

    }
}