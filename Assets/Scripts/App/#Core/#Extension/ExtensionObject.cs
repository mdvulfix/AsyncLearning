
using System;
using UnityEngine;

namespace Core
{
    public static class ExtensionObject
    {

        public static string GetName(this object instance) =>
            instance.GetType().Name;

        public static T To<T>(this object instance) =>
            (T)instance;

        public static int ToInt(this object instance)
            => instance.To<int>();


    }
}
