using System;
using System.Collections;

namespace Core
{
    public interface IYield : IEnumerator, IDisposable
    {
        bool keepWaiting { get; }
        Action Func { get; }

        void Run(Action action);
        void Resolve();

    }
}