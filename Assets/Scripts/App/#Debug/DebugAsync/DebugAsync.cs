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

        [SerializeField] private GameObject m_ObjBoll;

        [SerializeField] private BollDefault m_Boll;


        private static Transform m_ObjSpawnHolder;
        private static Transform m_ObjAsyncHolder;

        public IAsyncController m_AsyncController;

        private List<IUpdatable> m_Updatable;



        // LOAD //
        public override void Load()
        {
            Init();
            Activate();
            base.Load();
        }

        public override void Unload()
        {

            Deactivate();
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

            //m_Boll.Init();
            //m_Boll.Activate();
            //m_Boll.SetColor(Color.red);

            //foreach (var controller in m_Controllers)
            //    controller.Init();

            base.Init();
        }

        private void Start()
        {
            ExecuteAsync(Spawn());
            //Spawn();

        }


        private IEnumerator Spawn()
        {
            for (int i = 0; i < 5; i++)
            {
                var label = "Boll " + i;
                var position = new Vector3(Random.Range(0f, 2f), Random.Range(0f, 3f), Random.Range(0f, 2f));
                var boll = Spawn<BollDefault>(label, position, m_ObjBoll, m_ObjSpawnHolder);

                boll.Init();
                boll.Activate();
                boll.SetColor(Color.red);

                yield return new WaitForSeconds(1);
                yield return new WaitForFunc(() => boll.SetColor(Color.red));
                yield return new WaitForSeconds(2);
                yield return new WaitForFunc(() => boll.SetColor(Color.blue));
                //yield return Awaite(() => boll.Activate());
                //yield return Awaite(() => boll.SetColor(Color.red));

            }

        }


        private void ExecuteAsync(IEnumerator func)
            => m_AsyncController.Execute(func);

        //private IYield Awaite(Action action)
        //    => m_AsyncController.Awaite(action);


        private IEnumerator Run()
        {

            yield return null;

            var delay = 10f;
            var timer = delay;
            yield return new WaitForFunc(() => WaitForTimer(ref timer));

        }


        public IEnumerator SetColorAsync(BollDefault boll, Color color)
        {
            yield return new WaitForFunc(() => boll.SetColor(color));
            yield return new WaitForSeconds(2);
            yield return new WaitForFunc(() => boll.SetColor(color));
        }





        public bool WaitForKeyUp(KeyCode key)
        {
            if (Input.GetKeyUp(key))
                return true;


            return false;
        }



        public bool WaitForTimer(ref float timer)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
                return true;

            return false;

        }


        private void Update()
        {
            foreach (var controller in m_Updatable)
                controller.Update();


            var isKeyDown = false;

            isKeyDown = Input.GetKeyDown(KeyCode.Space) ? true : false;

            if (isKeyDown)
                Debug.Log($"{this.GetName()} Key {KeyCode.Space} is down!");


            m_AsyncController.Update();

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