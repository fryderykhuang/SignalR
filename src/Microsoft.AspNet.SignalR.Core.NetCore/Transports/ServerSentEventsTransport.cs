// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Core;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNetCore.Http;
using Utf8Json;

namespace Microsoft.AspNet.SignalR.Transports
{
    public class ServerSentEventsTransport : ForeverTransport
    {
        private static byte[] _keepAlive = Encoding.UTF8.GetBytes("data: {}\n\n");
        private static byte[] _dataInitialized = Encoding.UTF8.GetBytes("data: initialized\n\n");

        public ServerSentEventsTransport(HttpContext context, IDependencyResolver resolver)
            : base(context, resolver)
        {
        }

        public override Task KeepAlive()
        {
            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return EnqueueOperation(state => PerformKeepAlive(state), this);
        }

        public override Task Send(PersistentResponse response)
        {
            OnSendingResponse(response);

            var context = new SendContext(this, response);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return EnqueueOperation(state => PerformSend(state), context, state =>
            {
                var ctx = (SendContext) state;
                if (ctx.State is PersistentResponse resp)
                {
                    resp.OnMessageDropped();
                }
            });
        }

        protected internal override Task InitializeResponse(ITransportConnection connection)
        {
            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return base.InitializeResponse(connection)
                       .Then(s => WriteInit(s), this);
        }

        private static Task PerformKeepAlive(object state)
        {
            var transport = (ServerSentEventsTransport)state;

            transport.Context.Response.Body.Write(_keepAlive, 0, _keepAlive.Length);

            return transport.Context.Response.Body.FlushAsync();
        }

        private static byte[] _prefix = Encoding.UTF8.GetBytes("data: ");
        private static byte[] _suffix = Encoding.UTF8.GetBytes("\n\n");

        private static Task PerformSend(object state)
        {
            var context = (SendContext)state;

            if (context.State is IJsonWritable selfSerializer)
            {
                var srcbuf = ThreadStaticMemoryPool.GetBuffer();
                var writer = new JsonWriter(srcbuf);
                try
                {
                    writer.WriteRaw(_prefix);
                    selfSerializer.WriteJson(ref writer);
                    writer.WriteRaw(_suffix);
                    var buf = writer.GetBuffer();
                    context.Transport.Context.Response.Body.Write(buf.Array, buf.Offset, buf.Count);

                    var flushtask = context.Transport.Context.Response.Body.FlushAsync();
//                    flushtask.ContinueWith((t, s) => BufferPool.Return((byte[]) s), srcbuf);
                    return flushtask;
                    //                        context.Transport.TraceOutgoingMessage(writer.Buffer);
                }
                catch (Exception ex)
                {
                    // OnError will close the socket in the event of a JSON serialization or flush error.
                    // The client should then immediately reconnect instead of simply missing keep-alives.
                    context.Transport.OnError(ex);
                    throw;
                }
            }
            else
            {
                using (var writer = new BinaryMemoryPoolTextWriter(context.Transport.Pool))
                {
                    writer.Write("data: ");
                    context.Transport.JsonSerializer.Serialize(context.State, writer);
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.Flush();

                    var buf = writer.Buffer;
                    context.Transport.Context.Response.Body.Write(buf.Array, buf.Offset, buf.Count);

                    //                    context.Transport.TraceOutgoingMessage(writer.Buffer);
                    return context.Transport.Context.Response.Body.FlushAsync();
                }
            }

        }

        private static Task WriteInit(ServerSentEventsTransport transport)
        {
            transport.Context.Response.ContentType = "text/event-stream";

            transport.Context.Response.Body.Write(_dataInitialized, 0, _dataInitialized.Length);

            return transport.Context.Response.Body.FlushAsync();
        }

        private class SendContext
        {
            public readonly ServerSentEventsTransport Transport;
            public readonly object State;

            public SendContext(ServerSentEventsTransport transport, object state)
            {
                Transport = transport;
                State = state;
            }
        }
    }
}
