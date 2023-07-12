using System;
using UnityEngine;

namespace Core.Async
{
    public class YieldWaitForAction : YieldModel, IYield
    {

        protected bool m_isComplete = false;
        protected Action m_Resolve;

        public override bool keepWaiting => !m_isComplete;

        public YieldWaitForAction(Action func)
        {
            Func = func;
        }

        public override IYield Run(Action action)
        {


            m_Resolve += action;
            Debug.Log($"{this.GetName()}: Start waiting...");
            return this;
        }

        public override IYield Resolve()
        {
            m_isComplete = true;
            m_Resolve?.Invoke();
            Debug.Log($"{this.GetName()}: Waiting commplite. Yield resolved.");
            return this;
        }


        public override void Reset()
        {
            m_isComplete = false;
            m_Resolve = null;
            Debug.Log($"{this.GetName()}: Yield reseted.");
        }

    }

}