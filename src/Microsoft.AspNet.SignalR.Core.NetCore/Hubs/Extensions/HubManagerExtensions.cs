// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNet.SignalR.Core;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public static class HubManagerExtensions
    {
        public static HubDescriptor EnsureHub(this IHubManager hubManager, string hubName)
        {
            if (hubManager == null)
            {
                throw new ArgumentNullException("hubManager");
            }

            if (String.IsNullOrEmpty(hubName))
            {
                throw new ArgumentNullException("hubName");
            }

            var descriptor = hubManager.GetHub(hubName);

            return descriptor;
        }

        public static IEnumerable<HubDescriptor> GetHubs(this IHubManager hubManager)
        {
            if (hubManager == null)
            {
                throw new ArgumentNullException("hubManager");
            }

            return hubManager.GetHubs(d => true);
        }

        public static IEnumerable<MethodDescriptor> GetHubMethods(this IHubManager hubManager, string hubName)
        {
            if (hubManager == null)
            {
                throw new ArgumentNullException("hubManager");
            }

            return hubManager.GetHubMethods(hubName, m => true);
        }
    }
}
