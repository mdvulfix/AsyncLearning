using System;


namespace Core
{
    public interface ILoadable
    {

        event Action<IResult> Loaded;
        event Action<IResult> Unloaded;

        void Load();
        void Unload();

    }
}

