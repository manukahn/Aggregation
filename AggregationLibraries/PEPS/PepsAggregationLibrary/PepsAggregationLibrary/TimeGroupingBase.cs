//-----------------------------------------------------------------------
// <copyright company="Schneider Electric">
//     Copyright (c) Schneider Electric. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using Microsoft.OData.Core.UriParser.Semantic;
using SE.OIP.Common.Core.Http;

namespace PepsAggregationLibrary.Projection
{
    public abstract class TimeGroupingBase : SamplingImplementationBase
    {
        public override MethodInfo GetSamplingProcessingMethod(Type genericType)
        {
            return this.GetType().GetMethod("DoSampling");
        }

        public override Type GetResultType(Type inputType)
        {
            return typeof(DateTimeOffset);
        }

        /// <summary>
        /// Get the time zone of the entities from the request context
        /// </summary>
        /// <returns>Time zone string</returns>
        protected static string GetTimeZone()
        {
            object timezone = null;
            if (HttpContext.Current != null)
            {
                var httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
                if (httpRequestMessage != null)
                {
                    httpRequestMessage.Properties.TryGetValue(HttpProperties.Timezone, out timezone);
                }
            }

            if (timezone != null)
            {
                return timezone.ToString();
            }

            return null;
        }
    }
}