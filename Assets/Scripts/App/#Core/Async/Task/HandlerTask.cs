using System;
using System.Threading.Tasks;
using UnityEngine;
using TokenSource = System.Threading.CancellationTokenSource;
using Token = System.Threading.CancellationToken;


public class HandlerTask
{
    
    public delegate bool TaskHandler();
    
    
    private static TokenSource m_Source = new TokenSource();
    private Token m_Token;

    
    public HandlerTask()
    {
        m_Token = m_Source.Token;
    }

    public void Init()
    {
        
    }
    
    public void Dispose()
    {
        m_Source.Cancel();
        m_Source.Dispose();
    }


    public bool Execute<TTaskInfo, TResult>(Func<bool> task, float delay, out TResult result)
        where TTaskInfo: ITaskInfo<TResult>, new()
    {
        
        
        var taskInfo = Run<TTaskInfo, TResult>(task, delay).Result;
        //task.Invoke();

        //Debug.Log(taskInfo.Message);
        /*
        if(status)
        {
            result = default(TResult);
            return true;
        }
        */
        result = default(TResult);
        return true;
    }

    public TTaskInfo Run<TTaskInfo>(Func<bool> task, float delay)  
        where TTaskInfo: ITaskInfo, new()
    {
        var tokenSource = new TokenSource();
        var token = tokenSource.Token;

        var res = task.Invoke();
        //await TaskExecuteAsync<TTaskInfo>(task, token, delay);
        return Send<TTaskInfo>(res, $"Awaiting done.");
    }

    public async Task<TTaskInfo> Run<TTaskInfo, TResult>(Func<bool> task, float delay)  
        where TTaskInfo: ITaskInfo<TResult>, new()
    {
        var tokenSource = new TokenSource();
        var token = tokenSource.Token;

        return await TaskExecuteAsync<TTaskInfo>(task, token, delay);
    }

    private async Task<TTaskInfo> TaskExecuteAsync<TTaskInfo>(Func<bool> task, Token token, float delay = 1f)
        where TTaskInfo : ITaskInfo, new()
    { 
        token.ThrowIfCancellationRequested();
               
        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                m_Token.ThrowIfCancellationRequested();

                await Task.Delay(1);
                delay -= Time.deltaTime;

                if (delay <= 0)
                    break;
                
                if(true)
                    return Send<TTaskInfo>(true, $"Awaiting done.");
                
                
            }    

            token.ThrowIfCancellationRequested();

            if (delay <= 0)
                return Send<TTaskInfo>(false, "Task cancelled by time delay!");
                       
            return Send<TTaskInfo>(false, $"Task completed successfully, but...!");
        }
        catch (OperationCanceledException ex)
        {
            if (ex.CancellationToken == token)
                return Send<TTaskInfo>(false, "Await system state async task cancelled by local token!");
            else if(ex.CancellationToken == m_Token)
                return Send<TTaskInfo>(false, "Await system state async task cancelled by system token!");
            else
                return Send<TTaskInfo>(false, ex.Message);
        }
    }

    private TTaskInfo Send<TTaskInfo>(bool status, string message)
        where TTaskInfo : ITaskInfo, new()
    { 
        var taskInfo = new TTaskInfo();
        taskInfo.Configure(status, message);
        return taskInfo;
    }
    
    public static void CancelOnSystemCallback()
    { 
        m_Source.Cancel();
        m_Source.Dispose();
    }

}
public interface ITaskInfo<TResult>: ITaskInfo
{
    TResult Result {get; }
}

public interface ITaskInfo
{
    bool Status {get; }
    string Message {get; }

    void Configure(bool status, string message);
}

public interface ITask
{

}
