using System;
using UnityEngine;


namespace Core
{

    public struct Result : IResult
    {
        public Result(object context, bool state = false, string log = "...")
        {
            Context = context;
            State = state;
            Log = log;
        }

        public object Context { get; set; }
        public bool State { get; set; }
        public string Log { get; set; }

    }

    public interface IResult
    {
        object Context { get; set; }
        bool State { get; set; }
        string Log { get; set; }

    }

}