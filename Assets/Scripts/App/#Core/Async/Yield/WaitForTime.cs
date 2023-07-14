using UnityEngine;

namespace Core.Async
{
    public class WaitForTime : YieldModel, IYield
    {

        private float m_Timer;

        public WaitForTime(float delay)
        {
            m_Timer = delay;
            Func = () => WaitForTimer(ref m_Timer);
        }

        public bool WaitForTimer(ref float timer)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
                return true;

            return false;
        }

    }
}