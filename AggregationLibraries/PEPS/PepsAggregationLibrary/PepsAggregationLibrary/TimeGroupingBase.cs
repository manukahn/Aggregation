//-----------------------------------------------------------------------
// <copyright company="Schneider Electric">
//     Copyright (c) Schneider Electric. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using Microsoft.OData.Core.UriParser.Semantic;

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
    }
}