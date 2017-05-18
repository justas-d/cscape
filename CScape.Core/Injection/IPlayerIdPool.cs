namespace CScape.Core.Injection
{
    public interface IIdPool
    {
        uint NextEntity();
        void FreeEntity(uint id);

        short NextPlayer();
        void FreePlayer(short id);

        short NextNpc();
        void FreeNpc(short id);
    }
}
