using System;
using Core;

namespace Test
{
    public class WaitForFunc : YieldModel, IYield
    {

        public WaitForFunc(Func<bool> func)
        {
            Func = func;
        }

        public WaitForFunc(Action func)
        {
            Func = () => { func.Invoke(); return true; };
        }

    }



}