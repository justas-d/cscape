namespace CScape
{
    public interface IPlayerSaveData
    {
        int DatabaseId { get; }
        string PasswordHash { get; }
        string Username { get; }
        byte TitleIcon { get; }

        ushort X { get; }
        ushort Y { get; }
        byte Z { get; }
    }
}