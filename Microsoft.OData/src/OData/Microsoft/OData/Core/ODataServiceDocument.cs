//   OData .NET Libraries
//   Copyright (c) Microsoft Corporation
//   All rights reserved. 

//   Licensed under the Apache License, Version 2.0 (the ""License""); you may not use this file except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

//   THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 

//   See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.

namespace Microsoft.OData.Core
{
    #region Namespaces
    using System.Collections.Generic;
    #endregion Namespaces

    /// <summary>
    /// Class representing the a service document.
    /// </summary>
    public sealed class ODataServiceDocument : ODataAnnotatable
    {
        /// <summary>Gets or sets the set of entity sets in the service document.</summary>
        /// <returns>The set of entity sets in the service document.</returns>
        public IEnumerable<ODataEntitySetInfo> EntitySets
        {
            get;
            set;
        }

        /// <summary>Gets or sets the set of singletons in the service document.</summary>
        /// <returns>The set of singletons in the service document.</returns>
        public IEnumerable<ODataSingletonInfo> Singletons
        {
            get;
            set;
        }

        /// <summary>Gets or sets the set of function imports in the service document.</summary>
        /// <returns>The set of function imports in the service document.</returns>
        public IEnumerable<ODataFunctionImportInfo> FunctionImports
        {
            get;
            set;
        }
    }
}
