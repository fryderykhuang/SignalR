// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    internal class DisposableAsyncAction : IDisposable
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification =
            "The client projects use this.")]
        public static readonly DisposableAsyncAction Empty = new DisposableAsyncAction(() => Task.CompletedTask);

        private Func<object, Task> _action;
        private readonly object _state;

        public DisposableAsyncAction(Func<Task> action)
            : this(state => ((Func<Task>)state).Invoke(), state: action)
        {

        }

        public DisposableAsyncAction(Func<object, Task> action, object state)
        {
            _action = action;
            _state = state;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Interlocked.Exchange(ref _action, (state) => Task.CompletedTask).Invoke(_state);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

}
