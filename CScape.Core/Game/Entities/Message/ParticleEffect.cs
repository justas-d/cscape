namespace CScape.Core.Game.Entities.Message
{
    public class ParticleEffect
    {
        public short Id { get; }
        public short Height { get; }
        public short Delay { get; }

        public static ParticleEffect Stop { get; } = new ParticleEffect(-1, 0, 0);

        public static ParticleEffect LevelUp { get; } = new ParticleEffect(199, 0, 0);

        public ParticleEffect(short id, short height, short delay)
        {
            Id = id;
            Height = height;
            Delay = delay;
        }
    }
}