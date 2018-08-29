// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// The response returned from an incoming hub request.
    /// </summary>
    public class HubResponse
    {
        /// <summary>
        /// The changes made the the round tripped state.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Type is used for serialization")]
        [JsonProperty("S", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "S")]
        public IDictionary<string, object> State { get; set; }

        public bool ShouldSerializeState()
        {
            return State != null;
        }

        /// <summary>
        /// The result of the invocation.
        /// </summary>
        [JsonProperty("R", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "R")]
        public object Result { get; set; }

        public bool ShouldSerializeResult()
        {
            return Result != null;
        }

        /// <summary>
        /// The id of the operation.
        /// </summary>
        [JsonProperty("I")]
        [DataMember(Name = "I")]
        public string Id { get; set; }

        /// <summary>
        /// The progress update of the invocation.
        /// </summary>
        [JsonProperty("P", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "P")]
        public object Progress { get; set; }

        public bool ShouldSerializeProgress()
        {
            return Progress != null;
        }

        /// <summary>
        /// Indicates whether the Error is a see <see cref="HubException"/>.
        /// </summary>
        [JsonProperty("H", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "H")]
        public bool? IsHubException { get; set; }

        public bool ShouldSerializeIsHubException()
        {
            return IsHubException != null;
        }

        /// <summary>
        /// The exception that occurs as a result of invoking the hub method.
        /// </summary>
        [JsonProperty("E", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "E")]
        public string Error { get; set; }

        public bool ShouldSerializeError()
        {
            return Error != null;
        }


        /// <summary>
        /// The stack trace of the exception that occurs as a result of invoking the hub method.
        /// </summary>
        [JsonProperty("T", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "T")]
        public string StackTrace { get; set; }
        
        public bool ShouldSerializeStackTrace()
        {
            return StackTrace != null;
        }

        /// <summary>
        /// Extra error data contained in the <see cref="HubException"/>
        /// </summary>
        [JsonProperty("D", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "D")]
        public object ErrorData { get; set; }

        public bool ShouldSerializeErrorData()
        {
            return ErrorData != null;
        }
    }
}
