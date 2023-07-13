using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Core.Async;

namespace Test
{

    [Serializable]
    public class BollDefault : BollActivable
    {

        private MeshRenderer m_Renderer;

        [SerializeField] private Color m_ColorDefault;
        [SerializeField] private Color m_Color;


        public BollDefault() { }
        public BollDefault(params object[] args)
            => Init(args);


        public override void Init(params object[] args)
        {
            //var delay = 3f;
            //StopCoroutine(nameof(SetColorAsync));
            //StartCoroutine(SetColorAsync(m_Color, delay));
            //StartCoroutine(SetColorAsync((state) => { if (state) Debug.Log("Boll color has been changed!"); }));

            m_ColorDefault = Color.black;
            m_Color = Color.yellow;

            m_Renderer = GetComponent<MeshRenderer>();


            SetColor(m_ColorDefault);

            base.Init();
        }


        public override void SetColor(Color color)
        {
            m_Renderer.material.color = color;
        }


    }





}