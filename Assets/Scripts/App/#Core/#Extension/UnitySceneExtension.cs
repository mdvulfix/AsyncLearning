using UnityEngine;
using UnityEngine.SceneManagement;
using UScene = UnityEngine.SceneManagement.Scene;

namespace Core
{



    public static class UnitySceneExtension
    {
        public static bool TryGetComponentOnRootGameObject<T>(this UScene scene, out T instance, string objName = null)
            where T : Component
        {
            instance = null;
            var objs = scene.GetRootGameObjects();

            foreach (var obj in objs)

                if (obj.TryGetComponent(out instance))
                    return true;

            Debug.LogWarning($" {scene} do not has scene component!");
            return false;
        }



        private static bool ValidateComponentExistence<T>(GameObject obj, out T comp)
            where T : Component
        {
            comp = obj.GetComponent<T>();
            if (comp != null)
            {
                return true;
            }

            return false;
        }

    }
}