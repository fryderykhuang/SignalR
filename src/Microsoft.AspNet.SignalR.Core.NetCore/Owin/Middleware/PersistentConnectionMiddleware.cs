// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Core;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNetCore.Http;
//using Microsoft.Owin;

namespace Microsoft.AspNet.SignalR.Owin.Middleware
{
    public class PersistentConnectionMiddleware
    {
        private readonly Type _connectionType;
        private readonly ConnectionConfiguration _configuration;

        public PersistentConnectionMiddleware(RequestDelegate next,
                                              Type connectionType,
                                              ConnectionConfiguration configuration)
        {
            _connectionType = connectionType;
            _configuration = configuration;
        }

        public Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (JsonUtility.TryRejectJSONPRequest(_configuration, context))
            {
                return TaskAsyncHelper.Empty;
            }

            var connectionFactory = new PersistentConnectionFactory(_configuration.Resolver);
            PersistentConnection connection = connectionFactory.CreateInstance(_connectionType);

            connection.Initialize(_configuration.Resolver);

            return connection.ProcessRequestCore(context);
        }
    }
}
