using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Formatter;
using System.Web.OData.OData.Query.Aggregation;
using System.Web.OData.OData.Query.Aggregation.AggregationMethods;
using System.Web.OData.OData.Query.Aggregation.QueryableImplementation;
using System.Web.OData.Properties;
using System.Web.OData.Query;
using System.Web.OData.Query.Expressions;
using System.Web.OData.Query.Validators;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using EdmTypeKind = Microsoft.Data.Edm.EdmTypeKind;


namespace System.Web.OData.OData.Query
{
    /// <summary>
    /// OData query option class that implements aggregation
    /// </summary>
    public class ApplyQueryOption
    {
        private ODataQueryOptionParser _queryOptionParser;
        private ApplyClause _applyClause;
        private static MethodInfo _Intercept_mi;

        /// <summary>
        /// Create a new instance of ApplyQueryOption
        /// </summary>
        /// <param name="rawValue"></param>
        /// <param name="context"></param>
        /// <param name="queryOptionParser"></param>
        public ApplyQueryOption(ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull("queryOptionParser");
            }

            Context = context;
            _queryOptionParser = queryOptionParser;
        }

        static ApplyQueryOption()
        {
            _Intercept_mi = typeof(InterceptingProvider)
               .GetMethods()
               .FirstOrDefault(mi => mi.IsGenericMethod && mi.Name == "Intercept");
        }



        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the parsed <see cref="FilterClause"/> for this query option.
        /// </summary>
        public ApplyClause ApplyClause
        {
            get { return _applyClause ?? (_applyClause = _queryOptionParser.ParseApply()); }
        }
        
        /// <summary>
        /// execute the apply query to the given IQueryable.
        /// </summary>
        /// <remarks>
        /// The <see cref="ODataQuerySettings.HandleNullPropagation"/> property specifies
        /// how this method should handle null propagation.
        /// </remarks>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new <see cref="IQueryable"/> after the apply query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, IAssembliesResolver assembliesResolver)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            if (querySettings == null)
            {
                throw Error.ArgumentNull("querySettings");
            }
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            ApplyClause applyClause = ApplyClause;
            Contract.Assert(applyClause != null);

