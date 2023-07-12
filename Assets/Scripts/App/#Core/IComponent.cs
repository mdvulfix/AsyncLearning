using UnityEngine;
using UComponent = UnityEngine.Component;

namespace Core
{
    public interface IComponent
    {
        GameObject Obj { get; }

        TComponent SetComponent<TComponent>()
        where TComponent : UComponent;

        bool GetComponent<TComponent>(out TComponent component)
        where TComponent : UComponent;

        void SetParent(Transform parent);

    }

}

