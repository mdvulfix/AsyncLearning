using UnityEngine;

namespace Core.Async
{
    public class WaitForKey : YieldModel, IYield
    {
        private KeyCode m_Key;

        public WaitForKey(KeyCode key)
        {
            m_Key = key;
            Func = () => WaitForKeyUp(m_Key);
        }

        public bool WaitForKeyUp(KeyCode key)
        {
            if (Input.GetKeyUp(key))
                return true;

            return false;
        }

    }
}