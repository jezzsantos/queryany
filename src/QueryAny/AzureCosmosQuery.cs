using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryAny.Primitives;

namespace QueryAny
{
    public class AzureCosmosQuery
    {
        private readonly List<QueryExpression> expressions = new List<QueryExpression>();

        internal AzureCosmosQuery(QueryExpression expression)
        {
            if (expression != null)
            {
                this.expressions.Add(expression);
            }
        }

        public IReadOnlyList<QueryExpression> Expressions => this.expressions;

        public static AzureCosmosQuery Empty()
        {
            return new AzureCosmosQuery(null);
        }

        public static AzureCosmosQuery Create(string columnName, Condition @operator, string value)
        {
            return CreateInternal(columnName, @operator, value);
        }

        public static AzureCosmosQuery Create(string columnName, Condition @operator, DateTime value)
        {
            return CreateInternal(columnName, @operator, value);
        }

        public static AzureCosmosQuery Create(string columnName, Condition @operator, bool value)
        {
            return CreateInternal(columnName, @operator, value);
        }

        public static AzureCosmosQuery Create<TEntity>(Expression<Func<TEntity, string>> propertyName, Condition @operator,
            string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, @operator, value);
        }

        public static AzureCosmosQuery Create<TEntity>(Expression<Func<TEntity, DateTime>> propertyName, Condition @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, @operator, value);
        }

        public static AzureCosmosQuery Create<TEntity>(Expression<Func<TEntity, string>> propertyName, Condition @operator,
            bool value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, @operator, value);
        }

        public static AzureCosmosQuery CreateEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, Condition.Eq, DateTime.MinValue);
        }

        public static AzureCosmosQuery CreateNotEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return CreateInternal(columnName, Condition.Ne, DateTime.MinValue);
        }

        public AzureCosmosQuery From(string columnName, Condition @operator, string value)
        {
            return AddInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery From(string columnName, Condition @operator, DateTime value)
        {
            return AddInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery From(string columnName, Condition @operator, bool value)
        {
            return AddInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery From<TEntity>(Expression<Func<TEntity, string>> propertyName, Condition @operator,
            string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AddInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery From<TEntity>(Expression<Func<TEntity, DateTime>> propertyName, Condition @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AddInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery From<TEntity>(Expression<Func<TEntity, string>> propertyName, Condition @operator, bool value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AddInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery And(string columnName, Condition @operator, string value)
        {
            return AndInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery And(string columnName, Condition @operator, DateTime value)
        {
            return AndInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery And(string columnName, Condition @operator, bool value)
        {
            return AndInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery And<TEntity>(Expression<Func<TEntity, string>> propertyName, Condition @operator, string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery And<TEntity>(Expression<Func<TEntity, DateTime>> propertyName, Condition @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery And<TEntity>(Expression<Func<TEntity, string>> propertyName, Condition @operator, bool value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery AndEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, Condition.Eq, DateTime.MinValue);
        }

        public AzureCosmosQuery AndNotEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return AndInternal(columnName, Condition.Ne, DateTime.MinValue);
        }

        public AzureCosmosQuery And(AzureCosmosQuery query)
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
                        Combiner = Combine.And,
                        Condition = null,
                        NestedExpressions = query.Expressions.ToList()
                    });
                }
            }

            return this;
        }

        public AzureCosmosQuery Or(string columnName, Condition @operator, string value)
        {
            return OrInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery Or(string columnName, Condition @operator, DateTime value)
        {
            return OrInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery Or(string columnName, Condition @operator, bool value)
        {
            return OrInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery Or<TEntity>(Expression<Func<TEntity, string>> propertyName, Condition @operator, string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery Or<TEntity>(Expression<Func<TEntity, DateTime>> propertyName, Condition @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery Or<TEntity>(Expression<Func<TEntity, string>> propertyName, Condition @operator, bool value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, @operator, value);
        }

        public AzureCosmosQuery OrEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, Condition.Eq, DateTime.MinValue);
        }

        public AzureCosmosQuery OrNotEmptyDate<TEntity>(Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return OrInternal(columnName, Condition.Ne, DateTime.MinValue);
        }

        public AzureCosmosQuery And(Func<AzureCosmosQuery, AzureCosmosQuery> subquery)
        {
            if (!this.expressions.Any())
            {
                throw new ArgumentOutOfRangeException();
            }

            var query = new AzureCosmosQuery(null);
            subquery(query);

            this.expressions.Add(new QueryExpression
            {
                Condition = null,
                Combiner = Combine.And,
                NestedExpressions = query.expressions
            });

            return this;
        }

        public AzureCosmosQuery Or(Func<AzureCosmosQuery, AzureCosmosQuery> subquery)
        {
            if (!this.expressions.Any())
            {
                throw new ArgumentOutOfRangeException();
            }

            var query = new AzureCosmosQuery(null);
            subquery(query);

            this.expressions.Add(new QueryExpression
            {
                Condition = null,
                Combiner = Combine.Or,
                NestedExpressions = query.expressions
            });

            return this;
        }

        public AzureCosmosQuery Or(AzureCosmosQuery query)
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
                        Combiner = Combine.Or,
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

        private static AzureCosmosQuery CreateInternal(string columnName, Condition @operator, object value)
        {
            Guard.AgainstNullOrEmpty(() => columnName, columnName);

            return new AzureCosmosQuery(new QueryExpression
            {
                Condition = new QueryCondition
                {
                    Column = columnName,
                    Operator = @operator,
                    Value = value
                },
                Combiner = Combine.None
            });
        }

        private AzureCosmosQuery AddInternal(string columnName, Condition @operator, object value)
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
                Combiner = this.expressions.Any() ? Combine.And : Combine.None
            });

            return this;
        }

        private AzureCosmosQuery AndInternal(string columnName, Condition @operator, object value)
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
                Combiner = Combine.And
            });

            return this;
        }

        private AzureCosmosQuery OrInternal(string columnName, Condition @operator, object value)
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
                Combiner = Combine.Or
            });

            return this;
        }
    }

    public class QueryExpression
    {
        public QueryCondition Condition { get; set; }

        public Combine Combiner { get; set; }

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

        public Condition Operator { get; set; }

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
            Condition @operator, string value)
        {
            return IsInternal(expression, propertyName, @operator, value);
        }

        public static bool Is<TEntity>(this QueryExpression expression,
            Expression<Func<TEntity, DateTime>> propertyName, Condition @operator, DateTime value)
        {
            return IsInternal(expression, propertyName, @operator, value);
        }

        private static bool IsInternal<TEntity, TProperty>(this QueryExpression expression,
            Expression<Func<TEntity, TProperty>> propertyName, Condition @operator,
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
                   && expression.Condition.Operator == Condition.Eq
                   && expression.Condition.Value.Equals(DateTime.MinValue);
        }

        public static bool IsNotEmptyDate<TEntity>(this QueryExpression expression,
            Expression<Func<TEntity, DateTime>> propertyName)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);

            return expression.Condition != null
                   && expression.Condition.Column.EqualsOrdinal(columnName)
                   && expression.Condition.Operator == Condition.Ne
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

    public enum Condition
    {
        /// <summary>
        ///     Equals
        /// </summary>
        Eq,

        /// <summary>
        ///     Greater than
        /// </summary>
        Gt,

        /// <summary>
        ///     Greater than or equal to
        /// </summary>
        Ge,

        /// <summary>
        ///     Less than
        /// </summary>
        Lt,

        /// <summary>
        ///     Less than or equal to
        /// </summary>
        Le,

        /// <summary>
        ///     Not equal
        /// </summary>
        Ne
    }

    public enum Combine
    {
        None = 0,

        /// <summary>
        ///     Logical AND
        /// </summary>
        And = 1,

        /// <summary>
        ///     Logical OR
        /// </summary>
        Or = 2
    }
}