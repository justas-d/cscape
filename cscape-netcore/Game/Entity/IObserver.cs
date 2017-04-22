namespace CScape.Game.Entity
{
    public interface IObserver
    {
        bool CanSee(AbstractEntity obs);
    }
}
