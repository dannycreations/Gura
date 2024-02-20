using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class LambdaBinder
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
			if (expectedType.HasGenericParameters || !expectedType.IsDelegate)
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_VALIDDELEGATETYPEISEXPECTED, (!(expectedType != null)) ? "<null>" : expectedType.ToString()), 0, 0, 0);
				return false;
			}
			string expressionType = node.GetExpressionType(true);
			SyntaxTreeNode expression = node.GetExpression(true);
			ArgumentsTree arguments = node.GetArguments(false);
			MemberDescription memberDescription = expectedType.GetMembers("Invoke").FirstOrDefault((MemberDescription m) => m.IsMethod && !m.IsStatic);
			if (memberDescription == null)
			{
				bindingError = new MissingMethodException(expectedType.ToString(), "Invoke");
				return false;
			}
			if (memberDescription.GetParametersCount() != arguments.Count)
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_INVALIDLAMBDAARGUMENTS, expectedType), 0, 0, 0);
				return false;
			}
			string[] array = new string[arguments.Count];
			for (int i = 0; i < array.Length; i++)
			{
				SyntaxTreeNode syntaxTreeNode = null;
				if (!arguments.TryGetValue(i, out syntaxTreeNode) || syntaxTreeNode == null || syntaxTreeNode.GetExpressionType(true) != "PropertyOrField")
				{
					bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "expression", expressionType), node);
					return false;
				}
				array[i] = syntaxTreeNode.GetPropertyOrFieldName(true);
			}
			ParameterExpression[] array2 = new ParameterExpression[arguments.Count];
			for (int j = 0; j < arguments.Count; j++)
			{
				array2[j] = Expression.Parameter(memberDescription.GetParameter(j).ParameterType, array[j]);
			}
			ReadOnlyCollection<ParameterExpression> parameters = bindingContext.Parameters;
			List<ParameterExpression> list = new List<ParameterExpression>(array2.Length + parameters.Count);
			list.AddRange(array2);
			foreach (ParameterExpression parameterExpression in parameters)
			{
				if (Array.IndexOf<string>(array, parameterExpression.Name) < 0)
				{
					list.Add(parameterExpression);
				}
			}
			BindingContext bindingContext2 = bindingContext.CreateNestedContext(list.AsReadOnly(), memberDescription.ResultType);
			Expression expression2 = null;
			if (!AnyBinder.TryBindInNewScope(expression, bindingContext2, TypeDescription.GetTypeDescription(memberDescription.ResultType), out expression2, out bindingError))
			{
				return false;
			}
			boundExpression = Expression.Lambda(expectedType, expression2, array2);
			return true;
		}

		public static string[] ExtractArgumentNames(SyntaxTreeNode node)
		{
			ArgumentsTree arguments = node.GetArguments(false);
			string[] array = new string[arguments.Count];
			for (int i = 0; i < array.Length; i++)
			{
				SyntaxTreeNode syntaxTreeNode = null;
				if (!arguments.TryGetValue(i, out syntaxTreeNode) || syntaxTreeNode == null || syntaxTreeNode.GetExpressionType(true) != "PropertyOrField")
				{
					throw new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_MISSINGATTRONNODE, "expression", "Lambda"), node);
				}
				array[i] = syntaxTreeNode.GetPropertyOrFieldName(true);
			}
			return array;
		}
	}
}
