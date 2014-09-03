using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.OData.Query.Aggregation.AggregationMethods;
using System.Web.OData.OData.Query.Aggregation.QueryableImplementation;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using Microsoft.OData.Core.Aggregation;
using Microsoft.OData.Core.UriParser.Semantic;


namespace System.Web.OData.OData.Query.Aggregation
{

    public class GroupByImplementation : ApplyImplementationBase
    {
        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; set; }

        /// <summary>
        /// Execute group-by without aggregation on the results
        /// </summary>
        /// <param name="query">The collection to group</param>
        /// <param name="transformation">the group-by transformation as <see cref="ApplyGroupbyClause"/></param>
        /// <param name="keyType">the key type to group by</param>
        /// <param name="propertiesToGroupByExpressions">Lambda expression that represents access to the properties to group by</param>
        /// <returns>The results of the group by transformation as <see cref="IQueryable"/></returns>
        public IQueryable DoGroupBy(IQueryable query, ApplyGroupbyClause transformation, Type keyType, IEnumerable<LambdaExpression> propertiesToGroupByExpressions)
        {
            var keySelector = this.GetGroupByProjectionLambda(transformation.SelectedStatements.ToArray(), keyType, propertiesToGroupByExpressions.ToArray());
            object comparer = keyType.GetProperty("ComparerInstance").GetValue(null);
            var results = ExpressionHelpers.GroupBy(query, keySelector, this.Context.ElementClrType, keyType, comparer);

            return this.GetGroupingKeys(results, keyType, this.Context.ElementClrType);
        }

        /// <summary>
        /// Execute group-by with aggregation on the results
        /// </summary>
        /// <param name="query">The collection to group</param>
        /// <param name="maxResults">The max number of elements in a result set</param>
        /// <param name="transformation">the group-by transformation as <see cref="ApplyGroupbyClause"/></param>
        /// <param name="keyType">the key type to group by</param>
        /// <param name="propertiesToGroupByExpressions">Lambda expression that represents access to the properties to group by</param>
        /// <param name="propertyToAggregateExpression">Lambda expression that represents access to the property to aggregate</param>
        /// <param name="keys">output the collection keys of the grouped results</param>
        /// <param name="aggragatedValues">output the aggregated results</param>
        public void DoAggregatedGroupBy(IQueryable query, int maxResults, ApplyGroupbyClause transformation, 
            Type keyType, IEnumerable<LambdaExpression> propertiesToGroupByExpressions, LambdaExpression propertyToAggregateExpression, out IQueryable keys, out object[] aggragatedValues)
        {
            var keySelector = this.GetGroupByProjectionLambda(transformation.SelectedStatements.ToArray(), keyType, propertiesToGroupByExpressions.ToArray());
            object comparer = keyType.GetProperty("ComparerInstance").GetValue(null);
            var groupingResults = ExpressionHelpers.GroupBy(query, keySelector, this.Context.ElementClrType, keyType, comparer);
            var aggregationImplementation =
                AggregationMethodsImplementations.GetAggregationImplementation(transformation.Aggregate.AggregationMethod);
            (query.Provider as AggregationQueryProvider).Combiner = aggregationImplementation.CombineTemporaryResults;

            ///if group by is not supported in this IQueriable provider convert the grouping into memory implementation
            object convertedResult = null;
            if (QueriableProviderAdapter.ConvertionIsRequiredAsExpressionIfNotSupported(groupingResults, maxResults, out convertedResult))
            {
                groupingResults = convertedResult as IQueryable;
            }

            var resType = typeof(List<>).MakeGenericType(this.Context.ElementClrType);
            keys = this.GetGroupingKeys(groupingResults, keyType, this.Context.ElementClrType);
            var groupedValues = this.GetGroupingValues(groupingResults, keyType, resType, this.Context.ElementClrType);

            var results = new List<object>();
            foreach (var values in groupedValues)
            {
                var valuesAsQueryable = ExpressionHelpers.AsQueryable(this.Context.ElementClrType, values as IEnumerable);
                var aggragationResult = aggregationImplementation.DoAggregatinon(this.Context.ElementClrType,
                    valuesAsQueryable as IQueryable, transformation.Aggregate, propertyToAggregateExpression);
                results.Add(aggragationResult);
            }

            aggragatedValues = results.ToArray();
        }


