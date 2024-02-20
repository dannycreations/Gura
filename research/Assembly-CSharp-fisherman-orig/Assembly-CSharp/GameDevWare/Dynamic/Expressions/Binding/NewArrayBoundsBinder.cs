using System;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class NewArrayBoundsBinder
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
			object typeName = node.GetTypeName(true);
			Type type = null;
			if (!bindingContext.TryResolveType(typeName, out type))
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVETYPE, typeName), node);
				return false;
			}
			TypeDescription int32Type = TypeDescription.Int32Type;
			ArgumentsTree arguments = node.GetArguments(true);
			Expression[] array = new Expression[arguments.Count];
			for (int i = 0; i < arguments.Count; i++)
			{
				SyntaxTreeNode syntaxTreeNode = null;
				if (!arguments.TryGetValue(i, out syntaxTreeNode))
				{
					bindingError = new ExpressionParserException(Resources.EXCEPTION_BOUNDEXPR_ARGSDOESNTMATCHPARAMS, node);
					return false;
				}
				if (!AnyBinder.TryBindInNewScope(syntaxTreeNode, bindingContext, int32Type, out array[i], out bindingError))
				{
					return false;
				}
			}
			boundExpression = Expression.NewArrayBounds(type, array);
			return true;
		}
	}
}
