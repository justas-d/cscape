using System;
using CScape.Core.Data;
using CScape.Core.Game.Entities.FacingData;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class FacingCoordinateUpdateFlag : IUpdateFlag
    {
        [NotNull]
        public IFacingData Data { get; }

        public FacingCoordinateUpdateFlag([NotNull] IFacingData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public FlagType Type => FlagType.FacingDir;

        public void Write(OutBlob stream)
        {
            stream.Write16(Data.SyncX);
            stream.Write16(Data.SyncY);
        }
    }
}