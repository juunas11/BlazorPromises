using Microsoft.AspNetCore.Blazor;
using System;
using System.Threading.Tasks;

namespace PromiseBlazorTest
{
    public class PromiseCallbackHandler<TResult> : IPromiseCallbackHandler
    {
        private readonly TaskCompletionSource<TResult> _tcs;

        public PromiseCallbackHandler(TaskCompletionSource<TResult> tcs)
        {
            _tcs = tcs;
        }

        public void SetResult(string json)
        {
            TResult result = JsonUtil.Deserialize<TResult>(json);
            _tcs.SetResult(result);
        }

        public void SetError(string error)
        {
            var exception = new Exception(error);
            _tcs.SetException(exception);
        }
    }
}
