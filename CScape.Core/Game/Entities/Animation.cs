namespace CScape.Core.Game.Entities
{
    public class Animation
    {
        public short Id { get; }
        public byte Delay { get; }

        public static Animation Reset { get; } = new Animation(-1, 0);

        public Animation(short id, byte delay)
        {
            Id = id;
            Delay = delay;
        }
    }
}