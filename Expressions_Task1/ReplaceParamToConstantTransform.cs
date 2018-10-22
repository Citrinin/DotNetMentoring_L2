using System.Collections.Generic;
using System.Linq.Expressions;

namespace Expressions_Task1
{
    public class ReplaceParamToConstantTransform : ExpressionVisitor
    {
        private Dictionary<string, int> _parametersToReplace;

        public T VisitAndReplace<T>(T expression, Dictionary<string, int> parametersToReplace) where T : Expression
        {
            _parametersToReplace = parametersToReplace;
            return VisitAndConvert(expression, "");
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var parameters = node.Parameters;
            return Expression.Lambda(Visit(node.Body), parameters);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_parametersToReplace.ContainsKey(node.Name))
            {
                return Expression.Constant(_parametersToReplace[node.Name]);
            }

            return base.VisitParameter(node);

        }
    }
}
