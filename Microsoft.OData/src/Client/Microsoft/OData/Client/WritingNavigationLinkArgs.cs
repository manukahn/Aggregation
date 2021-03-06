//   OData .NET Libraries
//   Copyright (c) Microsoft Corporation
//   All rights reserved. 

//   Licensed under the Apache License, Version 2.0 (the ""License""); you may not use this file except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

//   THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 

//   See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.

namespace Microsoft.OData.Client
{
    using Microsoft.OData.Core;

    /// <summary>
    /// Writing navigation link arguments
    /// </summary>
    public sealed class WritingNavigationLinkArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WritingNavigationLinkArgs"/> class.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public WritingNavigationLinkArgs(ODataNavigationLink link, object source, object target)
        {
            Util.CheckArgumentNull(link, "link");
            Util.CheckArgumentNull(source, "source");
            Util.CheckArgumentNull(target, "target");
            this.Link = link;
            this.Source = source;
            this.Target = target;
        }

        /// <summary>
        /// Gets the link.
        /// </summary>
        /// <value>
        /// The link.
        /// </value>
        public ODataNavigationLink Link { get; private set; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        public object Source { get; private set; }

        /// <summary>
        /// Gets the target.
        /// </summary>
        public object Target { get; private set; }
    }
}
