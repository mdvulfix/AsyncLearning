using System;
using System.Collections;
using UnityEngine;

using Core.Factory;

namespace Core.Pool
{
    public class Pool<TPoolable> : PoolDefault, IPool<TPoolable>
    where TPoolable : IPoolable
    {

        public Pool() { }
        public Pool(params object[] args)
            => Init(args);


        public bool Push(TPoolable poolable)
            => Push<TPoolable>(poolable);

        public bool Pop(out TPoolable poolable)
            => Pop<TPoolable>(out poolable);

        public bool Peek(out TPoolable poolable)
            => Peek<TPoolable>(out poolable);


        public static new Pool<TPoolable> Get(params object[] args)
            => Get<Pool<TPoolable>>(args);

    }


    public class PoolDefault : PoolModel, IPool, IConfigurable
    {

        public PoolDefault() { }
        public PoolDefault(params object[] args)
            => Init(args);



        public bool Push<TPoolable>(TPoolable poolable)
        where TPoolable : IPoolable
        {
            base.Push(poolable);
            return true;
        }

        public bool Pop<TPoolable>(out TPoolable poolable)
        where TPoolable : IPoolable
        {
            poolable = default(TPoolable);

            if (base.Pop(out var iPoolable))
            {
                poolable = (TPoolable)iPoolable;
                return true;
            }

            return false;
        }

        public bool Peek<TPoolable>(out TPoolable poolable)
        where TPoolable : IPoolable
        {
            poolable = default(TPoolable);

            if (base.Peek(out var iPoolable))
            {
                poolable = (TPoolable)iPoolable;
                return true;
            }

            return false;
        }


        public static PoolDefault Get(params object[] args)
            => Get<PoolDefault>(args);


    }


    public interface IPool<TPoolable> : IPool
    {
        bool Push(TPoolable poolable);
        bool Pop(out TPoolable poolable);
        bool Peek(out TPoolable poolable);

    }


    public partial class PoolFactory : Factory<IPool>, IFactory
    {
        private PoolDefault GetPoolDefault(params object[] args)
        {

            var instance = new PoolDefault();

            if (args.Length > 0)
            {
                var config = (PoolConfig)args[(int)PoolModel.Params.Config];
                instance.Init(config);
            }

            return instance;
        }

        private Pool<TPoolable> GetPool<TPoolable>(params object[] args)
        where TPoolable : IPoolable
        {
            var instance = new Pool<TPoolable>();
            if (args.Length > 0)
            {
                var config = (PoolConfig)args[(int)PoolModel.Params.Config];
                instance.Init(config);
            }

            return instance;
        }



    }


    public partial class PoolFactory<TPoolable> : Factory<IPool>, IFactory
    where TPoolable : IPoolable
    {

        private Pool<TPoolable> GetPool(params object[] args)
        {
            var instance = new Pool<TPoolable>();
            if (args.Length > 0)
            {
                var config = (PoolConfig)args[(int)PoolModel.Params.Config];
                instance.Init(config);
            }

            return instance;
        }



    }

}