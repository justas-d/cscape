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
    public abstract class UpdateWriter : IUpdateWriter, IEnumerable<IUpdateFlag>
    {
        [NotNull]
        private readonly FlagAccumulatorComponent _accum;

        private readonly Dictionary<FlagType, IUpdateFlag> _overrides
            = new Dictionary<FlagType, IUpdateFlag>();

        public UpdateWriter([NotNull] FlagAccumulatorComponent accum)
        {
            _accum = accum;
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

        public abstract void Write(OutBlob stream);
        
        protected virtual IUpdateFlag GetFlag(FlagType type)
        {
            return _overrides.ContainsKey(type)
                ? _overrides[type]
                : _accum.Flags[type];
        }

        protected virtual int GetHeader(Func<FlagType, int> converter)
        {
            var retval = 0;

            foreach (var flag in this)
            {
                retval |= (int) converter(flag.Type);
            }

            return retval;
        }

        public virtual bool NeedsUpdate()
        {
            return GetHeader() != 0;
        }

        public void SetFlag(IUpdateFlag flag)
        {
            if (_overrides.ContainsKey(flag.Type))
                _overrides[flag.Type] = flag;
            else
                _overrides.Add(flag.Type, flag);
        }

    }
}