using System;

namespace Microsoft.AspNet.SignalR.Core
{
    static class ThreadStaticMemoryPool
    {
        [ThreadStatic]
        static byte[] _buffer = null;

        public static byte[] GetBuffer()
        {
            return _buffer ?? (_buffer = new byte[262120]);
        }
    }
}