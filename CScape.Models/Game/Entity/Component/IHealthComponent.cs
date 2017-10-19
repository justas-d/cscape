namespace CScape.Models.Game.Entity.Component
{
    public interface IHealthComponent
    {
        int Health { get; set; }
        int MaxHealth { get; set; }
    }
}