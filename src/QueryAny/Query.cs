using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryAny.Primitives;

namespace QueryAny
{
    public class Query
    {
        private readonly List<QueryExpression> expressions = new List<QueryExpression>();

        internal Query(QueryExpression expression)
        {
            if (expression != null)
            {
                this.expressions.Add(expression);
            }
        }

        public IReadOnlyList<QueryExpression> Expressions => this.expressions;

        public static Query Empty()
        {
            return new Query(null);
        }

        public static Query Create(string columnName, QueryOperator @operator, string value)
        {
            return CreateInternal(columnName, @operator, value);
        }

        public static Query Create(string columnName, QueryOperator @operator, DateTime value)
        {
            return CreateInternal(columnName, @operator, value);
        }

        public static Query Create(string columnName, QueryOperator @operator, bool value)
        {
            return CreateInternal(columnName, @operator, value);
        }

        public static Query Create<TEntity>(Expression<Func<TEntity, string>> propertyName, QueryOperator @operator,
            string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, @operator, value);
        }

        public static Query Create<TEntity>(Expression<Func<TEntity, DateTime>> propertyName, QueryOperator @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, @operator, value);
        }

        public static Query Create<TEntity>(Expression<Func<TEntity, string>> propertyName, QueryOperator @operator,
            bool value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, @operator, value);
        }

        public static Query CreateEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, QueryOperator.EQ, DateTime.MinValue);
        }

        public static Query CreateNotEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, QueryOperator.NE, DateTime.MinValue);
        }

        public Query From(string columnName, QueryOperator @operator, string value)
        {
            return AddInternal(columnName, @operator, value);
        }

        public Query From(string columnName, QueryOperator @operator, DateTime value)
        {
            return AddInternal(columnName, @operator, value);
        }

        public Query From(string columnName, QueryOperator @operator, bool value)
        {
            return AddInternal(columnName, @operator, value);
        }

        public Query From<TEntity>(Expression<Func<TEntity, string>> propertyName, QueryOperator @operator,
            string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AddInternal(columnName, @operator, value);
        }

        public Query From<TEntity>(Expression<Func<TEntity, DateTime>> propertyName, QueryOperator @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AddInternal(columnName, @operator, value);
        }

        public Query From<TEntity>(Expression<Func<TEntity, string>> propertyName, QueryOperator @operator, bool value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AddInternal(columnName, @operator, value);
        }

        public Query And(string columnName, QueryOperator @operator, string value)
        {
            return AndInternal(columnName, @operator, value);
        }

        public Query And(string columnName, QueryOperator @operator, DateTime value)
        {
            return AndInternal(columnName, @operator, value);
        }

        public Query And(string columnName, QueryOperator @operator, bool value)
        {
            return AndInternal(columnName, @operator, value);
        }

        public Query And<TEntity>(Expression<Func<TEntity, string>> propertyName, QueryOperator @operator, string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, @operator, value);
        }

        public Query And<TEntity>(Expression<Func<TEntity, DateTime>> propertyName, QueryOperator @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, @operator, value);
        }

        public Query And<TEntity>(Expression<Func<TEntity, string>> propertyName, QueryOperator @operator, bool value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, @operator, value);
        }

        public Query AndEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, QueryOperator.EQ, DateTime.MinValue);
        }

        public Query AndNotEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, QueryOperator.NE, DateTime.MinValue);
        }

        public Query And(Query query)
        {
            Guard.AgainstNull(() => query, query);
            if (query.Expressions.Any())
            {
                if (!Expressions.Any())
                {
                    this.expressions.AddRange(query.expressions);
                }
                else
                {
                    this.expressions.Add(new QueryExpression
                    {
                        Combiner = CombineOperator.AND,
                        Condition = null,
                        NestedExpressions = query.Expressions.ToList()
                    });
                }
            }

            return this;
        }

        public Query Or(string columnName, QueryOperator @operator, string value)
        {
            return OrInternal(columnName, @operator, value);
        }

        public Query Or(string columnName, QueryOperator @operator, DateTime value)
        {
            return OrInternal(columnName, @operator, value);
        }

        public Query Or(string columnName, QueryOperator @operator, bool value)
        {
            return OrInternal(columnName, @operator, value);
        }

        public Query Or<TEntity>(Expression<Func<TEntity, string>> propertyName, QueryOperator @operator, string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, @operator, value);
        }

        public Query Or<TEntity>(Expression<Func<TEntity, DateTime>> propertyName, QueryOperator @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, @operator, value);
        }

        public Query Or<TEntity>(Expression<Func<TEntity, string>> propertyName, QueryOperator @operator, bool value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, @operator, value);
        }

        public Query OrEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, QueryOperator.EQ, DateTime.MinValue);
        }

        public Query OrNotEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, QueryOperator.NE, DateTime.MinValue);
        }

        public Query And(Func<Query, Query> subquery)
        {
            if (!this.expressions.Any())
            {
                throw new ArgumentOutOfRangeException();
            }

            var query = new Query(null);
            subquery(query);

            this.expressions.Add(new QueryExpression
            {
                Condition = null,
                Combiner = CombineOperator.AND,
                NestedExpressions = query.expressions
            });

            return this;
        }

        public Query Or(Func<Query, Query> subquery)
        {
            if (!this.expressions.Any())
            {
                throw new ArgumentOutOfRangeException();
            }

            var query = new Query(null);
            subquery(query);

            this.expressions.Add(new QueryExpression
            {
                Condition = null,
                Combiner = CombineOperator.OR,
                NestedExpressions = query.expressions
            });

            return this;
        }

        public Query Or(Query query)
        {
            Guard.AgainstNull(() => query, query);
            if (query.Expressions.Any())
            {
                if (!Expressions.Any())
                {
                    this.expressions.AddRange(query.expressions);
                }
                else
                {
                    this.expressions.Add(new QueryExpression
                    {
                        Combiner = CombineOperator.OR,
                        Condition = null,
                        NestedExpressions = query.Expressions.ToList()
                    });
                }
            }

            return this;
        }

        public string ToQueryString()
        {
            if (!Expressions.Any())
            {
                return string.Empty;
            }

            if (Expressions.Count() == 1)
            {
                return Expressions.First().ToQueryString();
            }

            return Expressions.ToQueryString();
        }

        private static Query CreateInternal(string columnName, QueryOperator @operator, object value)
        {
            Guard.AgainstNullOrEmpty(() => columnName, columnName);

            return new Query(new QueryExpression
            {
                Condition = new QueryCondition
                {
                    Column = columnName,
                    Operator = @operator,
                    Value = value
                },
                Combiner = CombineOperator.None
            });
        }

        private Query AddInternal(string columnName, QueryOperator @operator, object value)
        {
            Guard.AgainstNullOrEmpty(() => columnName, columnName);

            this.expressions.Add(new QueryExpression
            {
                Condition = new QueryCondition
                {
                    Column = columnName,
                    Operator = @operator,
                    Value = value
                },
                Combiner = this.expressions.Any() ? CombineOperator.AND : CombineOperator.None
            });

            return this;
        }

        private Query AndInternal(string columnName, QueryOperator @operator, object value)
        {
            Guard.AgainstNullOrEmpty(() => columnName, columnName);

            if (!this.expressions.Any())
            {
                throw new ArgumentOutOfRangeException();
            }

            this.expressions.Add(new QueryExpression
            {
                Condition = new QueryCondition
                {
                    Column = columnName,
                    Operator = @operator,
                    Value = value
                },
                Combiner = CombineOperator.AND
            });

            return this;
        }

        private Query OrInternal(string columnName, QueryOperator @operator, object value)
        {
            Guard.AgainstNullOrEmpty(() => columnName, columnName);

            if (!this.expressions.Any())
            {
                throw new ArgumentOutOfRangeException();
            }

            this.expressions.Add(new QueryExpression
            {
                Condition = new QueryCondition
                {
                    Column = columnName,
                    Operator = @operator,
                    Value = value
                },
                Combiner = CombineOperator.OR
            });

            return this;
        }
    }

    public class QueryExpression
    {
        public QueryCondition Condition { get; set; }

        public CombineOperator Combiner { get; set; }

        public List<QueryExpression> NestedExpressions { get; set; }

        public string ToQueryString()
        {
            if (Condition == null
                || NestedExpressions != null)
                //HACK: Not sure how we are executing nested expressions properly
            {
                return NestedExpressions.ToQueryString();
            }

            return Condition.ToQueryString();
        }
    }

    public class QueryCondition
    {
        public string Column { get; set; }

        public QueryOperator Operator { get; set; }

        public object Value { get; set; }

        public string ToQueryString()
        {
            if (Value is DateTime)
            {
                var givenDate = (DateTime) Value;
                if (!givenDate.HasValue())
                {
                    givenDate = AzureCosmosTableQuery.MinAzureDateTime;
                }

                var dateValue = new DateTimeOffset(givenDate);
                return AzureCosmosTableQuery.GenerateFilterConditionForDate(Column,
                    Operator.ToString().ToLowerInvariant(), dateValue);
            }

            if (Value is bool)
            {
                var givenBool = (bool) Value;
                var boolValue = givenBool.ToLower();
                return AzureCosmosTableQuery.GenerateFilterCondition(Column, Operator.ToString().ToLowerInvariant(),
                    boolValue);
            }

            var stringValue = Value?.ToString();
            return AzureCosmosTableQuery.GenerateFilterCondition(Column, Operator.ToString().ToLowerInvariant(),
                stringValue);
        }
    }

    public static class QueryExpressionExtensions
    {
        public static bool Is<TEntity>(this QueryExpression expression, Expression<Func<TEntity, string>> propertyName,
            QueryOperator @operator, string value)
        {
            return IsInternal(expression, propertyName, @operator, value);
        }

        public static bool Is<TEntity>(this QueryExpression expression,
            Expression<Func<TEntity, DateTime>> propertyName, QueryOperator @operator, DateTime value)
        {
            return IsInternal(expression, propertyName, @operator, value);
        }

        private static bool IsInternal<TEntity, TProperty>(this QueryExpression expression,
            Expression<Func<TEntity, TProperty>> propertyName, QueryOperator @operator,
            TProperty value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return expression.Condition != null
                   && expression.Condition.Column.EqualsOrdinal(columnName)
                   && expression.Condition.Operator == @operator
                   && expression.Condition.Value.Equals(value);
        }

        public static bool IsEmptyDate<TEntity>(this QueryExpression expression,
            Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return expression.Condition != null
                   && expression.Condition.Column.EqualsOrdinal(columnName)
                   && expression.Condition.Operator == QueryOperator.EQ
                   && expression.Condition.Value.Equals(DateTime.MinValue);
        }

        public static bool IsNotEmptyDate<TEntity>(this QueryExpression expression,
            Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return expression.Condition != null
                   && expression.Condition.Column.EqualsOrdinal(columnName)
                   && expression.Condition.Operator == QueryOperator.NE
                   && expression.Condition.Value.Equals(DateTime.MinValue);
        }

        public static string ToQueryString(this IEnumerable<QueryExpression> expressions)
        {
            var combinedQuery = string.Empty;
            expressions
                .ToList()
                .ForEach(expression =>
                {
                    combinedQuery = !combinedQuery.HasValue()
                        ? expression.ToQueryString()
                        : AzureCosmosTableQuery.CombineFilters(combinedQuery,
                            expression.Combiner.ToString().ToLowerInvariant(), expression.ToQueryString());
                });

            return combinedQuery;
        }
    }

    public enum QueryOperator
    {
        /// <summary>
        ///     Equals
        /// </summary>
        // ReSharper disable once InconsistentNaming
        EQ,

        /// <summary>
        ///     Greater than
        /// </summary>
        // ReSharper disable once InconsistentNaming
        GT,

        /// <summary>
        ///     Greater than or equal to
        /// </summary>
        // ReSharper disable once InconsistentNaming
        GE,

        /// <summary>
        ///     Less than
        /// </summary>
        // ReSharper disable once InconsistentNaming
        LT,

        /// <summary>
        ///     Less than or equal to
        /// </summary>
        // ReSharper disable once InconsistentNaming
        LE,

        /// <summary>
        ///     Not equal
        /// </summary>
        // ReSharper disable once InconsistentNaming
        NE
    }

    public enum CombineOperator
    {
        None = 0,

        /// <summary>
        ///     Logical AND
        /// </summary>
        // ReSharper disable once InconsistentNaming
        AND = 1,

        /// <summary>
        ///     Logical OR
        /// </summary>
        // ReSharper disable once InconsistentNaming
        OR = 2
    }
}