using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;
using Core.Pool;
using Core.Factory;
//using Test;

namespace Core.Async
{
    public class AsyncController : ModelConfigurable, IAsyncController
    {

        private bool m_isDebug = true;

        private AsyncControllerConfig m_Config;

        [SerializeField] private static Transform m_ObjAsync;
        [SerializeField] private static Transform m_ObjAsyncPool;

        [SerializeField] private IFactory m_AwaiterFactory;

        private static List<IAwaiter> m_AwaiterIsReady;
        private int m_AwaiterIsReadyLimit = 1;

        private static Stack<IAsyncInfo> m_FuncExecuteQueue;
        private static Stack<IYield> m_FuncAwaiteQueue;
        private IAwaiter m_AwaiterYield;

        private IPoolController m_PoolController;

        public string Label => "AsyncController";

        public event Action<IAsyncInfo> FuncExecuted;

        public AsyncController() { }
        public AsyncController(params object[] args)
            => Init(args);


        public override void Init(params object[] args)
        {
            var config = (int)Params.Config;

            if (args.Length > 0)
                try { m_Config = (AsyncControllerConfig)args[config]; }
                catch { Debug.LogWarning($"{this}: config was not found. Configuration failed!"); return; }

            m_ObjAsync = m_Config.AsyncHolder;


            if (m_ObjAsync == null)
                m_ObjAsync = new GameObject("Async").transform;


            if (m_ObjAsyncPool == null)
                m_ObjAsyncPool = new GameObject("Pool").transform;

            m_ObjAsyncPool.SetParent(m_ObjAsync);

            if (m_AwaiterIsReady == null)
                m_AwaiterIsReady = new List<IAwaiter>(m_AwaiterIsReadyLimit);

            if (m_FuncExecuteQueue == null)
                m_FuncExecuteQueue = new Stack<IAsyncInfo>(100);

            if (m_FuncAwaiteQueue == null)
                m_FuncAwaiteQueue = new Stack<IYield>(100);

            // SET AWAITER //
            if (m_AwaiterFactory == null)
                m_AwaiterFactory = new AwaiterFactory();

            m_AwaiterYield = m_AwaiterFactory.Get<AwaiterDefault>();
            m_AwaiterYield.FuncExecuted += OnAwaiterYieldFuncExecutedRunNext;
            m_AwaiterYield.Init(new AwaiterConfig("AwaiterYield", m_ObjAsync));
            m_AwaiterYield.Activate();



            // SET POOL //
            m_PoolController = new PoolController(new PoolControllerConfig(m_ObjAsyncPool));


            AwaiterLimitUpdate();
            OnInitComplete(new Result(this, true, $"{Label} initialized."), m_isDebug);
        }

        public override void Dispose()
        {
            m_AwaiterYield.Deactivate();
            m_AwaiterYield.Dispose();
            m_AwaiterYield.FuncExecuted -= OnAwaiterYieldFuncExecutedRunNext;

            m_PoolController.Dispose();
            OnDisposeComplete(new Result(this, true, $"{Label} disposed."), m_isDebug);

        }

        public virtual void Update()
        {
            FuncQueueExecute();
        }


        public void Awaite(Func<bool> func)
            => m_FuncAwaiteQueue.Push(new WaitForFunc(func));

        public void Awaite(Action func)
            => m_FuncAwaiteQueue.Push(new WaitForFunc(func));

        public void Awaite(float delay)
            => m_FuncAwaiteQueue.Push(new WaitForTime(delay));




        public void Run()
        {
            if (m_FuncAwaiteQueue?.Count == 0)
                return;

            if (m_AwaiterYield.IsReady == true)
            {
                if (m_FuncAwaiteQueue.Count > 0)
                    m_AwaiterYield.Run(m_FuncAwaiteQueue.Pop());
            }
            else
            {
                Debug.LogWarning($"{this}: awaiter is not found!");
            }

        }

        public IEnumerator ExecuteAsync(IYield func)
        {
            if (GetAwaiter(out var awaiter))
            {
                yield return awaiter.ExecuteAsync(func, (result) => Debug.Log(result.Log));
            }
            else
            {
                Debug.LogWarning($"{this}: awaiter is not found!");
                m_FuncExecuteQueue.Push(new FuncAsyncInfo(func));
            }
        }




        private bool GetAwaiter(out IAwaiter awaiter)
        {
            awaiter = null;

            if ((m_AwaiterIsReady.Count < m_AwaiterIsReadyLimit))
                AwaiterLimitUpdate();

            if ((m_AwaiterIsReady.Count > 0))
            {
                awaiter = m_AwaiterIsReady.First();
                return true;
            }

            return false;
        }

