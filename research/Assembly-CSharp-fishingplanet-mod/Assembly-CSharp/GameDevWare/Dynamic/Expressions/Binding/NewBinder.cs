using System;
using System.Linq;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class NewBinder
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
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}
			ArgumentsTree arguments = node.GetArguments(false);
			object typeName = node.GetTypeName(true);
			Type type = null;
			if (!bindingContext.TryResolveType(typeName, out type))
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVETYPE, typeName), node);
				return false;
			}
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(type);
			SyntaxTreeNode syntaxTreeNode;
			if (typeDescription.IsDelegate && arguments.Count == 1 && (syntaxTreeNode = arguments.Values.Single<SyntaxTreeNode>()).GetExpressionType(true) == "Lambda")
			{
				return LambdaBinder.TryBind(syntaxTreeNode, bindingContext, typeDescription, out boundExpression, out bindingError);
			}
			float num = 0f;
			foreach (MemberDescription memberDescription in typeDescription.Constructors)
			{
				float num2 = 0f;
				Expression expression = null;
				if (memberDescription.TryMakeCall(null, arguments, bindingContext, out expression, out num2))
				{
					if (!float.IsNaN(num2) && num2 > num)
					{
						boundExpression = expression;
						num = num2;
						if (Math.Abs(num2 - 1f) < 1E-45f)
						{
							break;
						}
					}
				}
			}
			if (boundExpression == null)
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETOBINDCONSTRUCTOR, type), node);
				return false;
			}
			return true;
		}
	}
}
