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
            Cancel();
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

            Cancel();
            obj.SetActive(false);


            var log = $"{this}: {Label} deactivated.";
            var result = new Result(this, false, log, m_isDebug);
            Activated?.Invoke(result);

            ReadyChanged?.Invoke(result);
        }



        public void Run(Func<IResult> func)
        {
            //StopCoroutine(nameof(AsyncExecute));

            try
            {
                var isReady = false;
                var log = $"{this}: async operation started...";
                var result = new Result(this, isReady, log);

                StartCoroutine(AsyncExecute(func));
                Complite();

            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
                Cancel();
            }
        }

        public void Complite()
        {

            StopCoroutine(nameof(AsyncExecute));

            var isReady = true;
            var log = $"{this}: async operation finished...";
            var result = new Result(this, isReady, log);

            FuncExecuted.Invoke();

            Debug.Log(log);
            ReadyChanged?.Invoke(result);

        }

        public void Cancel()
        {
            StopCoroutine(nameof(AsyncExecute));

            var isReady = false;
            var log = $"{this}: async operation cancelled...";
            var result = new Result(this, isReady, log);

            Debug.Log(log);
            ReadyChanged?.Invoke(result);
        }



        protected virtual void OnReadyChanged(IResult result)
        {
            m_isReady = result.State;

            if (result.LogSend)
                Debug.Log($"{result.Context}: ready state changed to {m_isReady}.");


        }


        private IEnumerator<IResult> AsyncExecute(Func<IResult> func)
        {
            yield return func.Invoke();
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
        void Run(Func<IResult> func);
        void Complite();
        void Cancel();
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