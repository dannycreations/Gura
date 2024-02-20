using System;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class IndexBinder
	{
		public static bool TryBind(SyntaxTreeNode node, BindingContext bindingContext, TypeDescription expectedType, out Expression boundExpression, out Exception bindingError)
		{
			boundExpression = null;
			bindingError = null;
			bool useNullPropagation = node.GetUseNullPropagation(false);
			ArgumentsTree arguments = node.GetArguments(true);
			SyntaxTreeNode expression = node.GetExpression(true);
			Expression expression2 = null;
			if (!AnyBinder.TryBind(expression, bindingContext, TypeDescription.ObjectType, out expression2, out bindingError))
			{
				return false;
			}
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(expression2.Type);
			if (expression2.Type.IsArray)
			{
				TypeDescription int32Type = TypeDescription.Int32Type;
				Expression[] array = new Expression[arguments.Count];
				for (int i = 0; i < array.Length; i++)
				{
					SyntaxTreeNode syntaxTreeNode = null;
					if (!arguments.TryGetValue(i, out syntaxTreeNode))
					{
						bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGMETHODPARAMETER, i), node);
						return false;
					}
					if (!AnyBinder.TryBindInNewScope(syntaxTreeNode, bindingContext, int32Type, out array[i], out bindingError))
					{
						return false;
					}
				}
				try
				{
					if (array.Length == 1)
					{
						boundExpression = Expression.ArrayIndex(expression2, array[0]);
					}
					else
					{
						boundExpression = Expression.ArrayIndex(expression2, array);
					}
				}
				catch (Exception ex)
				{
					bindingError = new ExpressionParserException(ex.Message, ex, node);
					return false;
				}
			}
			else
			{
				float num = 0f;
				foreach (MemberDescription memberDescription in typeDescription.Indexers)
				{
					float num2 = 0f;
					Expression expression3 = null;
					if (memberDescription.TryMakeCall(expression2, arguments, bindingContext, out expression3, out num2))
					{
						if (num2 > num)
						{
							boundExpression = expression3;
							num = num2;
						}
					}
				}
			}
			if (boundExpression == null)
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETOBINDINDEXER, expression2.Type), node);
				return false;
			}
			if (useNullPropagation && typeDescription.CanBeNull)
			{
				bindingContext.RegisterNullPropagationTarger(expression2);
			}
			return true;
		}
	}
}
