using CScape.Models.Data;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class PlayerAppearanceUpdateFlag : IUpdateFlag
    {
        public Blob AppearanceData { get; }

        public FlagType Type => FlagType.Appearance;

        public PlayerAppearanceUpdateFlag(Blob appearanceData)
        {
            AppearanceData = appearanceData;
        }

        public void Write(OutBlob stream)
        {
            AppearanceData.FlushInto(stream);
        }
    }
}