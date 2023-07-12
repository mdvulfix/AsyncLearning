using System;
using System.Collections;
using Yield = UnityEngine.CustomYieldInstruction;

namespace Core
{

    public abstract class YieldModel : Yield
    {

        public Action Func { get; protected set; }


        public abstract IYield Run(Action action);
        public abstract IYield Resolve();


        public void Dispose()
            => Reset();



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