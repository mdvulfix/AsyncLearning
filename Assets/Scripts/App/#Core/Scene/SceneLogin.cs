using System;
using System.Collections.Generic;
using UnityEngine;

using Core;
using Core.Factory;


namespace Core.Scene
{
    [Serializable]
    public class SceneLogin : SceneModel, IScene
    {
        //[SerializeField] private ScreenLoading m_Loading;
        //[SerializeField] private ScreenLogin m_Login;


        public SceneLogin() { }
        public SceneLogin(params object[] args)
            => Init(args);


        public override void Init(params object[] args)
        {
            if (args.Length > 0)
            {
                base.Init(args);
                return;
            }

            // CONFIGURE BY DEFAULT //
            Debug.LogWarning($"{this}: {Label} will be initialized by default!");

            //var signals = new List<ISignal>();
            //signals.Add(m_SceneMenuActivate = new SignalSceneActivate(СacheProvider<SceneMenu>.Get()));
            //signals.Add(m_StateMenuSet = new SignalStateSet(СacheProvider<StateMenu>.Get()));

            //var config = new StateConfig(this, signals.ToArray());
            //base.Init(config);
            var index = SceneIndex.Login;
            var config = new SceneConfig(index);
            base.Init(config);

        }


        // FACTORY //
        public static SceneLogin Get(IFactory factory, params object[] args)
            => Get<SceneLogin>(factory, args);
    }


    public partial class SceneFactory : Factory<IScene>
    {
        private SceneLogin GetSceneLogin(params object[] args)
        {
            var instance = new SceneLogin(args);
            return instance;
        }
    }
}