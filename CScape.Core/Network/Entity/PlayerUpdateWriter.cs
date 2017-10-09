using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Data;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Network.Entity.Flag;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity
{
    public class PlayerUpdateWriter : IUpdateWriter, IEnumerable<IUpdateFlag>
    {
        [NotNull]
        private readonly FlagAccumulatorComponent _accum;

        private readonly Dictionary<FlagType, IUpdateFlag> _overrides
            = new Dictionary<FlagType, IUpdateFlag>();


        public PlayerUpdateWriter([NotNull] FlagAccumulatorComponent flags)
        {
            _accum = flags ?? throw new ArgumentNullException(nameof(flags));
        }


        public IEnumerator<IUpdateFlag> GetEnumerator()
        {
            foreach (var flag in _accum.Flags.Values) yield return flag;
            foreach (var flag in _overrides.Values) yield return flag;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual IUpdateFlag GetFlag(FlagType type)
        {
            return _overrides.ContainsKey(type) 
                ? _overrides[type] 
                : _accum.Flags[type];
        }

        protected virtual PlayerFlag GetHeader()
        {
            PlayerFlag retval = 0;

            foreach(var flag in this)
            {
                retval |= flag.Type.ToPlayer();
            }

            return retval;
        }

        public virtual bool NeedsUpdate()
        {
            return this.Any();
        }

        public void SetFlag(IUpdateFlag flag)
        {
            if (_overrides.ContainsKey(flag.Type))
                _overrides[flag.Type] = flag;
            else
                _overrides.Add(flag.Type, flag);
        }

        public virtual void Write(OutBlob stream)
        {
            var header = GetHeader();
            if (header != 0)
            {
                if ((header & PlayerFlag.ForcedMovement) != 0)
                {
                    GetFlag(FlagType.ForcedMovement).Write(stream);
                }
                if ((header & PlayerFlag.ParticleEffect) != 0)
                {
                    GetFlag(FlagType.ParticleEffect).Write(stream);
                }
                if ((header & PlayerFlag.Animation) != 0)
                {
                    GetFlag(FlagType.Animation).Write(stream);
                }
                if ((header & PlayerFlag.ForcedText) != 0)
                {
                    GetFlag(FlagType.OverheadText).Write(stream);
                }
                if ((header & PlayerFlag.Chat) != 0)
                {
                    GetFlag(FlagType.ChatMessage).Write(stream);
                }
                if ((header & PlayerFlag.InteractEnt) != 0)
                {
                    GetFlag(FlagType.InteractingEntity).Write(stream);
                }
                if ((header & PlayerFlag.Appearance) != 0)
                {
                    GetFlag(FlagType.Appearance).Write(stream);
                }
                if ((header & PlayerFlag.FacingCoordinate) != 0)
                {
                    GetFlag(FlagType.FacingDir).Write(stream);
                }
                if ((header & PlayerFlag.PrimaryHit) != 0 ||
                    (header & PlayerFlag.SecondaryHit) != 0)
                {
                    GetFlag(FlagType.Damage).Write(stream);
                }
            }
        }
    }
}