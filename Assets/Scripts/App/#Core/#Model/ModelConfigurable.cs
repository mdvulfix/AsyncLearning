using System;
using UnityEngine;

namespace Core
{
    public abstract class ModelConfigurable
    {

        [SerializeField] private bool m_isInitialized;

        public enum Params
        {
            Config,
            Factory
        }


        // SUBSCRIBE //
        public abstract void Subscribe();
        public abstract void Unsubscribe();


        // CONFIGURE //
        public abstract void Init(params object[] args);
        public abstract void Dispose();



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


        protected virtual void OnInitialized(IResult result)
        {
            m_isInitialized = result.State;

            if (result.LogSend)
                Debug.Log($"{result.Context}: initialization state changed to {m_isInitialized}. ");

        }


    }


}

