using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PromiseBlazorTest
{

    public static class Callbacks
    {
        private static Dictionary<string, Tuple<Type, object>> TaskCompletionSources =
            new Dictionary<string, Tuple<Type, object>>();

        public static void PromiseCallback(string callbackId, string result)
        {
            Tuple<Type, object> tcs = TaskCompletionSources[callbackId];
            object deserializedResult = typeof(JsonUtil)
                .GetMethod("Deserialize")
                .MakeGenericMethod(tcs.Item1)
                .Invoke(null, new[] { result });
            var setResult = typeof(TaskCompletionSource<>).MakeGenericType(tcs.Item1).GetMethod("SetResult");
            setResult.Invoke(tcs.Item2, new[] { deserializedResult });
            TaskCompletionSources.Remove(callbackId);
        }

        public static void PromiseError(string callbackId, string error)
        {
            Tuple<Type, object> tcs = TaskCompletionSources[callbackId];
            var setException = typeof(TaskCompletionSource<>).MakeGenericType(tcs.Item1)
                .GetMethod("SetException", new[] { typeof(Exception) });
            setException.Invoke(tcs.Item2, new[] { new Exception(error) });
            TaskCompletionSources.Remove(callbackId);
        }

        public static Task<TResult> RunPromiseAsync<TResult>(string fnName, object data = null)
        {
            var tcs = new TaskCompletionSource<TResult>();
            
            string callbackId = Guid.NewGuid().ToString();
            TaskCompletionSources.Add(callbackId, Tuple.Create(typeof(TResult), (object)tcs));

            RunFunction(callbackId, fnName, data);

            return tcs.Task;
        }

        private static void RunFunction(string callbackId, string fnName, object data)
        {
            if(data == null)
            {
                RegisteredFunction.Invoke<bool>("runFunction", callbackId, fnName);
            }
            else
            {
                RegisteredFunction.Invoke<bool>("runFunction", callbackId, fnName, data);
            }
        }
    }
}
