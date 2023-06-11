using QueryAny.Extensions;

namespace QueryAny
{
    public class JoinDefinition
    {
        public JoinDefinition(JoinSide left, JoinSide right, JoinType type)
        {
            left.GuardAgainstNull(nameof(left));
            right.GuardAgainstNull(nameof(right));
            Left = left;
            Right = right;
            Type = type;
        }

        public JoinSide Left { get; }

        public JoinSide Right { get; }

        public JoinType Type { get; }
    }
}