using System;
using System.Collections;
using UnityEngine;
using Core;
using Core.Factory;

namespace Core.Async
{

    public class AwaiterDefault : AwaiterModel, IAwaiter
    {




        public event Action<IAwaiter> FuncInvoked;
        public event Action<IAwaiter> FuncExecuted;


        public static string PREF_NAME = "AwaiterDefault";

        public AwaiterDefault() { }
        public AwaiterDefault(params object[] args)
            => Init(args);


        public override void Init(params object[] args)
        {
            if (args.Length > 0)
            {
                base.Init(args);
                return;
            }

            // CONFIGURE BY DEFAULT //
            Debug.LogWarning($"{this.GetName()}: {Label} will be initialized by default!");


            var config = new AwaiterConfig(Label, null);
            base.Init(config);

        }


        public override void Run(IYield func)
        {
            FuncInvoked?.Invoke(this);
            StartCoroutine(ExecuteAsync(func, (result) =>
            {
                if (result.State)
                    Resolve();

            }));

        }

        public override void Resolve()
        {
            FuncExecuted?.Invoke(this);

        }






        /*
        public override IEnumerator ExecuteAsync(IEnumerator func, Action<IResult> callback)
        {
            SetState(false);
            FuncInvoked?.Invoke(this);

            if (m_Coroutine != null)
                StopCoroutine(m_Coroutine);

            yield return m_Coroutine = StartCoroutine(func);
            callback?.Invoke(new Result(this, true, $"Async operation done!"));

            SetState(true);
            FuncExecuted?.Invoke(this);

        }
        */


        // FACTORY //
        public static AwaiterDefault Get(IFactory factory, params object[] args)
            => Get<AwaiterDefault>(factory, args);
    }



    public partial class AwaiterFactory : Factory<IAwaiter>, IFactory
    {
        private AwaiterDefault GetAwaiterDefault(params object[] args)
        {
            var prefabPath = $"{AwaiterModel.PREF_FOLDER}/{AwaiterDefault.PREF_NAME}";
            var prefab = Resources.Load<GameObject>(prefabPath);

            var obj = (prefab != null) ?
            GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) :
            new GameObject("Awaiter");

            obj.SetActive(false);

            var instance = obj.AddComponent<AwaiterDefault>();
            obj.name = $"Awaiter";

            if (args.Length > 0)
                try { instance.Init((AwaiterConfig)args[(int)AwaiterModel.Params.Config]); }
                catch { Debug.LogWarning($"{this.GetName()}: config was not found. Configuration failed!"); }

            return instance;
        }
    }
}

