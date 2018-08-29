// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public abstract class SignalProxyStatic : IClientProxy
    {
        private readonly IList<string> _exclude;

        protected SignalProxyStatic(IConnection connection, IHubPipelineInvoker invoker, string signal, string hubName, string prefix, IList<string> exclude)
        {
            Connection = connection;
            Invoker = invoker;
            HubName = hubName;
            Signal = prefix + hubName + "." + signal;
            _exclude = exclude;
        }

        protected IConnection Connection { get; private set; }
        protected IHubPipelineInvoker Invoker { get; private set; }
        protected string Signal { get; private set; }
        protected string HubName { get; private set; }

        public Task Invoke(string method, params object[] args)
        {
            return Invoke(method, null, args);
        }

        public Task Invoke(string method, Action<object[]> afterSerializationCb, params object[] args)
        {
            var invocation = GetInvocationData(method, args, afterSerializationCb);

            var context = new HubOutgoingInvokerContext(Connection, Signal, invocation)
            {
                ExcludedSignals = _exclude
            };

            return Invoker.Send(context);
        }

        protected virtual ClientHubInvocation GetInvocationData(string method, object[] args, Action<object[]> afterSerializationCb)
        {
            return new ClientHubInvocation
            {
                Hub = HubName,
                Method = method,
                Args = args,
                AfterSerializationCallback = afterSerializationCb
            };
        }
    }
}
