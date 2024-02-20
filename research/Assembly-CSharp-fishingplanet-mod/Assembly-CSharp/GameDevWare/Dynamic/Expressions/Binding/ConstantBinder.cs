using System;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class ConstantBinder
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
			object value = node.GetValue(true);
			object typeName = node.GetTypeName(true);
			Type type = null;
			if (!bindingContext.TryResolveType(typeName, out type))
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVETYPE, typeName), node);
				return false;
			}
			object obj = ConstantBinder.ChangeType(value, type);
			boundExpression = Expression.Constant(obj);
			return true;
		}

		public static object ChangeType(object value, Type toType)
		{
			if (toType == null)
			{
				throw new ArgumentNullException("toType");
			}
			if (toType.IsEnum)
			{
				return Enum.Parse(toType, Convert.ToString(value, Constants.DefaultFormatProvider));
			}
			return Convert.ChangeType(value, toType, Constants.DefaultFormatProvider);
		}
	}
}
