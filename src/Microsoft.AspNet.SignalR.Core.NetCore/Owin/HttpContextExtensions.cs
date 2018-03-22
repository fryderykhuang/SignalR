using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.SignalR.Core.Owin
{
    static class HttpContextExtensions
    {
        public static string LocalPath(this HttpRequest request)
        {
            return request.PathBase.Add(request.Path).Value;
        }
    }
}
