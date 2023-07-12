using System;

namespace Core
{
    public interface IActivable
    {

        event Action<IResult> Activated;
        event Action<IResult> Deactivated;

        void Activate();
        void Deactivate();

    }

}
