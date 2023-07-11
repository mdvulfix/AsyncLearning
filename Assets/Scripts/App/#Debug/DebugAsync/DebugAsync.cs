using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Async.Test
{
    [Serializable]
    public class DebugAsync : MonoBehaviour
    {
        [SerializeField] private GameObject m_Boll;


        private static Transform m_ObjSpawnHolder;
        private static Transform m_ObjAsyncHolder;

        public IAsyncController m_AsyncController;

        private List<IUpdatable> m_Updatable;

        private void Awake()
        {
            m_Updatable = new List<IUpdatable>(10);


        }

        private void OnEnable()
        {

            if (m_ObjSpawnHolder == null)
                m_ObjSpawnHolder = new GameObject("Spawn").transform;

            if (m_ObjAsyncHolder == null)
                m_ObjAsyncHolder = new GameObject("Async").transform;


            m_AsyncController = new AsyncController();
            var config = new AsyncControllerConfig(m_ObjAsyncHolder);
            m_AsyncController.Init(config);

            for (int i = 0; i < 1; i++)
            {
                m_Updatable.Add(m_AsyncController);
            }


            //foreach (var controller in m_Controllers)
            //    controller.Init();

        }

        private void OnDisable()
        {
            //foreach (var controller in m_Controllers)
            //   controller.Dispose();
        }

        private void Start()
        {
            Run(SpawnAsync());

        }

        private IEnumerator SpawnAsync()
        {
            for (int i = 0; i < 1; i++)
            {
                var label = "Boll " + i;
                var position = new Vector3(Random.Range(0f, 2f), Random.Range(0f, 3f), Random.Range(0f, 2f));
                var boll = Spawn<BollDefault>(label, position, m_Boll, m_ObjSpawnHolder);

                yield return Awaite(() => boll.Configure());
                yield return Awaite(() => boll.Load());
                yield return Awaite(() => boll.Activate());
                yield return Awaite(() => boll.SetColor(Color.red));

            }

        }

        private void Run(IEnumerator func)
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