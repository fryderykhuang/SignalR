// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNetCore.Http;

//using Microsoft.Owin;

namespace Microsoft.AspNet.SignalR.Owin.Middleware
{
    public class HubDispatcherMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HubConfiguration _configuration;

        public HubDispatcherMiddleware(RequestDelegate next, HubConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context?.Response == null)
            {
                throw new ArgumentNullException("context");
            }

            context.Response.OnStarting(async state =>
            {
                var ctx = (HttpContext) state;
                if (JsonUtility.TryRejectJSONPRequest(_configuration, ctx))
                {
                    return;
                }
                
                var dispatcher = new HubDispatcher(_configuration);

                dispatcher.Initialize(_configuration.Resolver);
                ctx.Response.StatusCode = 200;
                await dispatcher.ProcessRequest(ctx);

            }, context);

            await this._next(context);
        }
    }
}
