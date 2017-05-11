namespace CScape.Core.Injection
{
    public interface IIdPool<T>
    {
        T NextId();
        void FreeId(T id);
    }
}