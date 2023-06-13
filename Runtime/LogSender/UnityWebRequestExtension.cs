using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace LogSystem
{
    internal static class UnityWebRequestExtension
    {
        internal static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation reqOp)
        {
            TaskCompletionSource<UnityWebRequest.Result> tsc = new();
            reqOp.completed += _ => tsc.TrySetResult(reqOp.webRequest.result);
 
            if (reqOp.isDone)
                tsc.TrySetResult(reqOp.webRequest.result);
 
            return tsc.Task.GetAwaiter();
        }
    }
}