namespace CScape.Basic.Model
{
    public abstract class PlayerModelLeaf : IDbPlayerLeaf
    {
        int IDbPlayerLeaf.Id { get; set; }
        string IDbPlayerLeaf.PlayerId { get; set; }
        PlayerModel IDbPlayerLeaf.Player { get; set; }

        protected PlayerModelLeaf()
        {
            
        }
    }
}