using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Core.Factory;

namespace Core.Async
{
    public class AwaiterModel : ModelCacheable
    {

        private AwaiterConfig m_Config;

        private Transform m_Parent;

        [SerializeField] private bool m_isDebug = true;
        [SerializeField] private bool m_isReady;



        private bool isComplete = false;
        private Action m_OnResolve;
        private Action asyncAction;


        private Coroutine m_Coroutine;

        public string Label => "Awaiter";

        public bool IsReady => m_isReady;

        public event Action<IResult> Cached;
        public event Action<IResult> Initialized;
        public event Action<IResult> Activated;

        public event Action<IResult> ReadyChanged;
        public event Action FuncExecuted;

        public static string PREF_FOLDER = "Prefabs";



        // SUBSCRIBE //
        public override void Subscribe()
        {
            Cached += OnCached;
            Initialized += OnInitialized;
            Activated += OnActivated;
            ReadyChanged += OnReadyChanged;

        }

        public override void Unsubscribe()
        {
            ReadyChanged -= OnReadyChanged;
            Activated -= OnActivated;
            Initialized -= OnInitialized;
            Cached -= OnCached;
        }


        // CONFIGURE //
        public override void Init(params object[] args)
        {
            var config = (int)Params.Config;

            if (args.Length > 0)
                try { m_Config = (AwaiterConfig)args[config]; }
                catch { Debug.LogWarning($"{this}: config was not found. Configuration failed!"); return; }




            transform.name = m_Config.Label;

            if (m_Parent == null)
                m_Parent = m_Config.Parent;

            SetParent(m_Parent);



            var log = $"{this}: {Label} initialized.";
            var result = new Result(this, true, log, m_isDebug);

            Subscribe();
            Record();
            Initialized?.Invoke(result);

        }

        public override void Dispose()
        {
            Reset();
            Clear();

            var log = $"{this}: {Label} disposed.";
            var result = new Result(this, false, log, m_isDebug);

            Initialized?.Invoke(result);
            Unsubscribe();

        }


        public override void Record()
        {
            var log = $"{this}: {Label} recorded.";
            var result = new Result(this, true, log, m_isDebug);
            Cached?.Invoke(result);
        }

        public override void Clear()
        {
            var log = $"{this}: {Label} cleared.";
            var result = new Result(this, false, log, m_isDebug);
            Cached?.Invoke(result);
        }



        // ACTIVATE //
        public override void Activate()
        {
            var obj = gameObject;

            obj.SetActive(true);



            var log = $"{this}: {Label} activated.";
            var result = new Result(this, true, log, m_isDebug);
            Activated?.Invoke(result);

            ReadyChanged?.Invoke(result);

        }

        public override void Deactivate()
        {
            var obj = gameObject;

            Reset();
            obj.SetActive(false);


            var log = $"{this}: {Label} deactivated.";
            var result = new Result(this, false, log, m_isDebug);
            Activated?.Invoke(result);

            ReadyChanged?.Invoke(result);
        }



        public IEnumerator Run(IEnumerator func)
        {

            if (m_Coroutine != null)
                StopCoroutine(m_Coroutine);


            m_Coroutine = StartCoroutine(func);
            yield return m_Coroutine;

            Resolve();
        }




        public IYield Awaite(Action func)
        {
            var awaiteFunc = new YieldWaitForAction(func);
            Run(awaiteFunc);
            //func.Invoke();
            return awaiteFunc;


            //StopCoroutine(nameof(AsyncExecute));

            //var isReady = false;
            //var log = $"{this}: async operation started...";
            //var result = new Result(this, isReady, log);

            //Complite();

        }





        public void Resolve()
        {
            if (m_Coroutine != null)
                StopCoroutine(m_Coroutine);


            m_isReady = true;
            m_OnResolve?.Invoke();

        }

        public void Reset()
        {
            if (m_Coroutine != null)
                StopCoroutine(m_Coroutine);


            m_isReady = true;
        }



        protected virtual void OnReadyChanged(IResult result)
        {
            m_isReady = result.State;

            if (result.LogSend)
                Debug.Log($"{result.Context}: ready state changed to {m_isReady}.");


        }








        // FACTORY //
        public static TAwaiter Get<TAwaiter>(params object[] args)
        where TAwaiter : IAwaiter
        {
            IFactory factoryCustom = null;

            if (args.Length > 0)
                try { factoryCustom = (IFactory)args[(int)Params.Factory]; }
                catch { Debug.Log($"Custom factory not found! The instance will be created by default."); }


            var factory = (factoryCustom != null) ? factoryCustom : new AwaiterFactory();
            var instance = factory.Get<TAwaiter>(args);

            return instance;
        }



    }

    public struct AwaiterConfig : IConfig
    {
        public AwaiterConfig(string label, Transform parent)
        {
            Label = label;
            Parent = parent;
        }

        public string Label { get; private set; }
        public Transform Parent { get; private set; }

    }

    public interface IAwaiter : IPoolable, IActivable, ICacheable, IComponent
    {
        bool IsReady { get; }

        event Action<IResult> Cached;
        event Action<IResult> Initialized;
        event Action<IResult> Activated;

        event Action FuncExecuted;
        event Action<IResult> ReadyChanged;

        //void FuncRun(Func<Action<bool>, IEnumerator> func);
        IEnumerator Run(IEnumerator func);
        IYield Awaite(Action func);
        void Resolve();
        void Reset();
    }





    public partial class AwaiterFactory : Factory<IAwaiter>, IFactory
    {

        public AwaiterFactory()
        {
            Set<AwaiterDefault>(Constructor.Get((args) => GetAwaiterDefault(args)));

        }



    }


}

/*
public void FuncRun(Func<Action<bool>, IEnumerator> func)
{
    Func = () => func(FuncComplite);
    StopCoroutine(Func());

    try
    {
        var isReady = false;
        var log = $"{this}: async operation started...";
        var result = new Result(this, isReady, log);

        StartCoroutine(Func());

        Debug.Log(log);
        ReadyChanged?.Invoke(result);


    }
    catch (Exception exception)
    {
        Debug.Log(exception.Message);
        Cancel();
    }
}
*/