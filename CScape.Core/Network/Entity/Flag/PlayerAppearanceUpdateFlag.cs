using System;
using CScape.Core.Data;
using CScape.Models.Data;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class PlayerAppearanceUpdateFlag : IUpdateFlag
    {
        public FlagType Type => FlagType.Appearance;

        public void Write(OutBlob stream)
        {
            TODO
            // TODO : WritePlayerAppearance
            throw new NotImplementedException();
        }
    }
}