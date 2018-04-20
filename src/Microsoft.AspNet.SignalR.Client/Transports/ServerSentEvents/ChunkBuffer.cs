﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.AspNet.SignalR.Client.Transports.ServerSentEvents
{
    public class ChunkBuffer
    {
        private int _offset;
        private readonly byte[] _buffer;
        private readonly StringBuilder _lineBuilder;

        public ChunkBuffer()
        {
            BufferPool = ArrayPool<byte>.Create();
            _buffer = new StringBuilder();
            _lineBuilder = new StringBuilder();
        }

        private static ArrayPool<byte> BufferPool;

        public bool HasChunks
        {
            get
            {
                return _offset < _buffer.Length;
            }
        }

        public void Add(byte[] buffer, int length)
        {
            Add(new ArraySegment<byte>(buffer, 0, length));
        }

        public void Add(ArraySegment<byte> buffer, Action<ArraySegment<byte>> msgAction)
        {
            for (int i = buffer.Offset; i < buffer.Count; i++)
            {
                if (buffer.Array[i] != (byte) '\r')
                    continue;
                if (buffer.Array[i + 1] != (byte) '\n')
                    continue;

            }

            msgAction()

            _buffer.Append(Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count));
        }

        public string ReadLine()
        {
            // Lock while reading so that we can make safe assuptions about the buffer indicies
            for (int i = _offset; i < _buffer.Length; i++, _offset++)
            {
                if (_buffer[i] == '\n')
                {
                    _buffer.Remove(0, _offset + 1);

                    string line = _lineBuilder.ToString().Trim();
                    _lineBuilder.Clear();

                    _offset = 0;
                    return line;
                }

                _lineBuilder.Append(_buffer[i]);
            }

            return null;
        }
    }
}
