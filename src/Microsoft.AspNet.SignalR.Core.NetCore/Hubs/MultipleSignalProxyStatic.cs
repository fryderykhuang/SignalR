// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class MultipleSignalProxyStatic : IClientProxy
    {
        private readonly IConnection _connection;
        private readonly IHubPipelineInvoker _invoker;
        private readonly IList<string> _exclude;
        private readonly IList<string> _signals;
        private readonly string _hubName;

        public MultipleSignalProxyStatic(IConnection connection, IHubPipelineInvoker invoker, IList<string> signals, string hubName, string prefix, IList<string> exclude)
        {
            _connection = connection;
            _invoker = invoker;
            _hubName = hubName;
            _signals = signals.Select(signal => prefix + _hubName + "." + signal).ToList();
            _exclude = exclude;
        }

        public Task Invoke(string method, params object[] args)
        {
            var invocation = GetInvocationData(method, args);

            var context = new HubOutgoingInvokerContext(_connection, _signals, invocation)
            {
                ExcludedSignals = _exclude
            };

            return _invoker.Send(context);
        }

        protected virtual ClientHubInvocation GetInvocationData(string method, object[] args)
        {
            return new ClientHubInvocation
            {
                Hub = _hubName,
                Method = method,
                Args = args
            };
        }
    }
}
