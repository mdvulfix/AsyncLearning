using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Pool
{


    public class PoolController<TPoolable> : PoolController, IPoolController<TPoolable>
    where TPoolable : IPoolable
    {

        public PoolController() { }
        public PoolController(params object[] args)
            => Init(args);


        public bool Push(TPoolable poolable)
            => Push<TPoolable>(poolable);

        public bool Pop(out TPoolable poolable)
            => Pop<TPoolable>(out poolable);

        public bool Peek(out TPoolable poolable)
            => Peek<TPoolable>(out poolable);


        // FACTORY //
        public static new PoolController<TPoolable> Get(params object[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    var config = (PoolControllerConfig)args[((int)Params.Config)];
                    var instance = new PoolController<TPoolable>();
                    instance.Init(config);
                    return instance;
                }

                catch { Debug.Log("Custom factory not found! The instance will be created by default."); }
            }

            return new PoolController<TPoolable>(); ;
        }
    }

    public class PoolController : ModelConfigurable, IPoolController
    {
        private static Transform m_PoolHolder;

        private IPool m_Pool;

        [Header("Debug")]
        [SerializeField] protected bool m_isDebug = true;

        [Header("Config")]
        [SerializeField] protected PoolControllerConfig m_Config;

        public event Action<IResult> Initialized;


        public PoolController() { }
        public PoolController(params object[] args)
            => Init(args);



        // SUBSCRIBE //
        public override void Subscribe()
        {
            Initialized += OnInitialized;

        }

        public override void Unsubscribe()
        {
            Initialized -= OnInitialized;

        }


        public override void Init(params object[] args)
        {

            var config = (int)Params.Config;


            if (args.Length > 0)
                try { m_Config = (PoolControllerConfig)args[config]; }
                catch { Debug.LogWarning($"{this}: config was not found. Configuration failed!"); return; }


            if (m_PoolHolder == null)
                m_PoolHolder = new GameObject("Pool").transform;

            var poolConfig = new PoolConfig();

            if (m_Pool == null)
            {
                m_Pool = new PoolDefault();
                m_Pool.Init(poolConfig);
            }

            Subscribe();

            var log = $"{this}: initialized.";
            var result = new Result(this, true, log, m_isDebug);
            Initialized?.Invoke(result);

        }



        public override void Dispose()
        {

            //m_AsyncController.Dispose();

            //m_Awaiter.Deactivate();
            //m_Awaiter.Dispose();

            var log = $"{this}: disposed.";
            var result = new Result(this, false, log, m_isDebug);
            Initialized?.Invoke(result);

            Unsubscribe();

        }





        public bool Push<TPoolable>(TPoolable poolable)
        where TPoolable : IPoolable
        {
            SetParent(poolable, m_PoolHolder);
            return m_Pool.Push(poolable);
        }

        public bool Pop<TPoolable>(out TPoolable poolable)
        where TPoolable : IPoolable
        {
            poolable = default(TPoolable);

            if (m_Pool.Pop(out var instance))
            {
                poolable = (TPoolable)instance;
                return true;
            }

            return false;
        }

        public bool Peek<TPoolable>(out TPoolable poolable)
        where TPoolable : IPoolable
        {
            poolable = default(TPoolable);

            if (m_Pool.Peek(out var instance))
            {
                poolable = (TPoolable)instance;
                return true;
            }

            return false;
        }




        public void SetPool(IPool pool)
            => m_Pool = pool;


        public void SetParent(IPoolable poolable, Transform parent)
        {
            if (poolable is IComponent)
                ((IComponent)poolable).SetParent(parent);
        }


        public int CountPoolables()
            => m_Pool.Count;




        public static PoolController Get(params object[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    var config = (PoolControllerConfig)args[((int)Params.Config)];
                    var instance = new PoolController();
                    instance.Init(config);
                    return instance;
                }

                catch { Debug.Log("Custom factory not found! The instance will be created by default."); }
            }

            return new PoolController();
        }
    }





    public struct PoolControllerConfig : IConfig
    {


        public PoolControllerConfig(Transform poolHolder)
        {
            PoolHolder = poolHolder;
        }

        public Transform PoolHolder { get; private set; }

    }



    public interface IPoolController<TPoolable> : IPoolController
    where TPoolable : IPoolable
    {

        bool Peek(out TPoolable poolable);
        bool Pop(out TPoolable poolable);
        bool Push(TPoolable poolable);


    }

    public interface IPoolController : IController, IConfigurable
    {
        bool Peek<TPoolable>(out TPoolable poolable)
        where TPoolable : IPoolable;

        bool Pop<TPoolable>(out TPoolable poolable)
        where TPoolable : IPoolable;

        bool Push<TPoolable>(TPoolable poolable)
        where TPoolable : IPoolable;

        void SetPool(IPool pool);

        int CountPoolables();


    }




}

