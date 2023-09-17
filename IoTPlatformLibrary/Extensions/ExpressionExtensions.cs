using System.Linq.Expressions;

namespace IoTPlatformLibrary.Extensions
{
    public static class ExpressionExtensions
    {
        public static string GetMemberName<TDelegate>(this Expression<TDelegate> expression)
        {
            var memberName = "";
            if (expression.Body is MemberExpression)
                memberName = ((MemberExpression)expression.Body).Member.Name;
            else
                memberName = GetMemberName((UnaryExpression)expression.Body);

            return memberName;
        }


        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)unaryExpression.Operand;
                return methodExpression.Method.Name;
            }

            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }
    }
}
