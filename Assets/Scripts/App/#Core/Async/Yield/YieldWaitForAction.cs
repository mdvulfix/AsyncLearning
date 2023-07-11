using System;

namespace Core.Async
{
    public class YieldWaitForAction : YieldModel, IYield
    {
        public YieldWaitForAction(Action func)
        {
            Func = func;
        }
    }

}