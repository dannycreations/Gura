using System;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class TypeBinaryBinder
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
			object typeName = node.GetTypeName(true);
			Type type = null;
			if (!bindingContext.TryResolveType(typeName, out type))
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVETYPE, typeName), node);
				return false;
			}
			Expression expression2 = null;
			if (!AnyBinder.TryBindInNewScope(expression, bindingContext, TypeDescription.ObjectType, out expression2, out bindingError))
			{
				return false;
			}
			if (expressionType != null)
			{
				if (!(expressionType == "TypeIs"))
				{
					if (!(expressionType == "TypeAs"))
					{
						if (!(expressionType == "Convert"))
						{
							if (!(expressionType == "ConvertChecked"))
							{
								goto IL_11F;
							}
							boundExpression = Expression.ConvertChecked(expression2, type);
						}
						else
						{
							boundExpression = Expression.Convert(expression2, type);
						}
					}
					else
					{
						boundExpression = Expression.TypeAs(expression2, type);
					}
				}
				else
				{
					boundExpression = Expression.TypeIs(expression2, type);
				}
				return true;
			}
			IL_11F:
			boundExpression = null;
			bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, expressionType), node);
			return false;
		}
	}
}
