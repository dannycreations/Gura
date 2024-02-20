﻿using System;
using System.Linq.Expressions;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class GroupBinder
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
			SyntaxTreeNode expression = node.GetExpression(true);
			return AnyBinder.TryBindInNewScope(expression, bindingContext, expectedType, out boundExpression, out bindingError);
		}
	}
}
