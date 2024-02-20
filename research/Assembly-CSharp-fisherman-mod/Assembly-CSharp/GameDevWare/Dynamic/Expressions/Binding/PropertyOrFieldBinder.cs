using System;
using System.Linq;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class PropertyOrFieldBinder
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
			Expression expression = null;
			SyntaxTreeNode expression2 = node.GetExpression(false);
			string propertyOrFieldName = node.GetPropertyOrFieldName(true);
			bool useNullPropagation = node.GetUseNullPropagation(false);
			Type type = null;
			bool flag = false;
			if (bindingContext.TryResolveType(expression2, out type))
			{
				expression = null;
				flag = true;
			}
			else if (expression2 == null)
			{
				expression = bindingContext.Global;
				type = ((expression == null) ? null : expression.Type);
				flag = false;
				if (propertyOrFieldName != null)
				{
					if (propertyOrFieldName == "null")
					{
						boundExpression = ExpressionUtils.NullConstant;
						return true;
					}
					if (propertyOrFieldName == "true")
					{
						boundExpression = ExpressionUtils.TrueConstant;
						return true;
					}
					if (propertyOrFieldName == "false")
					{
						boundExpression = ExpressionUtils.TrueConstant;
						return false;
					}
				}
				if (bindingContext.TryGetParameter(propertyOrFieldName, out boundExpression))
				{
					return true;
				}
			}
			else if (AnyBinder.TryBind(expression2, bindingContext, TypeDescription.ObjectType, out expression, out bindingError))
			{
				type = expression.Type;
				flag = false;
			}
			else
			{
				expression = null;
				type = null;
			}
			if (expression == null && type == null)
			{
				if (bindingError == null)
				{
					bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVENAME, propertyOrFieldName), node);
				}
				return false;
			}
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(type);
			if (flag && type.IsEnum)
			{
				MemberDescription memberDescription = typeDescription.GetMembers(propertyOrFieldName).FirstOrDefault((MemberDescription m) => m.IsStatic);
				if (memberDescription == null)
				{
					bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVEMEMBERONTYPE, propertyOrFieldName, type), node);
					return false;
				}
				boundExpression = memberDescription.ConstantValueExpression;
			}
			else
			{
				foreach (MemberDescription memberDescription2 in typeDescription.GetMembers(propertyOrFieldName))
				{
					if (memberDescription2.IsStatic == flag && memberDescription2.IsPropertyOrField)
					{
						if (memberDescription2.TryMakeAccessor(expression, out boundExpression))
						{
							break;
						}
					}
				}
			}
			if (boundExpression == null)
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVEMEMBERONTYPE, propertyOrFieldName, type), node);
				return false;
			}
			if (useNullPropagation && flag)
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETOAPPLYNULLCONDITIONALOPERATORONTYPEREF, type), 0, 0, 0);
				return false;
			}
			if (useNullPropagation && typeDescription.CanBeNull)
			{
				bindingContext.RegisterNullPropagationTarger(expression);
			}
			return true;
		}
	}
}
