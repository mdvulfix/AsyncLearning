using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;
using Core.Pool;
using Core.Factory;

namespace Core.Async
{
    public class AsyncController : ModelConfigurable, IAsyncController
    {

        private AsyncControllerConfig m_Config;

        [SerializeField] private static Transform m_AsyncHolder;


        [SerializeField] private IFactory m_AwaiterFactory;
        private Func<IAwaiter> GetAwaiterFunc;

        private static List<IAwaiter> m_AwaiterIsReady;
        private int m_AwaiterIsReadyLimit = 1;

        private static List<IAsyncInfo> m_FuncExecuteQueue;

        private IPoolController m_PoolController;

        public event Action<IResult> Initialized;
        public event Action<IAsyncInfo> FuncExecuted;

        public AsyncController() { }
        public AsyncController(params object[] args)
            => Init(args);


        // SUBSCRIBE //
        public override void Subscribe()
        {
            Initialized += OnInitialized;
            //SignalProvider.SignalCalled += OnSignalCalled;

        }

        public override void Unsubscribe()
        {

            Initialized -= OnInitialized;

            //SignalProvider.SignalCalled -= OnSignalCalled;

        }






        public override void Init(params object[] args)
        {
            var config = (int)Params.Config;

            if (args.Length > 0)
                try { m_Config = (AsyncControllerConfig)args[config]; }
                catch { Debug.LogWarning($"{this}: config was not found. Configuration failed!"); return; }


            if (m_AsyncHolder == null)
                m_AsyncHolder = new GameObject("Async").transform;

            if (m_AwaiterIsReady == null)
                m_AwaiterIsReady = new List<IAwaiter>(m_AwaiterIsReadyLimit);

            if (m_FuncExecuteQueue == null)
                m_FuncExecuteQueue = new List<IAsyncInfo>(100);

            // SET AWAITER //
            if (m_AwaiterFactory == null)
                m_AwaiterFactory = new AwaiterFactory();

            GetAwaiterFunc = () => m_AwaiterFactory.Get<AwaiterDefault>();

            // SET POOL //
            var poolControllerConfig = new PoolControllerConfig(m_AsyncHolder);
            m_PoolController = PoolController.Get();
            m_PoolController.Init(poolControllerConfig);

        }

        public override void Dispose()
        {
            m_PoolController.Dispose();

        }

        public virtual void Update()
        {
            FuncQueueRunAsync();
        }


        public void Run(Func<IResult> func)
        {
            if (GetAwaiter(out var awaiter))
                awaiter.Run(func);
            else
                m_FuncExecuteQueue.Add(new FuncAsyncInfo(awaiter, func));
        }


        private bool GetAwaiter(out IAwaiter awaiter)
        {
            awaiter = null;

            if ((m_AwaiterIsReady.Count < m_AwaiterIsReadyLimit))
                AwaiterLimitUpdate();


            awaiter = m_AwaiterIsReady.First();
            return true;
        }

        private void AwaiterLimitUpdate()
        {
            // Check awaiters in ready state limit;
            var awaiterIsReadyNumber = m_AwaiterIsReady.Count;

            // If the limit is less than the current number of awaiters, push unnecessary awaiters in the pool
            if (awaiterIsReadyNumber > m_AwaiterIsReadyLimit)
            {
                var number = awaiterIsReadyNumber - m_AwaiterIsReadyLimit;
                for (int i = 0; i < number; i++)
                {
                    PushAwaiter(m_AwaiterIsReady.First());
                }

            }
            // else, pop awaiters from the pool in the number of missing up to the limit
            else
            {

                var number = m_AwaiterIsReadyLimit - awaiterIsReadyNumber;
                for (int i = 0; i < number; i++)
                    PopAwaiter(out var awaiter);
            }
        }

        private void PushAwaiter(IAwaiter awaiter)
        {
            awaiter.Deactivate();
            awaiter.ReadyChanged -= OnAwaiterReadyChanged;

            m_PoolController.Push(awaiter);

        }

        private bool PopAwaiter(out IAwaiter awaiter)
        {
            if (!m_PoolController.Pop(out awaiter))
            {
                awaiter = GetAwaiterFunc();
                awaiter.Init(new AwaiterConfig("Awaiter " + awaiter.GetHashCode(), m_AsyncHolder));
            }

            awaiter.ReadyChanged += OnAwaiterReadyChanged;
            awaiter.Activate();

            return true;

        }

        private void FuncQueueRunAsync()
        {
            if (m_FuncExecuteQueue?.Count == 0)
                return;

            var funcsReadyToBeExecuted = (from IAsyncInfo funcInfo in m_FuncExecuteQueue
                                          where funcInfo.Awaiter.IsReady == true
                                          select funcInfo).ToArray();

            if (funcsReadyToBeExecuted.Length > 0)
            {
                foreach (var info in funcsReadyToBeExecuted)
                {
                    if (m_FuncExecuteQueue.Contains(info))
                        m_FuncExecuteQueue.Remove(info);

                    info.Awaiter.Run(info.FuncAsync);
                }
            }
        }

        private void OnAwaiterReadyChanged(IResult result)
        {
            IAwaiter awaiter = null;

            try { awaiter = (IAwaiter)result.Context; }
            catch { Debug.LogWarning($"{this}: instance type is not of awaiter!"); return; }

            if (result.State)
            {
                m_AwaiterIsReady.Add(awaiter);
            }
            else
            {
                if (m_AwaiterIsReady.Contains(awaiter))
                    m_AwaiterIsReady.Remove(awaiter);
            }

            AwaiterLimitUpdate();
        }


        public static AsyncController Get(params object[] args)
        {
            var config = (int)Params.Config;

            if (args.Length > 0)
            {
                try
                {
                    var instance = new AsyncController();
                    instance.Init((AsyncControllerConfig)args[config]);
                    return instance;
                }

                catch { Debug.Log("Custom factory not found! The instance will be created by default."); }
            }

            return new AsyncController();
        }

    }

    public interface IAsyncController : IController, IConfigurable, IUpdatable
    {
        event Action<IResult> Initialized;
        event Action<IAsyncInfo> FuncExecuted;

        void Run(Func<IResult> func);
    }

    public struct AsyncControllerConfig : IConfig
    {
        public AsyncControllerConfig(Transform asyncHolder)
        {
            AsyncHolder = asyncHolder;
        }

        public Transform AsyncHolder { get; private set; }
    }

    public struct FuncAsyncInfo : IAsyncInfo
    {
        public IAwaiter Awaiter { get; private set; }
        public Func<IResult> FuncAsync { get; private set; }

        public FuncAsyncInfo(IAwaiter awaiter, Func<IResult> func)
        {
            FuncAsync = func;
            Awaiter = awaiter;
        }




    }
    /*
    public struct FuncAsyncInfo : IAsyncInfo
    {
        public IAwaiter Awaiter { get; private set; }
        public Func<Action<bool>, IEnumerator> FuncAsync { get; private set; }

        public FuncAsyncInfo(IAwaiter awaiter, Func<Action<bool>, IEnumerator> func)
        {
            FuncAsync = func;
            Awaiter = awaiter;
        }
    }
    */
    public interface IAsyncInfo
    {
        IAwaiter Awaiter { get; }
        Func<IResult> FuncAsync { get; }

    }

}