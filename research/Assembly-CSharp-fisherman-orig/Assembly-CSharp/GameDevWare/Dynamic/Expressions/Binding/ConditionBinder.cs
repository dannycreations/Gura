using System;
using System.Linq.Expressions;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class ConditionBinder
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
			bindingError = null;
			boundExpression = null;
			SyntaxTreeNode testExpression = node.GetTestExpression(true);
			SyntaxTreeNode ifTrueExpression = node.GetIfTrueExpression(true);
			SyntaxTreeNode ifFalseExpression = node.GetIfFalseExpression(true);
			Expression expression = null;
			Expression expression2 = null;
			Expression expression3 = null;
			if (!AnyBinder.TryBindInNewScope(testExpression, bindingContext, TypeDescription.GetTypeDescription(typeof(bool)), out expression, out bindingError))
			{
				return false;
			}
			if (!AnyBinder.TryBindInNewScope(ifTrueExpression, bindingContext, TypeDescription.ObjectType, out expression2, out bindingError))
			{
				return false;
			}
			if (!AnyBinder.TryBindInNewScope(ifFalseExpression, bindingContext, TypeDescription.ObjectType, out expression3, out bindingError))
			{
				return false;
			}
			if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression2, ref expression3, ExpressionType.Conditional, out boundExpression))
			{
				if (expression2.Type != expression3.Type)
				{
					float num;
					ExpressionUtils.TryMorphType(ref expression2, expression3.Type, out num);
				}
				boundExpression = Expression.Condition(expression, expression2, expression3);
			}
			return true;
		}
	}
}