            // Ensure we have decided how to handle null propagation
            ODataQuerySettings updatedSettings = querySettings;
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings = new ODataQuerySettings(updatedSettings);
                updatedSettings.HandleNullPropagation = HandleNullPropagationOption.False;
            }

            var maxResults = querySettings.PageSize ?? 2000;

            // Call InterceptingProvider.Intercept that will create a new <see cref="InterceptingProvider"/> and set its visitors. 
            // InterceptingProvider will wrap the actual IQueryable to implement unsupported operations in memory.
            var mi = _Intercept_mi.MakeGenericMethod(Context.ElementClrType);
            IQueryable results = mi.Invoke(null, new Object[] { query, maxResults, null }) as IQueryable;

            //Each transformation is the input for the next transformation in the transformations list 
            foreach (var transformation in applyClause.Transformations)
            {
                switch (transformation.Item1)
                {
                    case "aggregate":
                        ApplyAggregateClause aggregateClause = transformation.Item2 as ApplyAggregateClause;
                        if (aggregateClause == null)
                        {
                            throw Error.Argument("aggregation transformation type mismatch", transformation.Item2);
                        }

                        LambdaExpression propertyToAggregateExpression = FilterBinder.Bind(aggregateClause.AggregatablePropertyExpression, Context.ElementClrType, Context.Model, assembliesResolver, updatedSettings);
                        
                        var aggregationImplementation = AggregationMethodsImplementations.GetAggregationImplementation(aggregateClause.AggregationMethod);
                        if (results.Provider is InterceptingProvider)
                        {
                            (results.Provider as InterceptingProvider).Combiner =
                                aggregationImplementation.CombineTemporaryResults;
                        }
                        IQueryable queryToUse = results;
                        if (aggregateClause.AggregatableProperty.Contains('/'))
                        {
                            queryToUse = AggregationImplementationBase.FilterNullValues(query, Context.ElementClrType, aggregateClause);
                        }
                        var projectionLambda = AggregationImplementationBase.GetProjectionLambda(Context.ElementClrType, aggregateClause, propertyToAggregateExpression);
                        var aggragationResult = aggregationImplementation.DoAggregatinon(Context.ElementClrType, queryToUse, aggregateClause, projectionLambda);
                        var aliasType = aggregationImplementation.GetResultType(Context.ElementClrType, aggregateClause);
                        
                        results = ProjectResult(aggragationResult, aggregateClause.Alias, aliasType);
                        break;
                    case "groupby":
                        IEnumerable<LambdaExpression> propertiesToGroupByExpressions = null;
                        var groupByImplementation = new GroupByImplementation() { Context = this.Context };
                        var groupByClause = transformation.Item2 as ApplyGroupbyClause;
                        if (groupByClause == null)
                        {
                            throw Error.Argument("aggregation transformation type mismatch", transformation.Item2);
                        }
                        var entityParam = Expression.Parameter(Context.ElementClrType, "$it");
                        if (groupByClause.SelectedPropertiesExpressions != null)
                        {
                            propertiesToGroupByExpressions =
                                groupByClause.SelectedPropertiesExpressions.Select(
                                    exp =>
                                        FilterBinder.Bind(exp, Context.ElementClrType, Context.Model, assembliesResolver,
                                            updatedSettings, entityParam));
                        }
                        var keyType = groupByImplementation.GetGroupByKeyType(groupByClause);
                        if (groupByClause.Aggregate == null)
                        {
                            // simple group-by without aggregation method
                            results = groupByImplementation.DoGroupBy(results, groupByClause, keyType, propertiesToGroupByExpressions);
                        }
                        else
                        {
                            IQueryable keys = null;
                            propertyToAggregateExpression = FilterBinder.Bind(groupByClause.Aggregate.AggregatablePropertyExpression, Context.ElementClrType, Context.Model, assembliesResolver, updatedSettings);
                 
                            object[] aggragatedValues = null;
                            groupByImplementation.DoAggregatedGroupBy(results, maxResults, groupByClause, keyType, propertiesToGroupByExpressions, propertyToAggregateExpression,
                                out keys, out aggragatedValues);

                            results = ProjectGroupedResult(groupByClause, keys, aggragatedValues, keyType, Context);
                        }
                        break;
                    case "filter":
                        var filterClause = transformation.Item2 as ApplyFilterClause;
                        if (filterClause == null)
                        {
                            throw Error.Argument("aggregation transformation type mismatch", transformation.Item2);
                        }
                        var filterImplementation = new FilterImplementation() { Context = this.Context };
                        results = filterImplementation.DoFilter(results, filterClause, querySettings, _queryOptionParser);

                        break;

                    throw Error.NotSupported("aggregation not supported", transformation.Item1);
                }
            }

            object convertedResult = null;
            
            if (!QueriableProviderAdapter.ConvertionIsRequiredAsExpressionIfNotSupported(results, maxResults, out convertedResult))
            {
                return results;
            }
            
            return convertedResult as IQueryable;
        }

        /// <summary>
        /// Generate a new type for the aggregated results and return an instance of the results with the aggregated value 
        /// </summary>
        /// <param name="dataToProject">The results to project</param>
        /// <param name="alias">The name of the alias to use in the new type</param>
        /// <param name="aliasType">the type of the alias to use in the new type</param>
        /// <returns>An instance of the results with the aggregated value as <see cref="IQueryable"/></returns>
        private IQueryable ProjectResult(object dataToProject, string alias, Type aliasType)
        {
            var properties = new List<Tuple<Type, string>>() 
            { 
                new Tuple<Type, string>(aliasType, alias)
            };
            var resType = AggregationTypesGenerator.CreateType(properties.Distinct(new TypeStringTupleComapere()).ToList(), Context, true);
            
            var objToProject = Activator.CreateInstance(resType);
            var pi = resType.GetProperty(alias);
            pi.SetValue(objToProject, dataToProject);

            var dataToProjectList = (new List<object>() { objToProject }).AsQueryable();
            return ExpressionHelpers.Cast(resType, dataToProjectList);
        }

        /// <summary>
        /// Generate a new type for the group-by results and return an instance of the results with the aggregated values 
        /// </summary>
        /// <param name="groupByTrasformation">The group-by transformation clause</param>
        /// <param name="keys">The collection of the group-by keys</param>
        /// <param name="aggragatedValues">The results of the group-by aggregation</param>
        /// <param name="keyType">The group-by key type</param>
        /// <param name="context">The OData query context</param>
        /// <returns>The group-by results as <see cref="IQueryable"/></returns>
        private IQueryable ProjectGroupedResult(ApplyGroupbyClause groupByTrasformation, IQueryable keys, object[] aggragatedValues, Type keyType, ODataQueryContext context)
        {
            List<object> result = new List<object>();
            var keyProperties = keyType.GetProperties();
            var projectionType = GetAggregationResultProjectionType(groupByTrasformation, keyType);
            
            int i = 0;
            foreach (var key in keys)
            {
                var objToProject = Activator.CreateInstance(projectionType);
                var pi = projectionType.GetProperty(groupByTrasformation.Aggregate.Alias);
                pi.SetValue(objToProject, aggragatedValues[i]);

                foreach (var key_pi in keyProperties)
                {
                    if (key_pi.Name == "ComparerInstance")
                    {
                        continue;
                    }
                    pi = projectionType.GetProperty(key_pi.Name);
                    pi.SetValue(objToProject, key_pi.GetValue(key));
                }
                result.Add(objToProject);
                i++;
            }
          
            return ExpressionHelpers.Cast(projectionType, result.AsQueryable());
        }

        /// <summary>
        /// Get the type to use for returning aggregation results in a group-by query 
        /// </summary>
        /// <param name="groupByTrasformation">the group by transformation</param>
        /// <param name="keyType">The type of a group by key</param>
        /// <returns>The new dynamic type</returns>
        private Type GetAggregationResultProjectionType(ApplyGroupbyClause groupByTrasformation, Type keyType)
        {
            if (groupByTrasformation.Aggregate == null)
            {
                throw new ArgumentException("group by without aggregate");
            }
            var keyProperties = new List<Tuple<Type, string>>();
            var aggregationImplementation = AggregationImplementations<AggregationImplementationBase>.GetAggregationImplementation(groupByTrasformation.Aggregate.AggregationMethod);
            var aliasType = aggregationImplementation.GetResultType(Context.ElementClrType, groupByTrasformation.Aggregate);

            keyProperties.Add(new Tuple<Type, string>(aliasType, groupByTrasformation.Aggregate.Alias));
            foreach (var prop in keyType.GetProperties())
            {
                if (prop.Name == "ComparerInstance")
                {
                    continue;
                }
                keyProperties.Add(new Tuple<Type, string>(prop.PropertyType, prop.Name));
            }
            
            return AggregationTypesGenerator.CreateType(keyProperties.Distinct(new TypeStringTupleComapere()).ToList(), Context, true);
        }
    }



    /// <summary>
    /// Comparer class for <see cref="Tuple<Type, string>"/> 
    /// </summary>
    public class TypeStringTupleComapere : IEqualityComparer<Tuple<Type, string>>
    {
        /// <inheritdoc/>
        public bool Equals(Tuple<Type, string> x, Tuple<Type, string> y)
        {
            if ((x == null) && (y != null))
            {
                return false;
            }
            if ((y == null) && (x != null))
            {
                return false;
            }
            return (x.Item1.FullName == y.Item1.FullName) && (x.Item2 == y.Item2);
        }

        /// <inheritdoc/>
        public int GetHashCode(Tuple<Type, string> obj)
        {
            return obj.Item1.GetHashCode() + obj.Item2.GetHashCode();
        }
    }
}
