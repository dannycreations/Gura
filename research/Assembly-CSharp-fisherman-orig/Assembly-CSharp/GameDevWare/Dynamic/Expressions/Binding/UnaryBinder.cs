using System;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class UnaryBinder
	{
		public static bool TryBind(SyntaxTreeNode node, BindingContext bindingContext, TypeDescription expectedType, out Expression boundExpression, out Exception bindingError)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			if (bindingContext == null)
			{
				throw new ArgumentNullException("bindingContext");
			}
			if (expectedType == null)
			{
				throw new ArgumentNullException("expectedType");
			}
			boundExpression = null;
			bindingError = null;
			string expressionType = node.GetExpressionType(true);
			SyntaxTreeNode expression = node.GetExpression(true);
			Expression expression2 = null;
			if (!AnyBinder.TryBindInNewScope(expression, bindingContext, TypeDescription.ObjectType, out expression2, out bindingError))
			{
				return false;
			}
			if (expressionType != null)
			{
				if (!(expressionType == "Negate"))
				{
					if (!(expressionType == "NegateChecked"))
					{
						if (!(expressionType == "Complement") && !(expressionType == "Not"))
						{
							if (!(expressionType == "UnaryPlus"))
							{
								goto IL_1EA;
							}
							if (!ExpressionUtils.TryPromoteUnaryOperation(ref expression2, ExpressionType.UnaryPlus, out boundExpression))
							{
								boundExpression = Expression.UnaryPlus(expression2);
							}
						}
						else if (!ExpressionUtils.TryPromoteUnaryOperation(ref expression2, ExpressionType.Not, out boundExpression))
						{
							boundExpression = Expression.Not(expression2);
						}
					}
					else if (!ExpressionUtils.TryPromoteUnaryOperation(ref expression2, ExpressionType.NegateChecked, out boundExpression))
					{
						if (expression2.Type == typeof(double) || expression2.Type == typeof(float))
						{
							boundExpression = Expression.Multiply(expression2, (expression2.Type != typeof(float)) ? ExpressionUtils.NegativeDouble : ExpressionUtils.NegativeSingle);
						}
						else
						{
							boundExpression = Expression.NegateChecked(expression2);
						}
					}
				}
				else if (!ExpressionUtils.TryPromoteUnaryOperation(ref expression2, ExpressionType.Negate, out boundExpression))
				{
					if (expression2.Type == typeof(double) || expression2.Type == typeof(float))
					{
						boundExpression = Expression.Multiply(expression2, (expression2.Type != typeof(float)) ? ExpressionUtils.NegativeDouble : ExpressionUtils.NegativeSingle);
					}
					else
					{
						boundExpression = Expression.Negate(expression2);
					}
				}
				return true;
			}
			IL_1EA:
			bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, expressionType), node);
			return false;
		}
	}
}
