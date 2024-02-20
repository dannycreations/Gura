using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal static class BinaryBinder
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
			string expressionType = node.GetExpressionType(true);
			SyntaxTreeNode leftExpression = node.GetLeftExpression(true);
			SyntaxTreeNode rightExpression = node.GetRightExpression(true);
			Expression expression = null;
			Expression expression2 = null;
			if (!AnyBinder.TryBindInNewScope(leftExpression, bindingContext, TypeDescription.ObjectType, out expression, out bindingError))
			{
				return false;
			}
			if (!AnyBinder.TryBindInNewScope(rightExpression, bindingContext, TypeDescription.ObjectType, out expression2, out bindingError))
			{
				return false;
			}
			if (expressionType != null)
			{
				if (BinaryBinder.<>f__switch$map2 == null)
				{
					BinaryBinder.<>f__switch$map2 = new Dictionary<string, int>(23)
					{
						{ "Add", 0 },
						{ "AddChecked", 1 },
						{ "Subtract", 2 },
						{ "SubtractChecked", 3 },
						{ "LeftShift", 4 },
						{ "RightShift", 5 },
						{ "GreaterThan", 6 },
						{ "GreaterThanOrEqual", 7 },
						{ "LessThan", 8 },
						{ "LessThanOrEqual", 9 },
						{ "Power", 10 },
						{ "Divide", 11 },
						{ "Multiply", 12 },
						{ "MultiplyChecked", 13 },
						{ "Modulo", 14 },
						{ "Equal", 15 },
						{ "NotEqual", 16 },
						{ "And", 17 },
						{ "Or", 18 },
						{ "ExclusiveOr", 19 },
						{ "AndAlso", 20 },
						{ "OrElse", 21 },
						{ "Coalesce", 22 }
					};
				}
				int num;
				if (BinaryBinder.<>f__switch$map2.TryGetValue(expressionType, out num))
				{
					switch (num)
					{
					case 0:
						if (expression.Type == typeof(string) || expression2.Type == typeof(string))
						{
							boundExpression = Expression.Call(BinaryBinder.StringConcat.Method, new Expression[]
							{
								Expression.Convert(expression, typeof(object)),
								Expression.Convert(expression2, typeof(object))
							});
						}
						else if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.Add, out boundExpression))
						{
							boundExpression = Expression.Add(expression, expression2);
						}
						break;
					case 1:
						if (expression.Type == typeof(string) || expression2.Type == typeof(string))
						{
							boundExpression = Expression.Call(BinaryBinder.StringConcat.Method, new Expression[]
							{
								Expression.Convert(expression, typeof(object)),
								Expression.Convert(expression2, typeof(object))
							});
						}
						else if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.AddChecked, out boundExpression))
						{
							boundExpression = Expression.AddChecked(expression, expression2);
						}
						break;
					case 2:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.Subtract, out boundExpression))
						{
							boundExpression = Expression.Subtract(expression, expression2);
						}
						break;
					case 3:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.SubtractChecked, out boundExpression))
						{
							boundExpression = Expression.SubtractChecked(expression, expression2);
						}
						break;
					case 4:
						ExpressionUtils.TryPromoteUnaryOperation(ref expression, ExpressionType.LeftShift, out boundExpression);
						ExpressionUtils.TryPromoteUnaryOperation(ref expression2, ExpressionType.LeftShift, out boundExpression);
						boundExpression = Expression.LeftShift(expression, expression2);
						break;
					case 5:
						ExpressionUtils.TryPromoteUnaryOperation(ref expression, ExpressionType.RightShift, out boundExpression);
						ExpressionUtils.TryPromoteUnaryOperation(ref expression2, ExpressionType.RightShift, out boundExpression);
						boundExpression = Expression.RightShift(expression, expression2);
						break;
					case 6:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.GreaterThan, out boundExpression))
						{
							boundExpression = Expression.GreaterThan(expression, expression2);
						}
						break;
					case 7:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.GreaterThanOrEqual, out boundExpression))
						{
							boundExpression = Expression.GreaterThanOrEqual(expression, expression2);
						}
						break;
					case 8:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.LessThan, out boundExpression))
						{
							boundExpression = Expression.LessThan(expression, expression2);
						}
						break;
					case 9:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.LessThanOrEqual, out boundExpression))
						{
							boundExpression = Expression.LessThanOrEqual(expression, expression2);
						}
						break;
					case 10:
					{
						TypeDescription typeDescription = TypeDescription.GetTypeDescription(expression.Type);
						TypeDescription typeDescription2 = ((!typeDescription.IsNullable) ? typeDescription : typeDescription.UnderlyingType);
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.Power, out boundExpression))
						{
							TypeDescription typeDescription3 = TypeDescription.GetTypeDescription(expression.Type);
							TypeDescription typeDescription4 = ((!typeDescription3.IsNullable) ? typeDescription3 : typeDescription3.UnderlyingType);
							bool flag = typeDescription.IsNullable || typeDescription3.IsNullable;
							if (typeDescription4 != typeof(double) && expression.Type == expression2.Type)
							{
								expression = Expression.ConvertChecked(expression, (!flag) ? typeof(double) : typeof(double?));
								expression2 = Expression.ConvertChecked(expression2, (!flag) ? typeof(double) : typeof(double?));
							}
							boundExpression = Expression.Power(expression, expression2);
							if (typeDescription != typeof(double))
							{
								boundExpression = Expression.ConvertChecked(boundExpression, (!flag) ? typeDescription2 : typeDescription2.GetNullableType());
							}
						}
						break;
					}
					case 11:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.Divide, out boundExpression))
						{
							boundExpression = Expression.Divide(expression, expression2);
						}
						break;
					case 12:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.Multiply, out boundExpression))
						{
							boundExpression = Expression.Multiply(expression, expression2);
						}
						break;
					case 13:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.MultiplyChecked, out boundExpression))
						{
							boundExpression = Expression.MultiplyChecked(expression, expression2);
						}
						break;
					case 14:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.Modulo, out boundExpression))
						{
							boundExpression = Expression.Modulo(expression, expression2);
						}
						break;
					case 15:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.Equal, out boundExpression))
						{
							boundExpression = Expression.Equal(expression, expression2);
						}
						break;
					case 16:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.NotEqual, out boundExpression))
						{
							boundExpression = Expression.NotEqual(expression, expression2);
						}
						break;
					case 17:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.And, out boundExpression))
						{
							boundExpression = Expression.And(expression, expression2);
						}
						break;
					case 18:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.Or, out boundExpression))
						{
							boundExpression = Expression.Or(expression, expression2);
						}
						break;
					case 19:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.ExclusiveOr, out boundExpression))
						{
							boundExpression = Expression.ExclusiveOr(expression, expression2);
						}
						break;
					case 20:
						boundExpression = Expression.AndAlso(expression, expression2);
						break;
					case 21:
						boundExpression = Expression.OrElse(expression, expression2);
						break;
					case 22:
						if (!ExpressionUtils.TryPromoteBinaryOperation(ref expression, ref expression2, ExpressionType.Coalesce, out boundExpression))
						{
							boundExpression = Expression.Coalesce(expression, expression2);
						}
						break;
					case 23:
						goto IL_71D;
					default:
						goto IL_71D;
					}
					return true;
				}
			}
			IL_71D:
			bindingError = new ExpressionParserException(string.Format(Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE, expressionType), node);
			return false;
		}

		private static readonly Func<object, object, string> StringConcat = new Func<object, object, string>(string.Concat);
	}
}