        private void AwaiterLimitUpdate()
        {
            IAwaiter awaiter;

            // Check awaiters in ready state limit;
            var awaiterIsReadyNumber = m_AwaiterIsReady.Count;

            // If the limit is less than the current number of awaiters, push unnecessary awaiters in the pool
            if (awaiterIsReadyNumber > m_AwaiterIsReadyLimit)
            {
                var number = awaiterIsReadyNumber - m_AwaiterIsReadyLimit;
                for (int i = 0; i < number; i++)
                {
                    awaiter = m_AwaiterIsReady.First();
                    awaiter.Deactivate();
                    awaiter.Dispose();
                    awaiter.FuncInvoked -= OnAwaiterFuncInvoked;
                    awaiter.FuncExecuted -= OnAwaiterFuncExecuted;
                    awaiter.Initialized -= OnAwaiterInitialized;
                    awaiter.Disposed -= OnAwaiterDisposed;

                    m_PoolController.Push(awaiter);
                }
            }
            // else, pop awaiters from the pool in the number of missing up to the limit
            else
            {
                var number = m_AwaiterIsReadyLimit - awaiterIsReadyNumber;
                for (int i = 0; i < number; i++)
                {
                    if (!m_PoolController.Pop(out awaiter))
                        awaiter = m_AwaiterFactory.Get<AwaiterDefault>();

                    awaiter.Initialized += OnAwaiterInitialized;
                    awaiter.Disposed += OnAwaiterDisposed;
                    awaiter.FuncInvoked += OnAwaiterFuncInvoked;
                    awaiter.FuncExecuted += OnAwaiterFuncExecuted;
                    awaiter.Init(new AwaiterConfig("Awaiter", m_ObjAsync));
                    awaiter.Activate();
                }
            }
        }

        private void FuncQueueExecute()
        {
            if (m_FuncExecuteQueue?.Count == 0)
                return;

            var awaiterIsReadyArr = (from IAwaiter awaiter in m_AwaiterIsReady
                                     where awaiter.IsReady == true
                                     select awaiter).ToArray();

            if (awaiterIsReadyArr.Length > 0)
            {
                if (m_FuncExecuteQueue.Count > 0)
                {
                    var info = m_FuncExecuteQueue.Pop();
                    info.Awaiter = awaiterIsReadyArr.First();
                    info.Awaiter.ExecuteAsync(info.Func, (result) => Debug.Log(result.Log));
                }
            }
        }


        private void OnAwaiterInitialized(IResult result)
        {
            if (!result.State)
                return;

            var awaiter = result.Context.Convert<IAwaiter>();
            m_AwaiterIsReady.Add(awaiter);

        }

        private void OnAwaiterDisposed(IResult result)
        {
            if (!result.State)
                return;

            var awaiter = result.Context.Convert<IAwaiter>();

            if (m_AwaiterIsReady.Contains(awaiter))
                m_AwaiterIsReady.Remove(awaiter);

        }


        private void OnAwaiterFuncInvoked(IAwaiter awaiter)
        {
            if (m_AwaiterIsReady.Contains(awaiter))
                m_AwaiterIsReady.Remove(awaiter);

            AwaiterLimitUpdate();

        }

        private void OnAwaiterFuncExecuted(IAwaiter awaiter)
        {
            m_AwaiterIsReady.Add(awaiter);

            AwaiterLimitUpdate();
        }


        private void OnAwaiterYieldFuncExecutedRunNext(IAwaiter awaiter)
        {
            if (m_FuncAwaiteQueue?.Count == 0)
                return;

            if (awaiter.IsReady == true)
            {
                if (m_FuncAwaiteQueue.Count > 0)
                    awaiter.Run(m_FuncAwaiteQueue.Pop());
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

        event Action<IAsyncInfo> FuncExecuted;

        void Awaite(Func<bool> func);
        void Awaite(Action func);
        void Awaite(float delay);

        void Run();

        IEnumerator ExecuteAsync(IYield func);




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

        public IYield Func { get; private set; }
        public IAwaiter Awaiter { get; set; }



        public FuncAsyncInfo(IYield func, IAwaiter awaiter)
        {
            Func = func;
            Awaiter = awaiter;
        }

        public FuncAsyncInfo(IYield func)
        {
            Func = func;
            Awaiter = null;
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
        IAwaiter Awaiter { get; set; }
        IYield Func { get; }

    }

}