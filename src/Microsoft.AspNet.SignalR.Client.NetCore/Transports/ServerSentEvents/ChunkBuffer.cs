// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Microsoft.AspNet.SignalR.Client.Transports.ServerSentEvents
{
    public class ChunkBuffer
    {
        private int _currentLength;
        private static readonly ArrayPool<byte> BufferPool;
        private byte[] _lineBuffer;
        private Queue<ArraySegment<>>

        static ChunkBuffer()
        {
            BufferPool = ArrayPool<byte>.Create();
        }

        public ChunkBuffer()
        {
            _lineBuffer = BufferPool.Rent(4000);
        }

        public bool HasChunks
        {
            get
            {
                return _offset < _buffer.Length;
            }
        }

        public void Add(byte[] buffer, int length)
        {
//            Buffer.BlockCopy(buffer,  ; _buffer

            _buffer.Append(Encoding.UTF8.GetString(buffer, 0, length));
        }

        public void Add(ArraySegment<byte> buffer, Action<ArraySegment<byte>> msgAction)
        {
            for (int i = buffer.Offset; i < buffer.Count; i++)
            {
                if (buffer.Array[i] != (byte)'\r')
                    continue;
                if (buffer.Array[i + 1] != (byte)'\n')
                    continue;

                var len = _currentLength + i;
                if (len > _lineBuffer.Length)
                {
                    _lineBuffer = BufferPool.Rent(len);
                }

                var offset = i - buffer.Offset;
                Buffer.BlockCopy(buffer.Array, buffer.Offset, _lineBuffer, _currentLength, offset);
                _currentLength += offset;
                msgAction(new ArraySegment<byte>(_lineBuffer, 0, _currentLength));


                msgAction(new ArraySegment<byte>(buffer.Array, i+2, buffer.Count - offset - 2));

                _currentLength = 0;



                return;
            }

            var len2 = _currentLength + buffer.Count;
            if (len2 > _lineBuffer.Length)
            {
                _lineBuffer = BufferPool.Rent(len2);
            }

            Buffer.BlockCopy(buffer.Array, buffer.Offset, _lineBuffer, _currentLength, buffer.Count);
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
