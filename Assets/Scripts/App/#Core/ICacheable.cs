using System;

namespace Core
{
    public interface ICacheable
    {
        event Action<IResult> Recorded;
        event Action<IResult> Cleared;

        void Record();
        void Clear();
    }
}

