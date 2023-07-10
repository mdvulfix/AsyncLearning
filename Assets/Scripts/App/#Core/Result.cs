using System;
using UnityEngine;


namespace Core
{


    public interface IResult<T> : IResult
    {
        T Instance { get; set; }
    }

    public interface IResult
    {

        object Context { get; set; }
        bool State { get; set; }
        string Log { get; set; }
        bool LogSend { get; set; }
    }


    public struct Result<T> : IResult<T>
    {

        public Result(T context, bool state = false, string log = "...", bool logSend = true)
        {
            Context = context;
            State = state;
            Log = log;
            LogSend = logSend;

            Instance = context;

        }

        public T Instance { get; set; }

        public object Context { get; set; }
        public bool State { get; set; }
        public string Log { get; set; }
        public bool LogSend { get; set; }

    }




    public struct Result : IResult
    {
        public Result(object context, bool state = false, string log = "...", bool logSend = true)
        {
            Context = context;
            State = state;
            Log = log;
            LogSend = logSend;

        }

        public object Context { get; set; }
        public bool State { get; set; }
        public string Log { get; set; }
        public bool LogSend { get; set; }
    }

}