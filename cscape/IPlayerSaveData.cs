namespace cscape
{
    public interface IPlayerSaveData
    {
        int Id { get; }
        string PasswordHash { get; }
        string Username { get; }
        byte TitleIcon { get; }
    }
}