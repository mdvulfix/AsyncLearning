
using System;
using UnityEngine;

namespace Core
{
    public static class ObjectExtension
    {
        public static string GetName(this object instance)
            => instance.GetType().Name;

        public static T Convert<T>(this object instance)
            => (T)instance;

        public static bool Is<T>(this object instance)
            => instance is T;


    }
}
