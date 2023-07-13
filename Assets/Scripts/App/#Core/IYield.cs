using System;
using System.Collections;

namespace Core
{
    public interface IYield : IEnumerator, IDisposable
    {
        bool keepWaiting { get; }

        Func<bool> Func { get; }

        event Action Resolved;

        void Resolve();

    }
}