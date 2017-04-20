namespace cscape
{
    public interface IPlayerSaveData
    {
        int Id { get; }
        string PasswordHash { get; }
        string Username { get; }
        byte TitleIcon { get; }

        ushort X { get; }
        ushort Y { get; }
        byte Z { get; }
    }
}