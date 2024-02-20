using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class InvokeBinder
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
			if (InvokeBinder.TryBindMethodCall(node, bindingContext, expectedType, out boundExpression, out bindingError))
			{
				return true;
			}
			SyntaxTreeNode expression = node.GetExpression(true);
			ArgumentsTree arguments = node.GetArguments(false);
			Expression expression2 = null;
			if (!AnyBinder.TryBind(expression, bindingContext, TypeDescription.ObjectType, out expression2, out bindingError))
			{
				return false;
			}
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(expression2.Type);
			if (!typeDescription.IsDelegate)
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETOINVOKENONDELEG, expression2.Type), node);
				return false;
			}
			MemberDescription memberDescription = typeDescription.GetMembers("Invoke").FirstOrDefault((MemberDescription m) => m.IsMethod && !m.IsStatic);
			if (memberDescription == null)
			{
				throw new MissingMethodException(expression2.Type.FullName, "Invoke");
			}
			float num = 0f;
			if (!memberDescription.TryMakeCall(expression2, arguments, bindingContext, out boundExpression, out num))
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETOBINDDELEG, expression2.Type, memberDescription), node);
				return false;
			}
			return true;
		}

		public static bool TryBindMethodCall(SyntaxTreeNode node, BindingContext bindingContext, TypeDescription expectedType, out Expression boundExpression, out Exception bindingError)
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
			bindingError = null;
			boundExpression = null;
			SyntaxTreeNode expression = node.GetExpression(true);
			string expressionType = expression.GetExpressionType(true);
			if (expressionType != "PropertyOrField")
			{
				return false;
			}
			SyntaxTreeNode expression2 = expression.GetExpression(false);
			Expression expression3 = null;
			Type type = null;
			TypeReference typeReference = null;
			bool flag;
			if (expression2 == null && bindingContext.Global != null)
			{
				expression3 = bindingContext.Global;
				type = expression3.Type;
				flag = false;
			}
			else
			{
				if (expression2 == null)
				{
					string propertyOrFieldName = expression.GetPropertyOrFieldName(false);
					bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVENAME, propertyOrFieldName ?? "<unknown>"), node);
					return false;
				}
				if (BindingContext.TryGetTypeReference(expression2, out typeReference) && bindingContext.TryResolveType(typeReference, out type))
				{
					flag = true;
				}
				else
				{
					if (!AnyBinder.TryBind(expression2, bindingContext, TypeDescription.ObjectType, out expression3, out bindingError))
					{
						if (typeReference != null && bindingError == null)
						{
							bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVETYPE, typeReference), node);
						}
						return false;
					}
					flag = false;
					type = expression3.Type;
				}
			}
			TypeReference typeReference2 = null;
			if (type == null || !BindingContext.TryGetMethodReference(expression, out typeReference2))
			{
				return false;
			}
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(type);
			foreach (MemberDescription memberDescription in typeDescription.GetMembers(typeReference2.Name))
			{
				if (memberDescription.IsMethod && memberDescription.IsStatic == flag)
				{
					SyntaxTreeNode syntaxTreeNode = new SyntaxTreeNode(new Dictionary<string, object>
					{
						{
							"expression",
							expression3 ?? type
						},
						{
							"arguments",
							node.GetValueOrDefault<object>("arguments", null)
						},
						{ "method", typeReference2 },
						{
							"useNullPropagation",
							expression.GetValueOrDefault<object>("useNullPropagation", null)
						},
						{
							"_pos",
							expression.GetPosition(false)
						}
					});
					return CallBinder.TryBind(syntaxTreeNode, bindingContext, expectedType, out boundExpression, out bindingError);
				}
			}
			return false;
		}
	}
}
