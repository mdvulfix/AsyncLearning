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

        private bool m_isDebug = true;

        private AsyncControllerConfig m_Config;

        [SerializeField] private static Transform m_AsyncHolder;


        [SerializeField] private IFactory m_AwaiterFactory;
        private Func<IAwaiter> GetAwaiterFunc;

        private static List<IAwaiter> m_AwaiterIsReady;
        private int m_AwaiterIsReadyLimit = 3;

        private static Stack<IAsyncInfo> m_FuncExecuteQueue;

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

            m_AsyncHolder = m_Config.AsyncHolder;


            if (m_AsyncHolder == null)
                m_AsyncHolder = new GameObject("Async").transform;

            if (m_AwaiterIsReady == null)
                m_AwaiterIsReady = new List<IAwaiter>(m_AwaiterIsReadyLimit);

            if (m_FuncExecuteQueue == null)
                m_FuncExecuteQueue = new Stack<IAsyncInfo>(100);

            // SET AWAITER //
            if (m_AwaiterFactory == null)
                m_AwaiterFactory = new AwaiterFactory();

            GetAwaiterFunc = () => m_AwaiterFactory.Get<AwaiterDefault>();

            // SET POOL //
            var poolControllerConfig = new PoolControllerConfig(m_AsyncHolder);
            m_PoolController = PoolController.Get();
            m_PoolController.Init(poolControllerConfig);



            AwaiterLimitUpdate();

            OnInitComplete(new Result(this, true, $"{Label} initialized."), m_isDebug);

        }

        public override void Dispose()
        {
            m_PoolController.Dispose();

            OnDisposeComplete(new Result(this, true, $"{Label} disposed."), m_isDebug);

        }

        public virtual void Update()
        {
            FuncQueueExecute();
        }




        public void Execute(IEnumerator func)
        {
            if (GetAwaiter(out var awaiter))
                awaiter.Execute(func);


            //return FuncQueueSetAwaiter(awaiter, func);
            Debug.LogWarning($"{this}: awaiter is not found!");
            m_FuncExecuteQueue.Push(new FuncAsyncInfo(func));
        }


        /*

        public IEnumerator Run(IEnumerator func)
        {
            if (!GetAwaiter(out var awaiter))
                throw new Exception("Awaiter was not found!");

            yield return awaiter.Run(func);
            //return FuncQueueSetAwaiter(awaiter, func);
        }




        public IYield Awaite(Action func)
        {


            if (!GetAwaiter(out var awaiter))
                throw new Exception("Awaiter was not found!");

            yield return awaiter.Run(func);

            //return FuncQueueSetAwaiter(awaiter, func);
        }

        */
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

            awaiter.Initialized -= OnAwaiterInitialized;
            awaiter.Disposed -= OnAwaiterDisposed;

            m_PoolController.Push(awaiter);

        }

        private bool PopAwaiter(out IAwaiter awaiter)
        {
            if (!m_PoolController.Pop(out awaiter))
            {
                awaiter = GetAwaiterFunc();
                awaiter.Init(new AwaiterConfig("Awaiter " + awaiter.GetHashCode(), m_AsyncHolder));
            }

            awaiter.Initialized += OnAwaiterInitialized;
            awaiter.Disposed += OnAwaiterDisposed;
            awaiter.Activate();

            return true;

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
                    info.Awaiter.Execute(info.Func);
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

        private void OnAwaiterFuncComplite(IResult result)
        {
            if (!result.State)
                return;

            var awaiter = result.Context.Convert<IAwaiter>();
            m_AwaiterIsReady.Add(awaiter);

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

        void Execute(IEnumerator func);
        //IEnumerator Run(IEnumerator func);
        //IYield Awaite(Action action);



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

        public IEnumerator Func { get; private set; }
        public IAwaiter Awaiter { get; set; }



        public FuncAsyncInfo(IEnumerator func, IAwaiter awaiter)
        {
            Func = func;
            Awaiter = awaiter;
        }

        public FuncAsyncInfo(IEnumerator func)
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
        IEnumerator Func { get; }

    }

}