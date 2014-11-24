using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.OData.OData.Query.Aggregation.AggregationMethods;
using System.Web.OData.OData.Query.Aggregation.QueryableImplementation;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using Microsoft.OData.Core.Aggregation;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation
{
    /// <summary>
    /// Implementation of the group-by aggregation transformation.
    /// </summary>
    public class GroupByImplementation : ApplyImplementationBase
    {
        /// <summary>
        ///  Gets or sets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; set; }

        /// <summary>
        /// Execute group-by without aggregation on the results.
        /// </summary>
        /// <param name="query">The collection to group.</param>
        /// <param name="maxResults">The max number of elements in a result set.</param>
        /// <param name="transformation">The group-by transformation as <see cref="ApplyGroupbyClause"/>.</param>
        /// <param name="keyType">The key type to group by.</param>
        /// <param name="propertiesToGroupByExpressions">Lambda expression that represents access to the properties to group by.</param>
        /// <returns>The results of the group by transformation as <see cref="IQueryable"/>.</returns>
        public IQueryable DoGroupBy(IQueryable query, int maxResults, ApplyGroupbyClause transformation, Type keyType, IEnumerable<LambdaExpression> propertiesToGroupByExpressions)
        {
            var propToGroupBy = (propertiesToGroupByExpressions != null)
                ? propertiesToGroupByExpressions.ToArray()
                : null;
            var keySelector = this.GetGroupByProjectionLambda(transformation.SelectedStatements.ToArray(), keyType, propToGroupBy);
            object comparer = keyType.GetProperty("ComparerInstance").GetValue(null);
            var results = ExpressionHelpers.GroupBy(query, keySelector, this.Context.ElementClrType, keyType, comparer);

            var keys = this.GetGroupingKeys(results, keyType, this.Context.ElementClrType);

            // if group by is not supported in this IQueriable provider convert the grouping into memory implementation
            object convertedResult = null;
            if (QueriableProviderAdapter.ConvertionIsRequiredAsExpressionIfNotSupported(keys, maxResults, out convertedResult))
            {
                keys = convertedResult as IQueryable;
            }
            
            var keysToReturn = ExpressionHelpers.Distinct(keyType, keys);
            return keysToReturn;
        }

        /// <summary>
        /// Execute group-by with aggregation on the results.
        /// </summary>
        /// <param name="query">The collection to group.</param>
        /// <param name="maxResults">The max number of elements in a result set.</param>
        /// <param name="transformation">The group-by transformation as <see cref="ApplyGroupbyClause"/>.</param>
        /// <param name="keyType">The key type to group by.</param>
        /// <param name="propertiesToGroupByExpressions">Lambda expression that represents access to the properties to group by.</param>
        /// <param name="propertyToAggregateExpression">Lambda expression that represents access to the property to aggregate.</param>
        /// <param name="keys">Output the collection keys of the grouped results.</param>
        /// <param name="aggragatedValues">Output the aggregated results.</param>
        public void DoAggregatedGroupBy(
            IQueryable query, int maxResults, ApplyGroupbyClause transformation,
            Type keyType, IEnumerable<LambdaExpression> propertiesToGroupByExpressions, LambdaExpression propertyToAggregateExpression, out IQueryable keys, out object[] aggragatedValues)
        {
            var propToGroupBy = (propertiesToGroupByExpressions != null)
               ? propertiesToGroupByExpressions.ToArray()
               : null;
            var keySelector = this.GetGroupByProjectionLambda(transformation.SelectedStatements.ToArray(), keyType, propToGroupBy);
            object comparer = keyType.GetProperty("ComparerInstance").GetValue(null);
            var groupingResults = ExpressionHelpers.GroupBy(query, keySelector, this.Context.ElementClrType, keyType, comparer);
            var aggregationImplementation =
                AggregationMethodsImplementations.GetAggregationImplementation(transformation.Aggregate.AggregationMethod);

            // if group by is not supported in this IQueriable provider convert the grouping into memory implementation
            object convertedResult = null;
            if (QueriableProviderAdapter.ConvertionIsRequiredAsExpressionIfNotSupported(groupingResults, maxResults, out convertedResult))
            {
                groupingResults = convertedResult as IQueryable;
            }

            var resType = typeof(List<>).MakeGenericType(this.Context.ElementClrType);
            keys = this.GetGroupingKeys(groupingResults, keyType, this.Context.ElementClrType);
            var groupedValues = this.GetGroupingValues(groupingResults, keyType, resType, this.Context.ElementClrType);

            // In case of paging due to memory execution of unsupported functions keys may not be distinct. 
            // Here we make sure that keys are distinct and all values that belong to a key are written to the right list.  
            List<object> distictKeys;
            List<object> groupedValuesPerKey;
            this.CombineValuesListsPerKey(keys.AllElements(), groupedValues.AllElements(), out distictKeys, out groupedValuesPerKey);
            keys = distictKeys.AsQueryable();

            var results = new List<object>();
            foreach (var values in groupedValuesPerKey)
            {
                IQueryable valuesAsQueryable;
                if (values is IEnumerable<object>)
                {
                    valuesAsQueryable = ExpressionHelpers.Cast(this.Context.ElementClrType, (values as IEnumerable<Object>).AsQueryable());
                }
                else
                {
                    valuesAsQueryable = ExpressionHelpers.Cast(this.Context.ElementClrType, (new List<Object>() { values }).AsQueryable());
                }
                
                IQueryable queryToUse = valuesAsQueryable;
                if (transformation.Aggregate.AggregatableProperty.Contains('/'))
                {
                    queryToUse = AggregationImplementationBase.FilterNullValues(query, this.Context.ElementClrType, transformation.Aggregate);
                }
                var projectionLambda = AggregationImplementationBase.GetProjectionLambda(this.Context.ElementClrType, transformation.Aggregate, propertyToAggregateExpression);
                string[] aggregationParams = AggregationImplementationBase.GetAggregationParams(transformation.Aggregate.AggregationMethod);
                var aggragationResult = aggregationImplementation.DoAggregatinon(this.Context.ElementClrType, queryToUse, transformation.Aggregate, projectionLambda, aggregationParams);
                results.Add(aggragationResult);
            }

            aggragatedValues = results.ToArray();
        }

        /// <summary>
        /// Parse the "with {aggregationName} as {alias}" statements. 
        /// </summary>
        /// <param name="statement">Statement to parse.</param>
        /// <param name="samplingMethod">SamplingMethod parsed from the statement.</param>
        /// <param name="alias">Alias parsed from the statement.</param>
        /// <param name="samplingProperty">The sampling property.</param>
        internal static void GetSamplingMethod(string statement, out string samplingMethod, out string alias, out string samplingProperty)
        {
            alias = null;
            samplingMethod = null;
            samplingProperty = null;
            string[] verbs = statement.Split(' ');
            int withIndex = verbs.Find("with");
            if (withIndex != -1)
            {
                alias = verbs.Last();
                samplingMethod = verbs[withIndex + 1];
                samplingProperty = verbs[0];
            }
        }

        /// <summary>
        /// Get the keys from the grouping results (i.e. results.select(res=>res.Key)).
        /// </summary>
        /// <param name="dataToProject">The grouping result.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="elementType">The entity type.</param>
        /// <returns>Collection of keys.</returns>
        internal IQueryable GetGroupingKeys(IQueryable dataToProject, Type keyType, Type elementType)
        {
            var groupedItemType = typeof(IGrouping<,>).MakeGenericType(keyType, elementType);
            var selector = ApplyImplementationBase.GetProjectionLambda(groupedItemType, "Key");
            return ExpressionHelpers.Select(groupedItemType, keyType, dataToProject, selector);
        }

        /// <summary>
        /// Get the values from the grouping results (i.e. results.select(res=>res.ToList())).
        /// </summary>
        /// <param name="dataToProject">The grouping results.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="resType">The result type.</param>
        /// <param name="elementType">The entity type.</param>
        /// <returns>Collection of results.</returns>
        internal IQueryable GetGroupingValues(IQueryable dataToProject, Type keyType, Type resType, Type elementType)
        {
            var toListMethodInfo = ExpressionHelperMethods.EnumerabltToListGeneric.MakeGenericMethod(elementType);
            var groupedItemType = typeof(IGrouping<,>).MakeGenericType(keyType, elementType);
            var selector = ApplyImplementationBase.GetMethodCallLambda(groupedItemType, toListMethodInfo);
            return ExpressionHelpers.Select(groupedItemType, resType, dataToProject, selector);
        }

        /// <summary>
        /// Helper method to create a new dynamic type for the group-by key.
        /// </summary>
        /// <param name="transformation">The group-by query.</param>
        /// <returns>The type of the key which was dynamically generated.</returns>
        internal Type GetGroupByKeyType(ApplyGroupbyClause transformation)
        {
            Contract.Assert(transformation != null);
            Contract.Assert(transformation.SelectedStatements != null);
            Contract.Assert(transformation.SelectedStatements.Any());

            var keyProperties = new List<Tuple<Type, string>>();
            var selectedStatementsDictionary = GetSelectedStatementsDictionary(transformation.SelectedStatements);

            foreach (var statement in selectedStatementsDictionary)
            {
                // simple property
                var statementString = statement.Value.First();
                if ((statement.Value.Count() == 1) && (statementString == statement.Key))
                {
                    string samplingMethod, alias, samplingProperty;
                    GroupByImplementation.GetSamplingMethod(statementString, out samplingMethod, out alias, out samplingProperty);
                    if (samplingMethod != null)
                    {
                        var pi = this.Context.ElementClrType.GetProperty(samplingProperty);
                        if (pi == null)
                        {
                            throw new ArgumentException(string.Format("Entity does not contain {0}", samplingProperty));
                        }
                        var implementation = SamplingMethodsImplementations.GetAggregationImplementation(samplingMethod);
                        var samplingType = implementation.GetResultType(pi.PropertyType);
                        keyProperties.Add(new Tuple<Type, string>(samplingType, alias));
                    }
                    else
                    {
                        var propName = statementString.TrimMethodCall().Split(' ').First();
                        var pi = this.Context.ElementClrType.GetProperty(propName);
                        if (pi == null)
                        {
                            throw new ArgumentException(string.Format("Entity does not contain {0}", propName));
                        }
                        keyProperties.Add(new Tuple<Type, string>(pi.PropertyType, pi.Name));
                    }
                }
                else
                {
                    // complex property
                    var propName = statement.Key.TrimMethodCall();
                    var pi = this.Context.ElementClrType.GetProperty(propName);
                    if (pi == null)
                    {
                            throw new ArgumentException(string.Format("Entity does not contain {0}", propName));
                    }
                    var newPropertyType = this.GenerateComplexType(pi.PropertyType, statement.Value);
                    keyProperties.Add(new Tuple<Type, string>(newPropertyType, propName));
                }
            }

            return AggregationTypesGenerator.CreateType(keyProperties.Distinct(new TypeStringTupleComapere()).ToList(), Context, true);
        }

        /// <summary>
        /// Take a list of keys and a matching list of values lists. Create a distinct list of keys and matching list of values. 
        /// If the input contains two values list for the same key they should concatenated.
        /// </summary>
        /// <param name="keys">original list of keys.</param>
        /// <param name="values">original list of values lists.</param>
        /// <param name="resKeys">list of distinct keys.</param>
        /// <param name="resValues">matching list of values maid up from the original list of values lists.</param>
        private void CombineValuesListsPerKey(List<object> keys, List<object> values, out List<object> resKeys, out List<object> resValues)
        {
            resValues = new List<object>();
            resKeys = new List<object>();
            List<int> visited = new List<int>();

            for (int i = 0; i < keys.Count; i++)
            {
                if (visited.Contains(i))
                {
                    continue;
                }

                resValues.Add(values[i]);
                resKeys.Add(keys[i]);
                for (int j = i; j < keys.Count; j++)
                {
                    var nextEqualKey = keys.FindIndex(j + 1 , x => x.Equals(keys[i]));
                    if (nextEqualKey > j)
                    {
                        if (values[nextEqualKey] is IEnumerable<object>)
                        {
                            var index = resValues.Count - 1;
                            var lst = (resValues.Last() as IEnumerable<object>).ToList();
                            lst.AddRange(values[nextEqualKey] as IEnumerable<object>);
                            resValues.RemoveAt(index);
                            resValues.Insert(index, lst);
                        }
                        else
                        {
                            resValues.Add(values[nextEqualKey]);
                        }
                        visited.Add(nextEqualKey);
                        j = nextEqualKey - 1;
                    }
                }
            }
        }


        /// <summary>
        /// Continue the recursive operation of creating the Group-By key.
        /// </summary>
        /// <param name="declaringType">The type based on which we are going to create the new type.</param>
        /// <param name="segments">The select segments that declare what to create.</param>
        /// <returns>A new type.</returns>
        private Type GenerateComplexType(Type declaringType, IEnumerable<string> segments)
        {
            Contract.Assert(declaringType != null);
            Contract.Assert(segments != null);

            var keyProperties = new List<Tuple<Type, string>>();
            var selectedStatementsDictionary = GetSelectedStatementsDictionary(segments);
            foreach (var statement in selectedStatementsDictionary)
            {
                // simple property
                var statementString = statement.Value.First();
                if ((statement.Value.Count() == 1) && (statementString == statement.Key))
                {
                    string samplingMethod, alias, samplingProperty;
                    GroupByImplementation.GetSamplingMethod(statementString.TrimMethodCallSufix(), out samplingMethod, out alias, out samplingProperty);
                    if (samplingMethod != null)
                    {
                        var pi = declaringType.GetProperty(samplingProperty);
                        var implementation = SamplingMethodsImplementations.GetAggregationImplementation(samplingMethod);
                        var samplingType = implementation.GetResultType(pi.PropertyType);
                        keyProperties.Add(new Tuple<Type, string>(samplingType, alias));
                    }
                    else
                    {
                        statementString = statementString.Split(' ').First().TrimMethodCallSufix();
                        var pi = declaringType.GetProperty(statementString);
                        keyProperties.Add(new Tuple<Type, string>(pi.PropertyType, pi.Name));
                    }
                }
                else
                {
                    // complex property
                    var key = statement.Key.Split(' ').First().TrimMethodCallSufix();
                    var pi = declaringType.GetProperty(key);
                    var newPropertyType = GenerateComplexType(pi.PropertyType, statement.Value);
                    keyProperties.Add(new Tuple<Type, string>(newPropertyType, key));
                }
            }
            return AggregationTypesGenerator.CreateType(keyProperties.Distinct(new TypeStringTupleComapere()).ToList(), Context, false);
        }

        /// <summary>
        /// Create a dictionary of all the selected segments based on their roots. 
        /// For example the list "Amount, Customer/Name, Customer/Address/Street" will create the following dictionary:
        /// (Amount, {Amount}), (Customer, {Name, Address/Street}). 
        /// </summary>
        /// <param name="selectedStatement">The statements.</param>
        /// <returns>A dictionary of all the selected segments based on their roots</returns>
        private static Dictionary<string, IEnumerable<string>> GetSelectedStatementsDictionary(IEnumerable<string> selectedStatement)
        {
            var selectedStatementsDictionary = new Dictionary<string, IEnumerable<string>>();
            var grouping = selectedStatement.GroupBy(
                str =>
                {
                    if (!str.Contains('/'))
                    {
                        return str;
                    }
                    else
                    {
                        return str.Substring(0, str.IndexOf('/'));
                    }
                },

                value =>
                {
                    if (!value.Contains("/"))
                    {
                        return value;
                    }
                    else
                    {
                        return value.Substring(value.IndexOf('/') + 1);
                    }
                });

            foreach (var g in grouping)
            {
                selectedStatementsDictionary.Add(g.Key, g.ToList());
            }

            return selectedStatementsDictionary;
        }



        /// <summary>
        /// Create a <see cref="LambdaExpression"/> such as: Expression{Func{Sales, keyType}} projectionLambda = s => new KeyType(Amount = s.Amount, Id=s.Id).
        /// </summary>
        /// <param name="selectStatements">The selected statements creating the key of the group-by operation.</param>
        /// <param name="keyType">The type of the key of the group-by operation.</param>
        /// <param name="propertiesToGroupByExpressions">List of expressions to the properties to group by.</param>
        /// <returns><see cref="LambdaExpression"/>.</returns>
        private LambdaExpression GetGroupByProjectionLambda(string[] selectStatements, Type keyType, LambdaExpression[] propertiesToGroupByExpressions)
        {
            Contract.Assert(keyType != null);
            Contract.Assert(selectStatements != null && selectStatements.Any());

            ParameterExpression entityParam;
            if (propertiesToGroupByExpressions != null && propertiesToGroupByExpressions.Any())
            {
                entityParam = propertiesToGroupByExpressions.First().Parameters.First();
            }
            else
            {
                entityParam = Expression.Parameter(this.Context.ElementClrType, "e");
            }

            var bindings = new List<MemberAssignment>();

            for (int i = 0; i < selectStatements.Length; i++)
            {
                var statement = selectStatements[i];
                Expression selectedProperyExpression = null;
                if (propertiesToGroupByExpressions != null)
                {
                    selectedProperyExpression = propertiesToGroupByExpressions[i].Body;
                }

                string samplingMethod, alias, samplingProperty;
                GroupByImplementation.GetSamplingMethod(statement, out samplingMethod, out alias, out samplingProperty);
                if (samplingMethod == null)
                {
                    var prop = statement;
                    if (prop.Contains('/'))
                    {
                        prop = statement.Substring(0, prop.IndexOf('/'));
                    }

                    prop = prop.TrimMethodCall().Split(' ').First();
                    var mi = keyType.GetMember(prop).First();
                    bindings.Add(Expression.Bind(mi, ApplyImplementationBase.GetPropertyExpression(keyType, statement, entityParam, selectedProperyExpression)));
                }
                else
                {
                    string prop = alias;
                    if (statement.Contains('/'))
                    {
                        prop = statement.Substring(0, statement.IndexOf('/'));
                    }

                    var mi = keyType.GetMember(prop).First();
                    var propertyType = GetPropertyInfo(this.Context.ElementClrType, statement).Last().PropertyType;

                    var implementation = SamplingMethodsImplementations.GetAggregationImplementation(samplingMethod);
                    MethodInfo method = implementation.GetSamplingProcessingMethod(propertyType);

                    var aggregationParamsExpressions = GetAggregationArgumentsExpressions(samplingMethod, method);

                    bindings.Add(Expression.Bind(mi, ApplyImplementationBase.GetComputedPropertyExpression(keyType, statement, entityParam, method, selectedProperyExpression, aggregationParamsExpressions)));
                }
            }

            var body = Expression.MemberInit(Expression.New(keyType), bindings);
            return Expression.Lambda(body, entityParam);
        }

        /// <summary>
        /// Get the list of parameter expression to be binded to the method call, 
        /// If the number of arguments provided by the query does not match the method signature this method will ignore non required arguments or create arguments with default values.
        /// </summary>
        /// <param name="samplingMethod">The sampling method string that contain the arguments</param>
        /// <param name="method">The <see cref="MethodInfo"/> of the sampling method to call</param>
        /// <returns>Array of expressions</returns>
        private static Expression[] GetAggregationArgumentsExpressions(string samplingMethod, MethodInfo method)
        {
            Expression[] aggregationParamsExpressions = null;
            string[] aggregationParamsAsStrings = AggregationImplementationBase.GetAggregationParams(samplingMethod);
            var expcetedParameters = method.GetParameters();

            if (aggregationParamsAsStrings != null && aggregationParamsAsStrings.Any())
            {
                aggregationParamsExpressions = ParseAggregationParams(aggregationParamsAsStrings, expcetedParameters);
                if (expcetedParameters.Length != aggregationParamsAsStrings.Length + 1)
                {
                    if (aggregationParamsAsStrings.Length > expcetedParameters.Length - 1)
                    {
                        var tmp = new Expression[expcetedParameters.Length - 1];
                        Array.Copy(aggregationParamsExpressions, tmp, expcetedParameters.Length - 1);
                        aggregationParamsExpressions = tmp;
                    }
                    else
                    {
                        var tmp = new List<Expression>();
                        tmp.AddRange(aggregationParamsExpressions);
                        GetDefaultArgumentsExpressions(aggregationParamsExpressions.Length, expcetedParameters.Length - 1, expcetedParameters, tmp);
                        aggregationParamsExpressions = tmp.ToArray();
                    }
                }
            }
            else
            {
                var tmp = new List<Expression>();
                GetDefaultArgumentsExpressions(0, expcetedParameters.Length - 1, expcetedParameters, tmp);
                aggregationParamsExpressions = tmp.ToArray();
            }
            return aggregationParamsExpressions;
        }

        private static Expression[] ParseAggregationParams(string[] aggregationParamsAsStrings,
            ParameterInfo[] expcetedParameters)
        {
            Expression[] aggregationParamsExpressions;
            var argumentList = new List<Expression>();
            for (int i = 0; i < aggregationParamsAsStrings.Length; i++)
            {
                var aggregationParamAsString = aggregationParamsAsStrings[i];
                if (expcetedParameters.Length >= i)
                {
                    var expectedParamater = expcetedParameters[i + 1];
                    if (expectedParamater.ParameterType == typeof(string))
                    {
                        argumentList.Add(Expression.Constant(aggregationParamAsString));
                    }
                    else if (expectedParamater.ParameterType == typeof(bool))
                    {
                        argumentList.Add(Expression.Constant(bool.Parse(aggregationParamAsString.ToLower())));
                    }
                    else if (expectedParamater.ParameterType == typeof(int))
                    {
                        argumentList.Add(Expression.Constant(int.Parse(aggregationParamAsString)));
                    }
                    else if (expectedParamater.ParameterType == typeof(long))
                    {
                        argumentList.Add(Expression.Constant(long.Parse(aggregationParamAsString)));
                    }
                    else if (expectedParamater.ParameterType == typeof(double))
                    {
                        argumentList.Add(Expression.Constant(double.Parse(aggregationParamAsString)));
                    }
                    else if (expectedParamater.ParameterType == typeof(byte))
                    {
                        argumentList.Add(Expression.Constant(byte.Parse(aggregationParamAsString)));
                    }
                    else if (expectedParamater.ParameterType == typeof(short))
                    {
                        argumentList.Add(Expression.Constant(short.Parse(aggregationParamAsString)));
                    }
                    else if (expectedParamater.ParameterType == typeof(char))
                    {
                        argumentList.Add(Expression.Constant(char.Parse(aggregationParamAsString)));
                    }
                    else if (expectedParamater.ParameterType == typeof(float))
                    {
                        argumentList.Add(Expression.Constant(float.Parse(aggregationParamAsString)));
                    }
                    else if (expectedParamater.ParameterType == typeof(DateTimeOffset))
                    {
                        argumentList.Add(Expression.Constant(DateTimeOffset.Parse(aggregationParamAsString)));
                    }
                }
            }
            aggregationParamsExpressions = argumentList.ToArray();
            return aggregationParamsExpressions;
        }

        private static void GetDefaultArgumentsExpressions(int start, int stop, ParameterInfo[] expcetedParameters, List<Expression> result)
        {
            for (int j = start; j < stop; j++)
            {
                object defaultValue = null;
                Type expectedType = expcetedParameters[j + 1].ParameterType;
                if (expcetedParameters[j + 1].IsOptional)
                {
                    defaultValue = expcetedParameters[j + 1].DefaultValue;
                }
                else if (expectedType == typeof(string))
                {
                    defaultValue = string.Empty;
                }
                else if (expectedType == typeof(bool))
                {
                    defaultValue = false;
                }
                else if (expectedType.IsEnum)
                {
                    defaultValue = Enum.GetValues(expectedType).GetValue(0);
                }
                else if (!expectedType.IsClass)
                {
                    defaultValue = 0;
                }

                result.Add(Expression.Constant(defaultValue));
            }
        }
    }
}
