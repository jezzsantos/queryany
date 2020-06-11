using System.Collections.Generic;

namespace QueryAny
{
    public class QueryExpression
    {
        public QueryCondition Condition { get; set; }

        public Combine Combiner { get; set; }

        public List<QueryExpression> NestedExpressions { get; set; }
    }

    public class QueryCondition
    {
        public string Column { get; set; }

        public Condition Operator { get; set; }

        public object Value { get; set; }
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