//   OData .NET Libraries
//   Copyright (c) Microsoft Corporation
//   All rights reserved. 

//   Licensed under the Apache License, Version 2.0 (the ""License""); you may not use this file except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

//   THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 

//   See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.

namespace Microsoft.OData.Core.UriParser
{
    #region Namespaces
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.OData.Core.UriParser.Parsers;
    using Microsoft.OData.Core.UriParser.Semantic;
    using Microsoft.OData.Core.UriParser.TreeNodeKinds;
    using Microsoft.OData.Edm;
    using ODataErrorStrings = Microsoft.OData.Core.Strings;
    #endregion

    /// <summary>
    /// URI Utility methods.
    /// </summary>
    public static class ODataUriUtils
    {
        /// <summary>
        /// Converts the given <paramref name="value"/> to a corresponding CLR type. Expects the 
        /// <paramref name="value"/> to have already been properly unescaped from an actual Uri.
        /// </summary>
        /// <param name="value">Value from a Uri to be converted.</param>
        /// <param name="version">Version to be compliant with.</param>
        /// <returns>A CLR object that the <paramref name="value"/> represents (won't be EnumNode).</returns>
        public static object ConvertFromUriLiteral(string value, ODataVersion version)
        {
            return ODataUriUtils.ConvertFromUriLiteral(value, version, null, null);
        }

        /// <summary>
        /// Converts the given <paramref name="value"/> to a corresponding CLR type. Expects the 
        /// <paramref name="value"/> to have already been properly unescaped from an actual Uri.
        /// </summary>
        /// <param name="value">Value from a Uri to be converted.</param>
        /// <param name="version">Version to be compliant with.</param>
        /// <param name="model">Optional model to perform verification against.</param>
        /// <param name="typeReference">Optional IEdmTypeReference to perform verification against. 
        ///  Callers must provide a <paramref name="model"/> containing this type if it is specified.</param>
        /// <returns>A CLR object that the <paramref name="value"/> represents or an EnumNode.</returns>
        public static object ConvertFromUriLiteral(string value, ODataVersion version, IEdmModel model, IEdmTypeReference typeReference)
        {
            ExceptionUtils.CheckArgumentNotNull(value, "value");
            if (typeReference != null && model == null)
            {
                throw new ODataException(ODataErrorStrings.ODataUriUtils_ConvertFromUriLiteralTypeRefWithoutModel);
            }

            if (model == null)
            {
                model = Microsoft.OData.Edm.Library.EdmCoreModel.Instance;
            }

            // Let ExpressionLexer try to get a primitive
            ExpressionLexer lexer = new ExpressionLexer(value, false /*moveToFirstToken*/, false /*useSemicolonDelimeter*/);
            Exception error;
            ExpressionToken token;

            lexer.TryPeekNextToken(out token, out error);

            if (token.Kind == ExpressionTokenKind.BracketedExpression)
            {
                // Should be a complex or collection value
                return ODataUriConversionUtils.ConvertFromComplexOrCollectionValue(value, version, model, typeReference);
            }

            QueryNode enumConstNode;
            if ((token.Kind == ExpressionTokenKind.Identifier)  // then try parsing the entire text as enum value
                && EnumBinder.TryBindIdentifier(lexer.ExpressionText, model, out enumConstNode))
            {
                return ((ConstantNode)enumConstNode).Value;
            }

            object result = lexer.ReadLiteralToken();

            // If we have a typeReference then perform verification and convert if necessary
            if (typeReference != null)
            {
                result = ODataUriConversionUtils.VerifyAndCoerceUriPrimitiveLiteral(result, model, typeReference, version);
            }

            return result;
        }

        /// <summary>
        /// Converts the given object to a string for use in a Uri. Does not perform any of the escaping that <see cref="System.Uri"/> provides.
        /// No type verification is used.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <param name="version">Version to be compliant with.</param>
        /// <returns>A string representation of <paramref name="value"/> for use in a Url.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "designed to aid the creation on a URI, not create a full one")]
        public static string ConvertToUriLiteral(object value, ODataVersion version)
        {
            return ODataUriUtils.ConvertToUriLiteral(value, version, null);
        }

        /// <summary>
        /// Converts the given object to a string in the specified format for use in a Uri. Does not perform any of the escaping that <see cref="System.Uri"/> provides.
        /// Will perform type verification based on the given model if possible.
        /// </summary>
        /// <param name="value">Value to be converted (can be EnumNode).</param>
        /// <param name="version">Version to be compliant with.</param>
        /// <param name="model">Optional model to perform verification against.</param>
        /// <returns>A string representation of <paramref name="value"/> for use in a Url.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "designed to aid the creation on a URI, not create a full one")]
        public static string ConvertToUriLiteral(object value, ODataVersion version, IEdmModel model)
        {
            if (value == null)
            {
                value = new ODataNullValue();
            }

            if (model == null)
            {
                model = Microsoft.OData.Edm.Library.EdmCoreModel.Instance;
            }

            ODataNullValue nullValue = value as ODataNullValue;
            if (nullValue != null)
            {
                return ExpressionConstants.KeywordNull;
            }

            ODataCollectionValue collectionValue = value as ODataCollectionValue;
            if (collectionValue != null)
            {
                return ODataUriConversionUtils.ConvertToUriCollectionLiteral(collectionValue, model, version);
            }

            ODataComplexValue complexValue = value as ODataComplexValue;
            if (complexValue != null)
            {
                return ODataUriConversionUtils.ConvertToUriComplexLiteral(complexValue, model, version);
            }

            ODataEnumValue enumValue = value as ODataEnumValue;
            if (enumValue != null)
            {
                return ODataUriConversionUtils.ConvertToUriEnumLiteral(enumValue, version);
            }

            return ODataUriConversionUtils.ConvertToUriPrimitiveLiteral(value, version);
        }
    }
}
