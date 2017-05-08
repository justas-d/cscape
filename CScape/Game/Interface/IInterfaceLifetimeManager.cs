namespace CScape.Game.Interface
{
    public interface IInterfaceLifetimeManager
    {
        /// <summary>
        /// Instructs the manager that the given interface are closing and that they should clean up.
        /// </summary>
        void Close(IManagedInterface interf);
    }
}