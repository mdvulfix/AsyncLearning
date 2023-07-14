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
        private Stack<BollDefault> m_Bolls;

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

            m_Bolls = new Stack<BollDefault>(10);
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
            Spawn();
            BollColorize(Color.yellow);
            BollColorize(Color.gray);



            //yield return m_AsyncController.ExecuteAsync(ColorizeAsync(Color.yellow, (result) => { Debug.Log(result.Log); }));
            //yield return m_AsyncController.ExecuteAsync(ColorizeAsync(Color.gray, (result) => { Debug.Log(result.Log); }));
            //yield return m_AsyncController.ExecuteAsync(ColorizeAsync(Color.green, (result) => { Debug.Log(result.Log); }));



        }


        private void Spawn()
        {
            for (int i = 0; i < 5; i++)
            {
                var label = "Boll " + i;
                var position = new Vector3(Random.Range(0f, 2f), Random.Range(0f, 3f), Random.Range(0f, 2f));
                var boll = Spawn<BollDefault>(label, position, m_ObjBoll, m_ObjSpawnHolder);

                boll.Init();
                boll.Activate();
                boll.SetColor(Color.red);

                m_Bolls.Push(boll);
            }

        }


        //private IEnumerator ColorizeAsync(Color color, Action<IResult> action)
        //{
        //yield return m_AsyncController.ExecuteAsync(BollColorizeAsync(color, (result) => { Debug.Log(result.Log); }));
        //action.Invoke(new Result(this, true, "All bolls colorizing done!"));
        //}



        private void BollColorize(Color color)
        {
            foreach (var boll in m_Bolls)
            {
                m_AsyncController.Awaite(() => boll.SetColor(color));
                m_AsyncController.Awaite(Random.Range(1f, 3f));
                m_AsyncController.Run();

            }

        }




        private IEnumerator BollColorizeAsync(Color color, Action<IResult> action)
        {

            foreach (var boll in m_Bolls)
            {
                yield return new WaitForSeconds(Random.Range(1f, 3f));
                yield return new WaitForFunc(() => boll.SetColor(color));

            }

            action.Invoke(new Result(this, true, "Colorizing done!"));
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