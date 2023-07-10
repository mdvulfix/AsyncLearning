using System;


namespace Core
{
    public interface IConfigurable : IDisposable
    {
        void Init(params object[] args);
    }
}

