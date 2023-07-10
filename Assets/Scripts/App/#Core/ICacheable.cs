namespace Core
{
    public interface ICacheable : ISubscriber
    {



        void Record();
        void Clear();
    }
}

