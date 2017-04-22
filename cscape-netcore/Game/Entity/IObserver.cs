namespace CScape.Game.Entity
{
    public interface IObserver
    {
        Observatory Observatory { get; }
        bool CanSee(AbstractEntity obs);
    }
}
