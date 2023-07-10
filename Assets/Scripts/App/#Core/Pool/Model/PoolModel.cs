using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Pool
{
    public abstract class PoolModel : ModelConfigurable
    {


        private PoolConfig m_Config;

        private bool m_isDebug = true;

        private Stack<IPoolable> m_Poolables = new Stack<IPoolable>(100);


        public int Count => m_Poolables.Count;

        public event Action<IResult> Initialized;


        // SUBSCRIBE //
        public override void Subscribe()
        {

            Initialized += OnInitialized;


        }

        public override void Unsubscribe()
        {


            Initialized -= OnInitialized;

        }



        // CONFIGURE //
        public override void Init(params object[] args)
        {
            var config = (int)Params.Config;

            if (args.Length > 0)
                try { m_Config = (PoolConfig)args[config]; }
                catch { Debug.LogWarning($"{this}: config was not found. Configuration failed!"); return; }



            Subscribe();

            var log = $"{this}: initialized.";
            var result = new Result(this, true, log, m_isDebug);

            Initialized?.Invoke(result);

        }


        public override void Dispose()
        {

            var log = $"{this}: disposed.";
            var result = new Result(this, false, log, m_isDebug);

            Initialized?.Invoke(result);

            Unsubscribe();
        }




        public bool Push(IPoolable poolable)
        {
            m_Poolables.Push(poolable);
            return true;
        }

        public bool Pop(out IPoolable poolable)
        {
            poolable = null;

            if (m_Poolables.Count > 0)
            {
                poolable = m_Poolables.Pop();
                return true;
            }

            return false;
        }

        public bool Peek(out IPoolable poolable)
        {
            poolable = null;

            if (m_Poolables.Count > 0)
            {
                poolable = m_Poolables.Peek();
                return true;
            }

            return false;
        }



        public IEnumerator GetEnumerator()
        => m_Poolables.GetEnumerator();



        // FACTORY //
        public static TPool Get<TPool>(params object[] args)
        where TPool : IPool, new()
        {
            var pool = new TPool();
            pool.Init(args);

            return pool;
        }
    }

    public interface IPool : IEnumerable, IConfigurable
    {
        event Action<IResult> Initialized;

        int Count { get; }

        bool Push(IPoolable poolable);
        bool Pop(out IPoolable poolable);
        bool Peek(out IPoolable poolable);

    }

    public delegate IPoolable GetPoolableDelegate();

    public struct PoolConfig : IConfig
    {
        public PoolConfig(int limit, GetPoolableDelegate getPoolable)
        {
            Limit = limit;

            GetPoolable = getPoolable;
        }

        public int Limit { get; private set; }
        public GetPoolableDelegate GetPoolable { get; private set; }
    }

}