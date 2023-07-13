using System;
using UnityEngine;
using UComponent = UnityEngine.Component;

namespace Core
{
    public abstract class ModelActivable : MonoBehaviour
    {
        [SerializeField] private bool m_isCached;
        [SerializeField] private bool m_isInitialized;
        [SerializeField] private bool m_isActivated;

        public string Name => this.GetName();
        public Type Type => this.GetType();

        public GameObject Obj => gameObject;




        public event Action<IResult> Recorded;
        public event Action<IResult> Cleared;

        public event Action<IResult> Initialized;
        public event Action<IResult> Disposed;

        public event Action<IResult> Activated;
        public event Action<IResult> Deactivated;




        public enum Params
        {
            Config,
            Factory
        }

        // CACHE //
        public abstract void Record();
        public abstract void Clear();

        // CONFIGURE //
        public abstract void Init(params object[] args);
        public abstract void Dispose();

        // ACTIVATE //
        public abstract void Activate();
        public abstract void Deactivate();


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


        protected virtual void OnActivateComplete(IResult result, bool isDebag = true)
        {
            m_isActivated = true;

            if (isDebag)
                Debug.Log($"{this.GetName()}: {result.Log}");

            Activated?.Invoke(result);

        }

        protected virtual void OnDeactivateComplete(IResult result, bool isDebag = true)
        {
            m_isActivated = false;

            if (isDebag)
                Debug.Log($"{this.GetName()}: {result.Log}");

            Deactivated?.Invoke(result);

        }

        // UNITY //

        private void OnEnable()
            => Record();

        private void OnDisable()
            => Clear();


    }

}

