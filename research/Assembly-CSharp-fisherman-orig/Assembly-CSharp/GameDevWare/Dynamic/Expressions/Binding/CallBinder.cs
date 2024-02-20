using System;
using System.Linq.Expressions;
using System.Reflection;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class CallBinder
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
			ArgumentsTree arguments = node.GetArguments(false);
			object methodName = node.GetMethodName(true);
			bool useNullPropagation = node.GetUseNullPropagation(false);
			TypeReference typeReference = null;
			if (!BindingContext.TryGetMethodReference(methodName, out typeReference))
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVENAME, methodName), node);
				return false;
			}
			Type type = null;
			if (!CallBinder.TryBindTarget(node, bindingContext, out expression, out type, out bindingError))
			{
				return false;
			}
			bool flag = expression == null;
			float num = 0f;
			bool flag2 = typeReference.TypeArguments.Count > 0;
			Type[] array = null;
			if (flag2)
			{
				array = new Type[typeReference.TypeArguments.Count];
				for (int i = 0; i < array.Length; i++)
				{
					TypeReference typeReference2 = typeReference.TypeArguments[i];
					if (!bindingContext.TryResolveType(typeReference2, out array[i]))
					{
						bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVETYPE, typeReference2), node);
						return false;
					}
				}
			}
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(type);
			foreach (MemberDescription memberDescription in typeDescription.GetMembers(typeReference.Name))
			{
				if (memberDescription.IsMethod)
				{
					MemberDescription memberDescription2 = memberDescription;
					MethodInfo methodInfo = memberDescription;
					if (methodInfo != null && methodInfo.IsStatic == flag && methodInfo.IsGenericMethod == flag2)
					{
						if (!flag2 || memberDescription.GenericArgumentsCount == typeReference.TypeArguments.Count)
						{
							if (flag2)
							{
								try
								{
									memberDescription2 = memberDescription2.MakeGenericMethod(array);
									methodInfo = memberDescription2;
								}
								catch (ArgumentException ex)
								{
									bindingError = ex;
									goto IL_246;
								}
							}
							float num2 = 0f;
							Expression expression2 = null;
							if (memberDescription2.TryMakeCall(expression, arguments, bindingContext, out expression2, out num2))
							{
								if (!float.IsNaN(num2) && num2 > num)
								{
									boundExpression = expression2;
									num = num2;
									if (Math.Abs(num2 - 1f) < 1E-45f)
									{
										break;
									}
								}
							}
						}
					}
				}
				IL_246:;
			}
			if (bindingError != null)
			{
				return false;
			}
			if (boundExpression == null)
			{
				bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETOBINDCALL, typeReference.Name, type, arguments.Count), node);
				return false;
			}
			if (useNullPropagation && expression == null)
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

		private static bool TryBindTarget(SyntaxTreeNode node, BindingContext bindingContext, out Expression target, out Type type, out Exception bindingError)
		{
			type = null;
			target = null;
			bindingError = null;
			object obj = null;
			if (node.TryGetValue("expression", out obj))
			{
				if (obj is Expression)
				{
					target = (Expression)obj;
					type = target.Type;
					return true;
				}
				if (obj is Type)
				{
					target = null;
					type = (Type)obj;
					return true;
				}
			}
			SyntaxTreeNode expression = node.GetExpression(false);
			if (expression == null)
			{
				if (bindingContext.Global == null)
				{
					object methodName = node.GetMethodName(false);
					bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNABLETORESOLVENAME, methodName ?? "<unknown>"), node);
					return false;
				}
				target = bindingContext.Global;
				type = target.Type;
			}
			else if (bindingContext.TryResolveType(expression, out type))
			{
				target = null;
			}
			else
			{
				if (!CallBinder.TryBind(expression, bindingContext, null, out target, out bindingError))
				{
					return false;
				}
				type = target.Type;
			}
			return true;
		}
	}
}
