using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UScene = UnityEngine.SceneManagement.Scene;

using Core;
//using Core.Signal;
using Core.Factory;

namespace Core.Scene
{
    [Serializable]
    public class SceneController : ModelConfigurable, ISceneController
    {

        //private IScene m_Scene;
        //private ISignal m_SignalSceneActivate;

        [SerializeField] private SceneLogin m_SceneLogin;

        private IFactory m_SceneFactory;
        private List<IScene> m_Scenes;
        private IScene m_SceneActive;



        [Header("Stats")]
        [SerializeField] private bool m_isInitialized;


        [Header("Debug")]
        [SerializeField] protected bool m_isDebug = true;

        [Header("Config")]
        [SerializeField] protected SceneControllerConfig m_Config;



        public event Action<IResult> Initialized;
        public event Action<IResult> SceneLoaded;
        public event Action<IResult> SceneActivated;



        public SceneController() { }
        public SceneController(params object[] args)
            => Init(args);



        // SUBSCRIBE //
        public override void Subscribe()
        {
            Initialized += OnInitialized;

            foreach (var scene in m_Scenes)
            {
                //scene.LoadRequired += OnSceneLoadRequired;
                scene.Loaded += OnSceneLoaded;
                scene.Activated += OnSceneActivated;
            }

            //SignalProvider.SignalCalled += OnSignalCalled;

        }

        public override void Unsubscribe()
        {
            foreach (var scene in m_Scenes)
            {
                //scene.LoadRequired += OnSceneLoadRequired;

                scene.Activated -= OnSceneActivated;
                scene.Loaded -= OnSceneLoaded;
            }

            Initialized -= OnInitialized;

            //SignalProvider.SignalCalled -= OnSignalCalled;

        }


        // CONFIGURE //
        public override void Init(params object[] args)
        {
            var config = (int)Params.Config;

            if (args.Length > 0)
                try { m_Config = (SceneControllerConfig)args[config]; }
                catch { if (m_isDebug) Debug.LogWarning("Config was not found. Configuration failed!"); return; }


            m_SceneFactory = m_Config.SceneFactory;
            m_Scenes = new List<IScene>();
            m_SceneLogin = Register<SceneLogin>(SceneIndex.Login);


            Subscribe();

            var log = $"{this}: initialized.";
            var result = new Result(this, true, log, m_isDebug);
            Initialized?.Invoke(result);

        }

        public override void Dispose()
        {
            foreach (var scene in m_Scenes)
                scene.Dispose();

            var log = $"{this}: disposed.";
            var result = new Result(this, false, log, m_isDebug);
            Initialized?.Invoke(result);

            Unsubscribe();

        }


        public void Enter() { }

        public void Login()
        {
            //SceneLoad(m_SceneLogin);
            //SceneActivate(m_SceneLogin);
        }


        public void Menu() { }
        //=> SceneActivate(m_SceneMenu);

        public void Level() { }
        //=> SceneActivate(m_SceneLevel);

        public void Exit() { }


        /*

        // LOAD //
        private void SceneLoad(IScene scene)
        {

            if (scene == null) { $"{scene.GetName()} not found!".Send(this, m_isDebug, LogFormat.Warning); return; }
            if (scene.isLoaded) { $"{scene.GetName()} is already loaded!".Send(this, m_isDebug, LogFormat.Warning); return; }

            //if (scene == null) { return new Result(this, false, $"{scene.GetName()} not found!", m_isDebug, LogFormat.Warning); }
            //if (scene.isLoaded) { return new Result(this, true, $"{scene.GetName()} is already loaded!", m_isDebug, LogFormat.Warning); }

            scene.Load();
        }

        private void SceneUnload(IScene scene)
        {
            if (scene == null) { $"{scene.GetName()} not found!".Send(this, m_isDebug, LogFormat.Warning); return; }
            if (!scene.isLoaded) { $"{scene.GetName()} is already unloaded!".Send(this, m_isDebug, LogFormat.Warning); return; }

            scene.Unload();
        }

        // ACTIVATE //
        private void SceneActivate(IScene scene)
        {
            if (scene == null) { $"{scene.GetName()} not found!".Send(this, m_isDebug, LogFormat.Warning); return; }
            if (!scene.isLoaded) { $"{scene.GetName()} is not loaded!".Send(this, m_isDebug, LogFormat.Warning); return; }
            if (scene.isActivated) { $"{scene.GetName()} is already activated!".Send(this, m_isDebug, LogFormat.Warning); return; }

            scene.Activate();

        }

        private void SceneDeactivate(IScene scene)
        {

            if (scene == null) { $"{scene.GetName()} not found!".Send(this, m_isDebug, LogFormat.Warning); return; }
            if (!scene.isLoaded) { $"{scene.GetName()} is not loaded!".Send(this, m_isDebug, LogFormat.Warning); return; }
            if (!scene.isActivated) { $"{scene.GetName()} is already deactivated!".Send(this, m_isDebug, LogFormat.Warning); return; }

            scene.Deactivate();
        }


        */



        private TScene Register<TScene>(SceneIndex index)
        where TScene : IScene
        {
            var factory = (m_SceneFactory != null) ? m_SceneFactory : new SceneFactory();
            var scene = factory.Get<TScene>(new SceneConfig(index));
            m_Scenes.Add(scene);
            return scene;

        }



        private void OnSceneLoaded(IResult result)
        {
            if (result == null)
                return;

            var status = result.State;
            var scene = result.Context is IScene ? (IScene)result.Context : null;

            if (m_isDebug)
                Debug.Log($"{scene} load status changed to {result}!");

            switch (status)
            {
                case true:
                    //Load
                    break;

                case false:
                    //Unload
                    break;

            }

        }

        private void OnSceneActivated(IResult result)
        {
            if (result == null)
                return;

            var status = result.State;
            var scene = result.Context is IScene ? (IScene)result.Context : null;

            if (m_isDebug)
                Debug.Log($"{scene} activation status changed to {result}!");


            switch (status)
            {
                case true:
                    //m_SceneActive = scene;
                    //Load
                    break;

                case false:
                    //Unload
                    break;

            }
        }





        /*

  // CALLBACK //
  private void OnSignalCalled(ISignal signal)
  {

      //if(signal is SignalSceneActivate)


  }


  private void OnSignalCached(ISignal signal)
  {

      //if(signal is SignalSceneActivate)
      //    SceneAc

  }
   */
    }
}


namespace Core
{
    public interface ISceneController : IController
    {
        event Action<IResult> SceneLoaded;
        event Action<IResult> SceneActivated;

        void Enter();
        void Login();
        void Menu();
        void Level();
        void Exit();

    }

    public struct SceneControllerConfig : IConfig
    {
        public SceneControllerConfig(IFactory sceneFactory)
        {
            SceneFactory = sceneFactory;
        }

        public IFactory SceneFactory { get; private set; }
    }
}