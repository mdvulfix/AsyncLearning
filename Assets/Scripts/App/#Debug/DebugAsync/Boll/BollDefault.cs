using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yield = UnityEngine.CustomYieldInstruction;

namespace Core.Async.Test
{



    [Serializable]
    public class BollDefault : BollModel
    {

        private MeshRenderer m_Renderer;

        [SerializeField] private Color m_ColorDefault;
        [SerializeField] private Color m_Color;


        public void Configure(params object[] args)
        {
            m_ColorDefault = Color.black;
            m_Color = Color.yellow;

            m_Renderer = GetComponent<MeshRenderer>();


            SetColor(m_ColorDefault);

            base.Init();

        }

        private void Start()
        {
            //var delay = 3f;
            //StopCoroutine(nameof(SetColorAsync));
            //StartCoroutine(SetColorAsync(m_Color, delay));
            //StartCoroutine(SetColorAsync((state) => { if (state) Debug.Log("Boll color has been changed!"); }));
        }



        public void SetColor(Color color)
        {
            m_Renderer.material.color = color;
        }

        public IResult SetColorAsync(Color color)
        {
            //if (!AwaitTimeDalay(5f))
            //     return false;

            try
            {
                StartCoroutine(AwaitTimeDalay(color, 6f));
                return new Result(this, true);
            }
            catch (System.Exception)
            {
                return new Result(this, false);
            }



        }


        private IEnumerator AwaitTimeDalay(Color color, float delay)
        {
            yield return new WaitForSeconds(delay);

            m_Renderer.material.color = color;
        }

        public override void Load()
        {
            throw new NotImplementedException();
        }

        public override void Unload()
        {
            throw new NotImplementedException();
        }



        // LOAD //
        //public override IAsync Load() { return new WaitForSeconds(5f); }
        //public override IAsync Unload();


    }





}