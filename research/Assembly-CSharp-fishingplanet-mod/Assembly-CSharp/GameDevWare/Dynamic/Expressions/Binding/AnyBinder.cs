using System;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class AnyBinder
	{
		public static bool TryBindInNewScope(SyntaxTreeNode node, BindingContext bindingContext, TypeDescription expectedType, out Expression boundExpression, out Exception bindingError)
		{
			bindingContext = bindingContext.CreateNestedContext();
			bool flag = AnyBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
			bindingContext.CompleteNullPropagation(ref boundExpression);
			return flag;
		}

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
			bool flag;
			try
			{
				string expressionType = node.GetExpressionType(true);
				switch (expressionType)
				{
				case "PropertyOrField":
					return PropertyOrFieldBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Constant":
					return ConstantBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Call":
					return CallBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Enclose":
				case "UncheckedScope":
				case "CheckedScope":
				case "Group":
					return GroupBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Invoke":
					return InvokeBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Lambda":
					return LambdaBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Index":
					return IndexBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "TypeOf":
					return TypeOfBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Convert":
				case "ConvertChecked":
				case "TypeIs":
				case "TypeAs":
					return TypeBinaryBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Default":
					return DefaultBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "New":
					return NewBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "NewArrayBounds":
					return NewArrayBoundsBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Add":
				case "AddChecked":
				case "Subtract":
				case "SubtractChecked":
				case "LeftShift":
				case "RightShift":
				case "GreaterThan":
				case "GreaterThanOrEqual":
				case "LessThan":
				case "LessThanOrEqual":
				case "Power":
				case "Divide":
				case "Multiply":
				case "MultiplyChecked":
				case "Modulo":
				case "Equal":
				case "NotEqual":
				case "And":
				case "Or":
				case "ExclusiveOr":
				case "AndAlso":
				case "OrElse":
				case "Coalesce":
					return BinaryBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Negate":
				case "NegateChecked":
				case "Complement":
				case "Not":
				case "UnaryPlus":
					return UnaryBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				case "Condition":
					return ConditionBinder.TryBind(node, bindingContext, expectedType, out boundExpression, out bindingError);
				}
				boundExpression = null;
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, expressionType), node);
				flag = false;
			}
			catch (ExpressionParserException ex)
			{
				boundExpression = null;
				bindingError = ex;
				flag = false;
			}
			catch (Exception ex2)
			{
				boundExpression = null;
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_FAILEDTOBIND, node.GetExpressionType(false) ?? "<unknown>", ex2.Message), ex2, node);
				flag = false;
			}
			return flag;
		}
	}
}
