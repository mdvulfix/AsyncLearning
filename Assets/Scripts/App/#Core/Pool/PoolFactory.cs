
using Core.Factory;

namespace Core.Pool
{
    public partial class PoolFactory : Factory<IPool>, IFactory
    {
        public PoolFactory()
        {
            Set<PoolDefault>(Constructor.Get((args) => GetPoolDefault(args)));
        }

    }

    public partial class PoolFactory<TPoolable> : Factory<IPool>, IFactory
    {
        public PoolFactory()
        {
            Set<Pool<TPoolable>>(Constructor.Get((args) => GetPool(args)));

        }

    }

}