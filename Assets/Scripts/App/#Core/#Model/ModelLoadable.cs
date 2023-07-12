using System;
using UnityEngine;
using UComponent = UnityEngine.Component;

namespace Core
{
    public abstract class ModelLoadable : MonoBehaviour
    {
        [SerializeField] private bool m_isLoaded;
        [SerializeField] private bool m_isCached;
        [SerializeField] private bool m_isInitialized;


        public string Name => this.GetName();
        public Type Type => this.GetType();

        public GameObject Obj => gameObject;


        public event Action<IResult> Loaded;
        public event Action<IResult> Unloaded;

        public event Action<IResult> Recorded;
        public event Action<IResult> Cleared;

        public event Action<IResult> Initialized;
        public event Action<IResult> Disposed;


        public enum Params
        {
            Config,
            Factory
        }

        // LOAD //
        public abstract void Load();
        public abstract void Unload();

        // CACHE //
        public abstract void Record();
        public abstract void Clear();

        // CONFIGURE //
        public abstract void Init(params object[] args);
        public abstract void Dispose();


        // COMPONENT //
        public TComponent SetComponent<TComponent>()
        where TComponent : UComponent
            => gameObject.AddComponent<TComponent>();

        public bool GetComponent<TComponent>(out TComponent component)
        where TComponent : UComponent
            => gameObject.TryGetComponent<TComponent>(out component);

        public void SetParent(Transform parent)
            => gameObject.transform.SetParent(parent);


        // VERIFY //    
        protected virtual bool VerifyLoad(bool isDebug = true)
        {
            if (m_isInitialized == false)
            {
                if (isDebug) Debug.LogWarning($"{this}: instance is not initialized. Load was aborted!");
                return true;
            }


            if (m_isLoaded == true)
            {
                if (isDebug) Debug.LogWarning($"{this}: instance was already loaded.");
                return true;
            }

            return false;
        }

        protected virtual bool VerifyInit(bool isDebug = true)
        {
            if (m_isInitialized == true)
            {
                if (isDebug) Debug.LogWarning($"{this}: instance was already initialized.");
                return true;
            }

            return false;
        }


        // CALLBACK //
        protected virtual void OnLoadComplete(IResult result, bool isDebag = true)
        {
            m_isLoaded = true;

            if (isDebag)
                Debug.Log($"{this.GetName()}: {result.Log}");

            Loaded?.Invoke(result);
        }

        protected virtual void OnUnloadComplete(IResult result, bool isDebag = true)
        {
            m_isLoaded = false;

            if (isDebag)
                Debug.Log($"{this.GetName()}: {result.Log}");

            Unloaded?.Invoke(result);
        }


        protected virtual void OnRecordComplete(IResult result, bool isDebag = true)
        {
            m_isCached = true;

            if (isDebag)
                Debug.Log($"{this.GetName()}: {result.Log}");

            Recorded?.Invoke(result);
        }

        protected virtual void OnClearComplete(IResult result, bool isDebag = true)
        {
            m_isCached = false;

            if (isDebag)
                Debug.Log($"{this.GetName()}: {result.Log}");

            Cleared?.Invoke(result);
        }


        protected virtual void OnInitComplete(IResult result, bool isDebag = true)
        {
            m_isInitialized = true;

            if (isDebag)
                Debug.Log($"{this.GetName()}: {result.Log}");

            Initialized?.Invoke(result);

        }

        protected virtual void OnDisposeComplete(IResult result, bool isDebag = true)
        {
            m_isInitialized = false;

            if (isDebag)
                Debug.Log($"{this.GetName()}: {result.Log}");

            Disposed?.Invoke(result);

        }


        // UNITY //
        private void Awake()
            => Load();

        private void OnEnable()
            => Record();

        private void OnDisable()
            => Clear();

        private void OnDestroy()
            => Unload();


    }

}

