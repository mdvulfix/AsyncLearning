using System;
using System.Collections;
using UnityEngine;


namespace Core.Async.Test
{
    public abstract class BollModel : ModelCacheable
    {
        private BollConfig m_Config;


        [SerializeField] private bool m_isDebug = true;

        public event Action<IResult> Cached;
        public event Action<IResult> Initialized;
        public event Action<IResult> Activated;


        // SUBSCRIBE //
        public override void Subscribe()
        {
            Cached += OnCached;
            Initialized += OnInitialized;
            Activated += OnActivated;

        }

        public override void Unsubscribe()
        {

            Activated += OnActivated;
            Initialized -= OnInitialized;
            Cached += OnCached;
        }


        public override void Record()
        {
            var log = $"{this}: recorded.";
            var result = new Result(this, true, log, m_isDebug);
            Cached?.Invoke(result);
        }

        public override void Clear()
        {
            var log = $"{this}: cleared.";
            var result = new Result(this, false, log, m_isDebug);
            Cached?.Invoke(result);
        }


        // CONFIGURE //
        public override void Init(params object[] args)
        {
            var config = (int)Params.Config;

            if (args.Length > 0)
                try { m_Config = (BollConfig)args[config]; }
                catch { Debug.LogWarning($"{this}: config was not found. Configuration failed!"); return; }



            var log = $"{this}: initialized.";
            var result = new Result(this, true, log, m_isDebug);

            Subscribe();
            Initialized?.Invoke(result);

        }

        public override void Dispose()
        {

            var log = $"{this}: disposed.";
            var result = new Result(this, false, log, m_isDebug);

            Initialized?.Invoke(result);
            Unsubscribe();

        }


        // LOAD //
        public abstract void Load();
        public abstract void Unload();


        // ACTIVATE //
        public override void Activate()
        {
            var obj = gameObject;

            obj.SetActive(true);

            var log = $"{this}: activated.";
            var result = new Result(this, true, log, m_isDebug);
            Activated?.Invoke(result);

        }

        public override void Deactivate()
        {
            var obj = gameObject;

            obj.SetActive(false);

            var log = $"{this}: deactivated.";
            var result = new Result(this, false, log, m_isDebug);
            Activated?.Invoke(result);
        }

    }

    public class BollConfig
    {
    }
}