using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Factory
{
    public class Factory<T> : FactoryDefault, IFactory<T>
    where T : IConfigurable
    {

        public T Get(params object[] args)
            => Get<T>(args);

        public void Set(IConstructor constructor)
            => Set<T>(constructor);

    }



    public class FactoryDefault : FactoryModel, IFactory
    {

        public void Set<T>(IConstructor constructor)
            => AddConstructor<T>(constructor);

        public override T Get<T>(params object[] args)
        {
            if (GetConstructor<T>(out var constructor))
                return (T)constructor.Create<T>(args);
            else
                return (T)Activator.CreateInstance(typeof(T), args);
        }

    }


    public abstract class FactoryModel
    {
        public string Name => this.GetName();
        public Type Type => this.GetType();

        protected Dictionary<Type, IConstructor> m_Constractors = new Dictionary<Type, IConstructor>(15);

        public abstract T Get<T>(params object[] args)
        where T : IConfigurable;

        protected bool GetConstructor<T>(out IConstructor constructor, params object[] args)
            => m_Constractors.TryGetValue(typeof(T), out constructor);

        protected void AddConstructor<T>(IConstructor constructor)
            => AddConstructor(typeof(T), constructor);

        protected void AddConstructor(Type instanceType, IConstructor constructor)
        {
            try
            { m_Constractors.Add(instanceType, constructor); }
            catch (Exception exeption)
            { Debug.LogWarning($"The instance constructor is already added! Exeption: {exeption.Message}"); }
        }

    }


    public interface IFactory<T> : IFactory
    where T : IConfigurable
    {
        T Get(params object[] args);
    }


    public interface IFactory
    {

        T Get<T>(params object[] args)
        where T : IConfigurable;
    }



    public delegate T PredefinedConstructor<T>(params object[] args);

    public class Constructor : IConstructor
    {
        private PredefinedConstructor<IConfigurable> m_Func;

        public Constructor(PredefinedConstructor<IConfigurable> func)
        {
            m_Func = func;
        }


        public T Create<T>(params object[] args)
        where T : IConfigurable
            => (T)m_Func.Invoke(args);


        public static IConstructor Get(PredefinedConstructor<IConfigurable> func)
            => new Constructor(func);
    }

    public interface IConstructor
    {
        T Create<T>(params object[] args)
        where T : IConfigurable;
    }


}