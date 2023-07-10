using System;
using UnityEngine;
using UComponent = UnityEngine.Component;

namespace Core
{
    public abstract class ModelLoadable : MonoBehaviour
    {

        [SerializeField] private bool m_isCached;
        [SerializeField] private bool m_isInitialized;
        [SerializeField] private bool m_isLoaded;
        [SerializeField] private bool m_isActivated;


        public enum Params
        {
            Config,
            Factory
        }


        public GameObject Obj => gameObject;

        public static readonly int PARAMS_CONFIG = 0;
        public static readonly int PARAMS_FACTORY = 1;



        // SUBSCRIBE //
        public abstract void Subscribe();
        public abstract void Unsubscribe();

        // CACHE //
        public abstract void Record();
        public abstract void Clear();

        // CONFIGURE //
        public abstract void Init(params object[] args);
        public abstract void Dispose();

        // LOAD //
        public abstract void Load();
        public abstract void Unload();

        // ACTIVATE //
        public abstract void Activate();
        public abstract void Deactivate();



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

        protected virtual bool VerifyActivate(bool isDebug = true)
        {

            if (m_isLoaded == false)
            {
                if (isDebug) Debug.LogWarning($"{this}: instance is not loaded. Activation was aborted!");
                return true;
            }

            return false;
        }


        // CALLBACK //
        protected virtual void OnCached(IResult result)
        {
            m_isCached = result.State;

            if (result.LogSend)
                Debug.Log($"{result.Context}: cache state changed to {m_isCached}.");

        }

        protected virtual void OnInitialized(IResult result)
        {
            m_isInitialized = result.State;

            if (result.LogSend)
                Debug.Log($"{result.Context}: initialization state changed to {m_isInitialized}. ");

        }

        protected virtual void OnLoaded(IResult result)
        {
            m_isLoaded = result.State;

            if (result.LogSend)
                Debug.Log($"{result.Context}: load state changed to {m_isLoaded}.");
        }

        protected virtual void OnActivated(IResult result)
        {

            m_isActivated = result.State;

            if (result.LogSend)
                Debug.Log($"{result.Context}: activation state changed to {m_isActivated}.");

        }



        // COMPONENT //
        public TComponent SetComponent<TComponent>()
        where TComponent : UComponent
            => gameObject.AddComponent<TComponent>();

        public bool GetComponent<TComponent>(out TComponent component)
        where TComponent : UComponent
            => gameObject.TryGetComponent<TComponent>(out component);

        public void SetParent(Transform parent)
            => gameObject.transform.SetParent(parent);


        // UNITY //
        protected virtual void OnEnable()
            => Record();
        protected virtual void OnDisable()
            => Clear();


    }



    public interface IComponent
    {
        GameObject Obj { get; }

        TComponent SetComponent<TComponent>()
        where TComponent : UComponent;

        bool GetComponent<TComponent>(out TComponent component)
        where TComponent : UComponent;

        void SetParent(Transform parent);

    }

}

