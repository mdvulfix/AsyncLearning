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

        private void Start()
        {
            StartCoroutine(WaitForTimer(2));
        }



        public override void SetColor(Color color)
        {
            m_Renderer.material.color = color;
        }



        public IYield GetYield(float delay)
        {
            return new YieldWaitForAction(() => WaitForTimer(delay));

        }




        public IEnumerator WaitForTimer(float delay)
        {
            Debug.Log($"{this.GetName()} Delay {delay}. Timer started.");

            while (delay > 0)
            {
                Debug.Log($"{this.GetName()} Delay {delay}");
                yield return null;
                delay -= Time.deltaTime;
            }

            Debug.Log($"{this.GetName()} Delay {delay}. Timer finished.");
        }

        public bool Timer(float delay)
        {

            delay -= Time.deltaTime;
            Debug.Log($"{this.GetName()} Delay {delay}.");
            if (delay <= 0)
                return false;
            else
                return true;



        }


    }





}