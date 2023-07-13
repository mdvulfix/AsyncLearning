using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Core.Async;

namespace Test
{

    [Serializable]
    public class BollDefault : BollLoadable
    {

        private MeshRenderer m_Renderer;

        [SerializeField] private Color m_ColorDefault;
        [SerializeField] private Color m_Color;


        public BollDefault() { }
        public BollDefault(params object[] args)
            => Init(args);


        public override void Load()
        {
            //var delay = 3f;
            //StopCoroutine(nameof(SetColorAsync));
            //StartCoroutine(SetColorAsync(m_Color, delay));
            //StartCoroutine(SetColorAsync((state) => { if (state) Debug.Log("Boll color has been changed!"); }));

            m_ColorDefault = Color.black;
            m_Color = Color.yellow;

            m_Renderer = GetComponent<MeshRenderer>();


            SetColor(m_ColorDefault);



            Init();
            base.Load();
        }


        public override void Unload()
        {
            Dispose();
            base.Unload();
        }

        private IEnumerator Start()
        {

            yield return null;

            var delay = 10f;
            var timer = delay;
            yield return new WaitForFunc(() => WaitForTimer(ref timer));

        }

        private void Update()
        {

            var isKeyDown = false;

            isKeyDown = Input.GetKeyDown(KeyCode.Space) ? true : false;

            if (isKeyDown)
                Debug.Log($"{this.GetName()} Key {KeyCode.Space} is down!");

        }



        public class WaitForFunc : YieldModel, IYield
        {

            public WaitForFunc(Func<bool> func)
            {
                Func = func;
            }



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

        public override void SetColor(Color color)
        {
            m_Renderer.material.color = color;
        }


    }





}