        /// <summary>
        /// Parse the "with {aggregationName} as {alias}" statements. 
        /// </summary>
        /// <param name="statement">statement to parse</param>
        /// <param name="samplingMethod">samplingMethod parsed from the statement</param>
        /// <param name="alias">alias parsed from the statement</param>
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
        /// Get the keys from the grouping results (i.e. results.select(res=>res.Key))
        /// </summary>
        /// <param name="dataToProject">The grouping result</param>
        /// <param name="keyType">The key type</param>
        /// <param name="elementType">the entity type</param>
        /// <returns>collection of keys</returns>
        internal IQueryable GetGroupingKeys(IQueryable dataToProject, Type keyType, Type elementType)
        {
            var groupedItemType = typeof(IGrouping<,>).MakeGenericType(keyType, elementType);
            var selector = ApplyImplementationBase.GetProjectionLambda(groupedItemType, "Key");
            return ExpressionHelpers.Select(groupedItemType, keyType, dataToProject, selector) as IQueryable;
        }

        /// <summary>
        /// Get the values from the grouping results (i.e. results.select(res=>res.ToList()))
        /// </summary>
        /// <param name="dataToProject">The grouping results</param>
        /// <param name="keyType">The key type</param>
        /// <param name="resType">the result type</param>
        /// <param name="elementType">the entity type</param>
        /// <returns>collection of results</returns>
        internal IEnumerable GetGroupingValues(IQueryable dataToProject, Type keyType, Type resType, Type elementType)
        {
            var toListMethodInfo = ExpressionHelperMethods.EnumerabltToListGeneric.MakeGenericMethod(elementType);
            var groupedItemType = typeof(IGrouping<,>).MakeGenericType(keyType, elementType);
            var selector = ApplyImplementationBase.GetMethodCallLambda(groupedItemType, toListMethodInfo);
            return ExpressionHelpers.Select(groupedItemType, resType, dataToProject, selector);
        }

        /// <summary>
        /// Helper method to create a new dynamic type for the group-by key
        /// </summary>
        /// <param name="transformation"></param>
        /// <returns></returns>
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
                var statementString = statement.Value.First().TrimMethodCallSufix();
                if ((statement.Value.Count() == 1) && (statementString == statement.Key))
                {
                    string samplingMethod, alias, samplingProperty;
                    GroupByImplementation.GetSamplingMethod(statementString, out samplingMethod, out alias, out samplingProperty);
                    if (samplingMethod != null)
                    {
                        var pi = Context.ElementClrType.GetProperty(samplingProperty);
                        var implementation = SamplingMethodsImplementations.GetAggregationImplementation(samplingMethod);
                        var samplingType = implementation.GetResultType(pi.PropertyType);
                        keyProperties.Add(new Tuple<Type, string>(samplingType, alias));
                    }
                    else
                    {
                        var pi = Context.ElementClrType.GetProperty(statementString.TrimMethodCallPrefix().Split(' ').First());
                        keyProperties.Add(new Tuple<Type, string>(pi.PropertyType, pi.Name));
                    }
                }
                else // complex property
                {
                    var propName = statement.Key.TrimMethodCallPrefix();
                    var pi = Context.ElementClrType.GetProperty(propName);
                    var newPropertyType = GenerateComplexType(pi.PropertyType, statement.Value);
                    keyProperties.Add(new Tuple<Type, string>(newPropertyType, propName));
                }
            }

            return AggregationTypesGenerator.CreateType(keyProperties.Distinct(new TypeStringTupleComapere()).ToList(), Context, true);
        }


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
                else //complex property
                {
                    var key = statement.Key.Split(' ').First().TrimMethodCallSufix();
                    var pi = declaringType.GetProperty(key);
                    var newPropertyType = GenerateComplexType(pi.PropertyType, statement.Value);
                    keyProperties.Add(new Tuple<Type, string>(newPropertyType, key));
                }
            }
            return AggregationTypesGenerator.CreateType(keyProperties.Distinct(new TypeStringTupleComapere()).ToList(), Context, false);
        }


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
                }
            );
            foreach (var g in grouping)
            {
                selectedStatementsDictionary.Add(g.Key, g.ToList());
            }
            return selectedStatementsDictionary;
        }



       /// <summary>
       /// Create a <see cref="LambdaExpression"/> such as: Expression<Func<Sales, keyType>> projectionLambda = s => new KeyType(Amount = s.Amount, Id=s.Id)
       /// </summary>
       /// <param name="selectStatements">The selected statements creating the key of the group-by operation</param>
       /// <param name="keyType">The type of the key of the group-by operation</param>
       /// <returns><see cref="LambdaExpression"/></returns>
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
                    prop = prop.TrimMethodCallPrefix().Split(' ').First();
                    var mi = keyType.GetMember(prop).First();
                    bindings.Add(Expression.Bind(mi,
                        ApplyImplementationBase.GetPropertyExpression(keyType, statement, entityParam, selectedProperyExpression)));
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

                    bindings.Add(Expression.Bind(mi,
                        ApplyImplementationBase.GetComputedPropertyExpression(keyType, statement, entityParam, method, selectedProperyExpression)));
                }
            }

            var body = Expression.MemberInit(Expression.New(keyType), bindings);
            return Expression.Lambda(body, entityParam);
        }
    }
}
