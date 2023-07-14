using System;
using UnityEngine;
using Core;

namespace Test
{
    [Serializable]
    public abstract class DebugModel : ModelLoadable
    {
        private bool m_isDebug = false;

        private DebugConfig m_Config;

        public string Label => "Debug";

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
                try { m_Config = (DebugConfig)args[config]; }
                catch { Debug.LogWarning($"{this}: {Label} config was not found. Configuration failed!"); return; }


            OnInitComplete(new Result(this, true, $"{Label} initialized."), m_isDebug);

        }

        public override void Dispose()
        {
            OnDisposeComplete(new Result(this, true, $"{Label} disposed."), m_isDebug);
        }


        // ACTIVATE //
        public override void Activate()
        {
            Obj.SetActive(true);
            OnActivateComplete(new Result(this, true, $"{Label} activated."), m_isDebug);

        }

        public override void Deactivate()
        {

            Obj.SetActive(false);
            OnDeactivateComplete(new Result(this, true, $"{Label} deactivated."), m_isDebug);
        }



    }

    public class DebugConfig
    {
    }
}