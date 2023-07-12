using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Core.Async;

using Random = UnityEngine.Random;

namespace Test.Async
{
    [Serializable]
    public class DebugAsync : DebugModel
    {

        [SerializeField] private BollDefault m_Boll;


        private static Transform m_ObjSpawnHolder;
        private static Transform m_ObjAsyncHolder;

        public IAsyncController m_AsyncController;

        private List<IUpdatable> m_Updatable;



        // LOAD //
        public override void Load()
        {
            Init();
            base.Load();
        }

        public override void Unload()
        {
            Dispose();
            base.Unload();
        }

        public override void Init(params object[] args)
        {
            m_Updatable = new List<IUpdatable>(10);


            if (m_ObjSpawnHolder == null)
                m_ObjSpawnHolder = new GameObject("Spawn").transform;

            if (m_ObjAsyncHolder == null)
                m_ObjAsyncHolder = new GameObject("Async").transform;


            m_AsyncController = new AsyncController(new AsyncControllerConfig(m_ObjAsyncHolder));

            for (int i = 0; i < 1; i++)
            {
                m_Updatable.Add(m_AsyncController);
            }


            if (m_Boll == null)
            {
                var label = "Boll";
                var position = new Vector3(Random.Range(0f, 2f), Random.Range(0f, 3f), Random.Range(0f, 2f));
                var boll = Spawn<BollDefault>(label, position, null, m_ObjSpawnHolder);
            }

            m_Boll.Init();
            //m_Boll.Activate();
            m_Boll.SetColor(Color.red);

            //foreach (var controller in m_Controllers)
            //    controller.Init();

            base.Init();
        }

        private void Start()
        {
            //RunAsync(Spawn());

        }

        /*
        private IEnumerator Spawn()
        {
            for (int i = 0; i < 1; i++)
            {
                var label = "Boll " + i;
                var position = new Vector3(Random.Range(0f, 2f), Random.Range(0f, 3f), Random.Range(0f, 2f));
                var boll = Spawn<BollDefault>(label, position, m_Boll, m_ObjSpawnHolder);

                yield return Awaite(() => boll.Load());
                yield return Awaite(() => boll.Init());
                yield return Awaite(() => boll.Activate());
                yield return Awaite(() => boll.SetColor(Color.red));

            }

        }
        */

        private void RunAsync(IEnumerator func)
            => m_AsyncController.Run(func);

        private IYield Awaite(Action action)
            => m_AsyncController.Awaite(action);


        private void Update()
        {
            foreach (var controller in m_Updatable)
                controller.Update();
        }


        private T Spawn<T>(string name, Vector3 position, GameObject prefab, Transform parent)
        where T : Component
        {
            GameObject obj;

            if (prefab == null)
            {
                obj = new GameObject();
                obj.AddComponent<T>();
                obj.transform.position = position;
            }
            else
            {
                obj = Instantiate(prefab, position, Quaternion.identity);
            }

            if (parent == null)
                parent = transform;

            obj.name = name;
            obj.transform.SetParent(parent);
            obj.SetActive(false);

            return obj.GetComponent<T>();
        }

    }
}