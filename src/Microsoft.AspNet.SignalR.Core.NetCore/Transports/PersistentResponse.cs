// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR.Messaging;
using Newtonsoft.Json;
using Utf8Json.Internal;
using JsonWriter = Utf8Json.JsonWriter;

namespace Microsoft.AspNet.SignalR.Transports
{
    /// <summary>
    /// Represents a response to a connection.
    /// </summary>
    public sealed class PersistentResponse : IJsonWritable
    {
        private readonly Func<Message, bool> _exclude;
        private readonly WriteCursorDelegate _writeCursor;

        public PersistentResponse()
            : this(message => false, (ref JsonWriter writer) => {})
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="PersistentResponse"/>.
        /// </summary>
        /// <param name="exclude">A filter that determines whether messages should be written to the client.</param>
        /// <param name="writeCursor">The cursor writer.</param>
        public PersistentResponse(Func<Message, bool> exclude, WriteCursorDelegate writeCursor)
        {
            _exclude = exclude;
            _writeCursor = writeCursor;
        }

        /// <summary>
        /// The list of messages to be sent to the receiving connection.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an optimization and this type is only used for serialization.")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This type is only used for serialization")]
        public IList<ArraySegment<Message>> Messages { get; set; }

        public bool Terminal { get; set; }

        /// <summary>
        /// The total count of the messages sent the receiving connection.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// True if the connection is in process of initializing
        /// </summary>
        public bool Initializing { get; set; }

        /// <summary>
        /// True if the connection was forcibly closed. 
        /// </summary>
        public bool Aborted { get; set; }

        /// <summary>
        /// True if the client should try reconnecting.
        /// </summary>
        // This is set when the host is shutting down.
        public bool Reconnect { get; set; }

        /// <summary>
        /// Signed token representing the list of groups. Updates on change.
        /// </summary>
        public string GroupsToken { get; set; }

        /// <summary>
        /// The time the long polling client should wait before reestablishing a connection if no data is received.
        /// </summary>
        public long? LongPollDelay { get; set; }

        /// <summary>
        /// Serializes only the necessary components of the <see cref="PersistentResponse"/> to JSON
        /// using Json.NET's JsonTextWriter to improve performance.
        /// </summary>
        /// <param name="writer">The <see cref="System.IO.TextWriter"/> that receives the JSON serialization.</param>
        void IJsonWritable.WriteJson(ref Utf8Json.JsonWriter writer)
        {
//            if (writer == null)
//            {
//                throw new ArgumentNullException("writer");
//            }

            writer.WriteBeginObject();

            writer.WritePropertyName("C");
            writer.WriteRaw((byte) '"');
            _writeCursor(ref writer);
            writer.WriteRaw((byte) '"');

//            // REVIEW: Is this 100% correct?
//            writer.Write('"');
//            writer.Write("C");
//            writer.Write('"');
//            writer.Write(':');
//            writer.Write('"');
//            _writeCursor(writer);
//            writer.Write('"');
//            writer.Write(',');

            if (Initializing)
            {
                writer.WriteValueSeparator();
                writer.WritePropertyName("S");
                writer.WriteInt32(1);
            }

            if (Reconnect)
            {
                writer.WriteValueSeparator();
                writer.WritePropertyName("T");
                writer.WriteInt32(1);
            }

            if (GroupsToken != null)
            {
                writer.WriteValueSeparator();
                writer.WritePropertyName("G");
                writer.WriteString(GroupsToken);
            }

            if (LongPollDelay.HasValue)
            {
                writer.WriteValueSeparator();
                writer.WritePropertyName("L");
                writer.WriteInt64(LongPollDelay.Value);
            }

            writer.WriteValueSeparator();
            writer.WritePropertyName("M");
            writer.WriteBeginArray();

            WriteMessages(ref writer);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static unsafe void MemoryCopy(ref JsonWriter writer, byte[] src, int srcOffset, int srcCount)
        {
            writer.EnsureCapacity(srcCount);
            var buffer = writer.GetBuffer().Array;
            var offset = writer.CurrentOffset;
            fixed (byte* numPtr1 = &buffer[offset])
            fixed (byte* numPtr2 = &src[srcOffset])
                Buffer.MemoryCopy((void*)numPtr2, (void*)numPtr1, (long)checked(buffer.Length - offset), (long)srcCount);

            writer.AdvanceOffset(srcCount);
        }

        private readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

        private void WriteMessages(ref Utf8Json.JsonWriter writer)
        {
            if (Messages == null)
            {
                return;
            }

            // If the writer is a binary writer then write to the underlying writer directly
//            var binaryWriter = writer as IBinaryWriter;

            bool first = true;

            for (int i = 0; i < Messages.Count; i++)
            {
                ArraySegment<Message> segment = Messages[i];
                for (int j = segment.Offset; j < segment.Offset + segment.Count; j++)
                {
                    Message message = segment.Array[j];

                    if (!message.IsCommand && !_exclude(message))
                    {
//                        if (binaryWriter != null)
//                        {
                        if (!first)
                        {
                            // We need to write the array separator manually
                            writer.WriteRaw((byte) ',');
                        }
                        
                        var buf = Utf8Json.JsonSerializer.SerializeUnsafe<object>(message.Value);
                        if (buf.Count > 0)
                            MemoryCopy(ref writer, buf.Array, buf.Offset, buf.Count);

//                        if (message.Value is ClientHubInvocation chi)
//                        {
//                            chi.AfterSerializationCallback?.Invoke(chi.Args);
//                        }

                        first = false;
//                        }
//                        else
//                        {
//                            // Write the raw JSON value
//                            jsonWriter.WriteRawValue(message.GetString());
//                        }
                    }
                }
            }
        }

        public void OnMessageDropped()
        {
//            foreach (var message in Messages)
//            {
//                for (int i = message.Offset; i < message.Count; ++i)
//                {
//                    if (message.Array == null)
//                        continue;
//                    var msg = message.Array[i];
//                    if (msg.Value is ClientHubInvocation chi)
//                    {
//                        chi.AfterSerializationCallback?.Invoke(chi.Args);
//                    }
//                }
//            }
        }
    }
}

