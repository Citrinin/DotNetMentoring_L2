using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Expressions_Task3
{
    public class ExpressionToFTSRequestTranslator : ExpressionVisitor
    {
        StringBuilder resultString;
        List<string> resultList;

        public List<string> Translate(Expression exp)
        {
            resultString = new StringBuilder();
            resultList = new List<string>();
            Visit(exp);

            return resultList.Count == 0 ? (new List<string> {resultString.ToString()}) : resultList;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }

            if (node.Method.DeclaringType == typeof(string) &&
                node.Arguments.All(a =>
                    a.NodeType == ExpressionType.Constant ||
                    a.NodeType == ExpressionType.Convert &&
                    ((UnaryExpression)a).Operand.NodeType == ExpressionType.Constant
                ))
            {
                switch (node.Method.Name)
                {
                    case nameof(string.StartsWith):
                        {
                            Visit(node.Arguments);
                            resultString.Append("*");
                            break;
                        }
                    case nameof(string.EndsWith):
                        {
                            resultString.Append("*");
                            Visit(node.Arguments);
                            break;
                        }
                    case nameof(string.Contains):
                        {
                            resultString.Append("*");
                            Visit(node.Arguments);
                            resultString.Append("*");
                            break;
                        }
                    default:
                        {
                            return base.VisitMethodCall(node);
                        }
                }
                return node;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                {
                    if (node.Left.NodeType == ExpressionType.MemberAccess &&
                        node.Right.NodeType == ExpressionType.Constant)
                    {
                        Visit(node.Left);
                        resultString.Append("(");
                        Visit(node.Right);
                        resultString.Append(")");
                    }
                    else if (node.Right.NodeType == ExpressionType.MemberAccess &&
                             node.Left.NodeType == ExpressionType.Constant)
                    {
                        Visit(node.Right);
                        resultString.Append("(");
                        Visit(node.Left);
                        resultString.Append(")");
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Incorrect fields format", node.NodeType));
                    }


                    break;
                }
                case ExpressionType.AndAlso:
                {
                    Visit(node.Left);
                    resultList.Add(resultString.ToString());
                    resultString = new StringBuilder();
                    Visit(node.Right);
                    resultList.Add(resultString.ToString());
                    break;
                }
                default:
                {
                    throw new NotSupportedException(string.Format("Operation {0} is not supported", node.NodeType));
                }
            };

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            resultString.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            resultString.Append(node.Value);

            return node;
        }
    }
}
