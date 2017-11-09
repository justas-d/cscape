using System;
using CScape.Core.Extensions;
using CScape.Models.Game.Entity.FacingData;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class FacingCoordinateUpdateFlag : IUpdateFlag
    {
        [NotNull]
        public IFacingState State { get; }

        public FacingCoordinateUpdateFlag([NotNull] IFacingState state)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public FlagType Type => FlagType.FacingDir;

        public void Write(OutBlob stream)
        {
            stream.Write16((State.Coordinate.X * 2 + 1).CastClamp(short.MinValue, short.MaxValue));
            stream.Write16((State.Coordinate.Y * 2 + 1).CastClamp(short.MinValue, short.MaxValue));
        }
    }
}