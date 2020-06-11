using System.Collections.Generic;

namespace QueryAny
{
    public class WhereExpression
    {
        public WhereCondition Condition { get; set; }

        public LogicalOperator Operator { get; set; }

        public List<WhereExpression> NestedExpressions { get; set; }
    }

    public class WhereCondition
    {
        public string FieldName { get; set; }

        public ConditionOperator Operator { get; set; }

        public object Value { get; set; }
    }

    public enum ConditionOperator
    {
        EqualTo,
        GreaterThan,
        GreaterThanEqualTo,
        LessThan,
        LessThanEqualTo,
        NotEqualTo
    }

    public enum LogicalOperator
    {
        None = 0,
        And = 1,
        Or = 2
    }
}