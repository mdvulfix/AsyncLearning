using System;
using System.Collections;
using Yield = UnityEngine.CustomYieldInstruction;

namespace Core
{

    public abstract class YieldModel : Yield
    {
        protected bool m_isComplete = false;
        protected Action m_Resolve;


        public Action Func { get; protected set; }

        public override bool keepWaiting => !m_isComplete;

        public virtual void Run(Action action)
        {
            m_Resolve += action;
        }

        public virtual void Resolve()
        {
            m_isComplete = true;
            m_Resolve?.Invoke();
        }


        public override void Reset()
        {
            m_isComplete = false;
            m_Resolve = null;
        }



        public virtual void Dispose()
        {
            Reset();
        }


        public IEnumerator GetEnumerator()
        {
            // Запускаем асинхронную функцию в новом потоке
            // Добавляем задержку, чтобы дать возможность другим корутинам выполниться
            yield return null;

            // Выполняем асинхронную функцию
            Func.Invoke();

            // Завершаем
            Resolve();
        }
    }
}