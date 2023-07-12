using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Core.Factory;

namespace Core.Async
{
    public class AwaiterModel : ModelActivable
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

        public event Action FuncExecuted;

        public static string PREF_FOLDER = "Prefabs";





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


        // LOAD //
        public override void Load()
            => OnLoadComplete(new Result(this, true, $"{Label} loaded."), m_isDebug);

        public override void Unload()
            => OnUnloadComplete(new Result(this, true, $"{Label} unloaded."), m_isDebug);

        // CACHE //
        public override void Record()
            => OnRecordComplete(new Result(this, true, $"{Label} recorded to cache."), m_isDebug);

        public override void Clear()
            => OnClearComplete(new Result(this, true, $"{Label} cleared from cache."), m_isDebug);


        // CONFIGURE //
        public override void Init(params object[] args)
        {
            var config = (int)Params.Config;

            if (args.Length > 0)
                try { m_Config = (AwaiterConfig)args[config]; }
                catch { Debug.LogWarning($"{this}: {Label} config was not found. Configuration failed!"); return; }


            transform.name = m_Config.Label;

            if (m_Parent == null)
                m_Parent = m_Config.Parent;

            SetParent(m_Parent);

            OnInitComplete(new Result(this, true, $"{Label} initialized."), m_isDebug);
        }

        public override void Dispose()
        {
            Reset();
            OnDisposeComplete(new Result(this, true, $"{Label} disposed."), m_isDebug);

        }


        // ACTIVATE //
        public override void Activate()
        {
            Resolve();

            Obj.SetActive(true);
            OnActivateComplete(new Result(this, true, $"{Label} activated."), m_isDebug);

        }

        public override void Deactivate()
        {
            Reset();

            Obj.SetActive(false);
            OnDeactivateComplete(new Result(this, true, $"{Label} deactivated."), m_isDebug);
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

    public interface IAwaiter : IConfigurable, ICacheable, IActivable, IComponent, IPoolable
    {
        bool IsReady { get; }

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