//   OData .NET Libraries
//   Copyright (c) Microsoft Corporation
//   All rights reserved. 

//   Licensed under the Apache License, Version 2.0 (the ""License""); you may not use this file except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

//   THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 

//   See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.

using Microsoft.OData.Edm.Expressions;

namespace Microsoft.OData.Edm.Library.Expressions
{
    /// <summary>
    /// Represents an EDM property reference expression.
    /// </summary>
    public class EdmPropertyReferenceExpression : EdmElement, IEdmPropertyReferenceExpression
    {
        private readonly IEdmExpression baseExpression;
        private readonly IEdmProperty referencedProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmPropertyReferenceExpression"/> class.
        /// </summary>
        /// <param name="baseExpression">Expression for the structured value containing the referenced property.</param>
        /// <param name="referencedProperty">Referenced property.</param>
        public EdmPropertyReferenceExpression(IEdmExpression baseExpression, IEdmProperty referencedProperty)
        {
            EdmUtil.CheckArgumentNull(baseExpression, "baseExpression");
            EdmUtil.CheckArgumentNull(referencedProperty, "referencedPropert");

            this.baseExpression = baseExpression;
            this.referencedProperty = referencedProperty;
        }

        /// <summary>
        /// Gets the expression for the structured value containing the referenced property.
        /// </summary>
        public IEdmExpression Base
        {
            get { return this.baseExpression; }
        }

        /// <summary>
        /// Gets the referenced property.
        /// </summary>
        public IEdmProperty ReferencedProperty
        {
            get { return this.referencedProperty; }
        }

        /// <summary>
        /// Gets the kind of this expression.
        /// </summary>
        public EdmExpressionKind ExpressionKind
        {
            get { return EdmExpressionKind.PropertyReference; }
        }
    }
}
