using System;
using System.Collections;
using UnityEngine;
using Yield = UnityEngine.CustomYieldInstruction;

namespace Core
{

    public abstract class YieldModel : Yield
    {
        public Func<bool> Func { get; set; }

        public override bool keepWaiting => WaitFuncResult();

        public event Action Resolved;

        public void Resolve()
        {
            Resolved?.Invoke();
            Debug.Log($"{this.GetName()}: Waiting commplite. Yield resolved.");
        }

        public override void Reset()
        {
            Debug.Log($"{this.GetName()}: Yield reseted.");
            base.Reset();
        }

        public void Dispose()
            => Reset();


        private bool WaitFuncResult()
        {
            while (!Func.Invoke())
            {
                Debug.Log("Waiting...");
                return true;
            }

            Debug.Log("Executed!");
            Resolve();
            return false;

        }


    }
}