namespace CScape.Basic.Model
{
    public interface IDbPlayerLeaf
    {
        int Id { get; set; }
        string PlayerId { get; set; }
        PlayerModel Player { get; set; }
    }
}