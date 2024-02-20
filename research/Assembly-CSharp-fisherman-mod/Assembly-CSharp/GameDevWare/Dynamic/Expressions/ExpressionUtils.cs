using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.Binding;

namespace GameDevWare.Dynamic.Expressions
{
	internal static class ExpressionUtils
	{
		public static bool TryPromoteBinaryOperation(ref Expression leftOperand, ref Expression rightOperand, ExpressionType type, out Expression operation)
		{
			if (leftOperand == null)
			{
				throw new ArgumentNullException("leftOperand");
			}
			if (rightOperand == null)
			{
				throw new ArgumentNullException("rightOperand");
			}
			operation = null;
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(leftOperand.Type);
			TypeDescription typeDescription2 = TypeDescription.GetTypeDescription(rightOperand.Type);
			TypeDescription typeDescription3 = ((!typeDescription.IsNullable) ? typeDescription : typeDescription.UnderlyingType);
			TypeDescription typeDescription4 = ((!typeDescription2.IsNullable) ? typeDescription2 : typeDescription2.UnderlyingType);
			if (typeDescription3.IsEnum || typeDescription4.IsEnum)
			{
				return ExpressionUtils.TryPromoteEnumBinaryOperation(ref leftOperand, typeDescription, ref rightOperand, typeDescription2, type, out operation);
			}
			if (typeDescription3.IsNumber && typeDescription4.IsNumber)
			{
				return ExpressionUtils.TryPromoteNumberBinaryOperation(ref leftOperand, typeDescription, ref rightOperand, typeDescription2, type, out operation);
			}
			if (ExpressionUtils.IsNull(leftOperand, true) && typeDescription2.CanBeNull)
			{
				leftOperand = typeDescription2.DefaultExpression;
			}
			else if (ExpressionUtils.IsNull(rightOperand, true) && typeDescription.CanBeNull)
			{
				rightOperand = typeDescription.DefaultExpression;
			}
			else if (typeDescription.IsNullable != typeDescription2.IsNullable)
			{
				leftOperand = ExpressionUtils.ConvertToNullable(leftOperand, typeDescription);
				if (type != ExpressionType.Coalesce)
				{
					rightOperand = ExpressionUtils.ConvertToNullable(rightOperand, typeDescription2);
				}
			}
			return false;
		}

		private static bool TryPromoteNumberBinaryOperation(ref Expression leftOperand, TypeDescription leftType, ref Expression rightOperand, TypeDescription rightType, ExpressionType type, out Expression operation)
		{
			if (leftOperand == null)
			{
				throw new ArgumentNullException("leftOperand");
			}
			if (leftType == null)
			{
				throw new ArgumentNullException("leftType");
			}
			if (rightOperand == null)
			{
				throw new ArgumentNullException("rightOperand");
			}
			if (rightType == null)
			{
				throw new ArgumentNullException("rightType");
			}
			operation = null;
			TypeDescription typeDescription = ((!leftType.IsNullable) ? leftType : leftType.UnderlyingType);
			TypeDescription typeDescription2 = ((!rightType.IsNullable) ? rightType : rightType.UnderlyingType);
			TypeCode typeCode = typeDescription.TypeCode;
			TypeCode typeCode2 = typeDescription2.TypeCode;
			bool flag = leftType.IsNullable || rightType.IsNullable;
			bool flag2 = rightType.IsNullable || (type != ExpressionType.Coalesce && flag);
			if (typeDescription == typeDescription2)
			{
				if (typeCode >= TypeCode.SByte && typeCode <= TypeCode.UInt16)
				{
					leftOperand = Expression.Convert(leftOperand, (!flag) ? TypeDescription.Int32Type : TypeDescription.Int32Type.GetNullableType());
					rightOperand = Expression.Convert(rightOperand, (!flag2) ? TypeDescription.Int32Type : TypeDescription.Int32Type.GetNullableType());
					return false;
				}
			}
			else if (typeCode == TypeCode.Decimal || typeCode2 == TypeCode.Decimal)
			{
				if (typeCode == TypeCode.Double || typeCode == TypeCode.Single || typeCode2 == TypeCode.Double || typeCode2 == TypeCode.Single)
				{
					return false;
				}
				if (typeCode == TypeCode.Decimal)
				{
					rightOperand = Expression.Convert(rightOperand, (!flag2) ? typeof(decimal) : typeof(decimal?));
				}
				else
				{
					leftOperand = Expression.Convert(leftOperand, (!flag) ? typeof(decimal) : typeof(decimal?));
				}
			}
			else if (typeCode == TypeCode.Double || typeCode2 == TypeCode.Double)
			{
				if (typeCode == TypeCode.Double)
				{
					rightOperand = Expression.Convert(rightOperand, (!flag2) ? typeof(double) : typeof(double?));
				}
				else
				{
					leftOperand = Expression.Convert(leftOperand, (!flag) ? typeof(double) : typeof(double?));
				}
			}
			else if (typeCode == TypeCode.Single || typeCode2 == TypeCode.Single)
			{
				if (typeCode == TypeCode.Single)
				{
					rightOperand = Expression.Convert(rightOperand, (!flag2) ? typeof(float) : typeof(float?));
				}
				else
				{
					leftOperand = Expression.Convert(leftOperand, (!flag) ? typeof(float) : typeof(float?));
				}
			}
			else if (typeCode == TypeCode.UInt64)
			{
				float num = 0f;
				Expression expression = rightOperand;
				Type type2 = ((!flag2) ? typeof(ulong) : typeof(ulong?));
				if (NumberUtils.IsSignedInteger(typeCode2) && !ExpressionUtils.TryMorphType(ref expression, type2, out num))
				{
					return false;
				}
				rightOperand = expression;
				rightOperand = ((rightOperand.Type == type2) ? rightOperand : Expression.Convert(rightOperand, type2));
			}
			else if (typeCode2 == TypeCode.UInt64)
			{
				float num2 = 0f;
				Expression expression2 = leftOperand;
				Type type3 = ((!flag) ? typeof(ulong) : typeof(ulong?));
				if (NumberUtils.IsSignedInteger(typeCode) && !ExpressionUtils.TryMorphType(ref expression2, type3, out num2))
				{
					return false;
				}
				leftOperand = expression2;
				leftOperand = ((leftOperand.Type == type3) ? leftOperand : Expression.Convert(leftOperand, type3));
			}
			else if (typeCode == TypeCode.Int64 || typeCode2 == TypeCode.Int64)
			{
				if (typeCode == TypeCode.Int64)
				{
					rightOperand = Expression.Convert(rightOperand, (!flag2) ? typeof(long) : typeof(long?));
				}
				else
				{
					leftOperand = Expression.Convert(leftOperand, (!flag) ? typeof(long) : typeof(long?));
				}
			}
			else if ((typeCode == TypeCode.UInt32 && NumberUtils.IsSignedInteger(typeCode2)) || (typeCode2 == TypeCode.UInt32 && NumberUtils.IsSignedInteger(typeCode)))
			{
				rightOperand = Expression.Convert(rightOperand, (!flag2) ? typeof(long) : typeof(long?));
				leftOperand = Expression.Convert(leftOperand, (!flag) ? typeof(long) : typeof(long?));
			}
			else if (typeCode == TypeCode.UInt32 || typeCode2 == TypeCode.UInt32)
			{
				if (typeCode == TypeCode.UInt32)
				{
					rightOperand = Expression.Convert(rightOperand, (!flag2) ? typeof(uint) : typeof(uint?));
				}
				else
				{
					leftOperand = Expression.Convert(leftOperand, (!flag) ? typeof(uint) : typeof(uint?));
				}
			}
			else
			{
				rightOperand = Expression.Convert(rightOperand, (!flag2) ? typeof(int) : typeof(int?));
				leftOperand = Expression.Convert(leftOperand, (!flag) ? typeof(int) : typeof(int?));
			}
			if (flag)
			{
				leftOperand = ExpressionUtils.ConvertToNullable(leftOperand, null);
			}
			if (flag2)
			{
				rightOperand = ExpressionUtils.ConvertToNullable(rightOperand, null);
			}
			return false;
		}

		private static bool TryPromoteEnumBinaryOperation(ref Expression leftOperand, TypeDescription leftType, ref Expression rightOperand, TypeDescription rightType, ExpressionType type, out Expression operation)
		{
			if (leftOperand == null)
			{
				throw new ArgumentNullException("leftOperand");
			}
			if (leftType == null)
			{
				throw new ArgumentNullException("leftType");
			}
			if (rightOperand == null)
			{
				throw new ArgumentNullException("rightOperand");
			}
			if (rightType == null)
			{
				throw new ArgumentNullException("rightType");
			}
			operation = null;
			TypeDescription typeDescription = ((!leftType.IsNullable) ? leftType : leftType.UnderlyingType);
			TypeDescription typeDescription2 = ((!rightType.IsNullable) ? rightType : rightType.UnderlyingType);
			bool flag = leftType.IsNullable != rightType.IsNullable;
			if (typeDescription.IsEnum && typeDescription2.IsNumber && (type == ExpressionType.Add || type == ExpressionType.AddChecked || type == ExpressionType.Subtract || type == ExpressionType.SubtractChecked))
			{
				TypeDescription underlyingType = typeDescription.UnderlyingType;
				leftOperand = Expression.Convert(leftOperand, (!flag) ? underlyingType : underlyingType.GetNullableType());
				if (flag)
				{
					rightOperand = ExpressionUtils.ConvertToNullable(rightOperand, leftType);
				}
				if (type != ExpressionType.Add)
				{
					if (type != ExpressionType.AddChecked)
					{
						if (type != ExpressionType.Subtract)
						{
							if (type != ExpressionType.SubtractChecked)
							{
								throw new InvalidOperationException("Only subtraction and addition with numbers are promoted.");
							}
							operation = Expression.SubtractChecked(leftOperand, rightOperand);
						}
						else
						{
							operation = Expression.Subtract(leftOperand, rightOperand);
						}
					}
					else
					{
						operation = Expression.AddChecked(leftOperand, rightOperand);
					}
				}
				else
				{
					operation = Expression.Add(leftOperand, rightOperand);
				}
				operation = Expression.Convert(operation, (!flag) ? typeDescription : typeDescription.GetNullableType());
				return true;
			}
			if (typeDescription2.IsEnum && typeDescription.IsNumber && (type == ExpressionType.Add || type == ExpressionType.AddChecked || type == ExpressionType.Subtract || type == ExpressionType.SubtractChecked))
			{
				TypeDescription underlyingType2 = typeDescription2.UnderlyingType;
				rightOperand = Expression.ConvertChecked(rightOperand, (!flag) ? underlyingType2 : underlyingType2.GetNullableType());
				if (flag)
				{
					leftOperand = ExpressionUtils.ConvertToNullable(leftOperand, rightType);
				}
				operation = Expression.MakeBinary(type, leftOperand, rightOperand);
				operation = Expression.Convert(operation, (!flag) ? typeDescription2 : typeDescription2.GetNullableType());
				return true;
			}
			if (ExpressionUtils.IsNull(leftOperand, true) && rightType.CanBeNull)
			{
				leftType = rightType;
				leftOperand = rightType.DefaultExpression;
			}
			else if (ExpressionUtils.IsNull(rightOperand, true) && leftType.CanBeNull)
			{
				rightType = leftType;
				rightOperand = leftType.DefaultExpression;
			}
			else
			{
				if (typeDescription2 == typeDescription && (type == ExpressionType.And || type == ExpressionType.Or || type == ExpressionType.ExclusiveOr || type == ExpressionType.GreaterThan || type == ExpressionType.GreaterThanOrEqual || type == ExpressionType.LessThan || type == ExpressionType.LessThanOrEqual))
				{
					TypeDescription underlyingType3 = typeDescription2.UnderlyingType;
					rightOperand = Expression.ConvertChecked(rightOperand, (!flag) ? underlyingType3 : underlyingType3.GetNullableType());
					leftOperand = Expression.Convert(leftOperand, (!flag) ? underlyingType3 : underlyingType3.GetNullableType());
					operation = Expression.MakeBinary(type, leftOperand, rightOperand);
					return true;
				}
				if (flag)
				{
					leftOperand = ExpressionUtils.ConvertToNullable(leftOperand, leftType);
					if (type != ExpressionType.Coalesce)
					{
						rightOperand = ExpressionUtils.ConvertToNullable(rightOperand, rightType);
					}
				}
			}
			return false;
		}

		public static bool TryPromoteUnaryOperation(ref Expression operand, ExpressionType type, out Expression operation)
		{
			if (operand == null)
			{
				throw new ArgumentNullException("operand");
			}
			operation = null;
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(operand.Type);
			TypeDescription typeDescription2 = ((!typeDescription.IsNullable) ? typeDescription : typeDescription.UnderlyingType);
			bool isNullable = typeDescription.IsNullable;
			if (typeDescription2.IsEnum)
			{
				ExpressionUtils.MorphType(ref operand, (!isNullable) ? typeDescription2.UnderlyingType : typeDescription2.UnderlyingType.GetNullableType());
			}
			else if (typeDescription2.TypeCode >= TypeCode.SByte && typeDescription2.TypeCode <= TypeCode.UInt16)
			{
				ExpressionUtils.MorphType(ref operand, (!isNullable) ? typeof(int) : typeof(int?));
			}
			else if (typeDescription2.TypeCode == TypeCode.UInt32 && type == ExpressionType.Not)
			{
				ExpressionUtils.MorphType(ref operand, (!isNullable) ? typeof(long) : typeof(long?));
			}
			return false;
		}

		public static bool IsNull(Expression expression, bool unwrapConversions = true)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			if (object.ReferenceEquals(expression, ExpressionUtils.NullConstant))
			{
				return true;
			}
			ConstantExpression constantExpression = null;
			return ExpressionUtils.TryExposeConstant(expression, out constantExpression) && (object.ReferenceEquals(constantExpression, ExpressionUtils.NullConstant) || (constantExpression.Value == null && constantExpression.Type == typeof(object)));
		}

		public static void MorphType(ref Expression expression, Type toType)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			if (toType == null)
			{
				throw new ArgumentNullException("toType");
			}
			float num = 0f;
			if (!ExpressionUtils.TryMorphType(ref expression, toType, out num) || num <= 0f)
			{
				throw new InvalidOperationException(string.Format("Failed to change type of expression '{0}' to '{1}'.", expression, toType));
			}
		}

		public static bool TryMorphType(ref Expression expression, Type toType, out float quality)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			if (toType == null)
			{
				throw new ArgumentNullException("toType");
			}
			if (expression.Type == toType)
			{
				quality = 1f;
				return true;
			}
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(expression.Type);
			TypeDescription typeDescription2 = TypeDescription.GetTypeDescription(toType);
			if (ExpressionUtils.TryConvertInPlace(ref expression, typeDescription2, out quality) || ExpressionUtils.TryFindConversion(ref expression, typeDescription, typeDescription2, out quality))
			{
				return true;
			}
			quality = 0f;
			return false;
		}

		private static bool TryFindConversion(ref Expression expression, TypeDescription actualType, TypeDescription targetType, out float quality)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			if (actualType == null)
			{
				throw new ArgumentNullException("actualType");
			}
			if (targetType == null)
			{
				throw new ArgumentNullException("targetType");
			}
			quality = 0f;
			TypeDescription typeDescription = ((!actualType.IsNullable) ? actualType : actualType.UnderlyingType);
			TypeDescription typeDescription2 = ((!targetType.IsNullable) ? targetType : targetType.UnderlyingType);
			if (targetType.CanBeNull && ExpressionUtils.IsNull(expression, true))
			{
				expression = targetType.DefaultExpression;
				quality = 1f;
				return true;
			}
			if (typeDescription2 == actualType)
			{
				expression = Expression.Convert(expression, targetType);
				quality = 0.7f;
				return true;
			}
			TypeConversion typeConversion = null;
			if (!TypeConversion.TryGetTypeConversion(typeDescription, typeDescription2, out typeConversion) || typeConversion.Quality <= 0f)
			{
				return false;
			}
			if (typeConversion.Implicit != null && typeConversion.Implicit.TryMakeConversion(expression, out expression, true))
			{
				quality = 0.5f;
				return true;
			}
			expression = Expression.Convert(expression, targetType);
			quality = typeConversion.Quality;
			return true;
		}

		private static bool TryConvertInPlace(ref Expression expression, TypeDescription targetType, out float quality)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			if (targetType == null)
			{
				throw new ArgumentNullException("targetType");
			}
			quality = 0f;
			TypeDescription typeDescription = ((!targetType.IsNullable) ? targetType : targetType.UnderlyingType);
			ConstantExpression constantExpression = null;
			if (!ExpressionUtils.TryExposeConstant(expression, out constantExpression))
			{
				return false;
			}
			object value = constantExpression.Value;
			TypeDescription typeDescription2 = TypeDescription.GetTypeDescription(constantExpression.Type);
			TypeDescription typeDescription3 = ((!typeDescription2.IsNullable) ? typeDescription2 : typeDescription2.UnderlyingType);
			if (value == null && typeDescription2.DefaultExpression != constantExpression && targetType.CanBeNull)
			{
				quality = 1f;
				expression = Expression.Constant(null, targetType);
				return true;
			}
			if (value == null)
			{
				return false;
			}
			TypeCode typeCode = typeDescription3.TypeCode;
			bool flag;
			switch (typeDescription.TypeCode)
			{
			case TypeCode.Char:
			case TypeCode.UInt16:
				flag = ExpressionUtils.IsInRange(value, typeCode, 0L, 65535UL);
				break;
			case TypeCode.SByte:
				flag = ExpressionUtils.IsInRange(value, typeCode, -128L, 127UL);
				break;
			case TypeCode.Byte:
				flag = ExpressionUtils.IsInRange(value, typeCode, 0L, 255UL);
				break;
			case TypeCode.Int16:
				flag = ExpressionUtils.IsInRange(value, typeCode, -32768L, 32767UL);
				break;
			case TypeCode.Int32:
				flag = ExpressionUtils.IsInRange(value, typeCode, -2147483648L, 2147483647UL);
				break;
			case TypeCode.UInt32:
				flag = ExpressionUtils.IsInRange(value, typeCode, 0L, (ulong)(-1));
				break;
			case TypeCode.Int64:
				flag = ExpressionUtils.IsInRange(value, typeCode, long.MinValue, 9223372036854775807UL);
				break;
			case TypeCode.UInt64:
				flag = ExpressionUtils.IsInRange(value, typeCode, 0L, ulong.MaxValue);
				break;
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				flag = NumberUtils.IsSignedInteger(typeCode) || NumberUtils.IsUnsignedInteger(typeCode);
				break;
			default:
				flag = false;
				break;
			}
			if (!flag)
			{
				return false;
			}
			object obj = Convert.ChangeType(value, typeDescription.TypeCode, Constants.DefaultFormatProvider);
			expression = Expression.Constant(obj, targetType);
			quality = 0.7f;
			return true;
		}

		private static Expression ConvertToNullable(Expression notNullableExpression, TypeDescription typeDescription)
		{
			if (notNullableExpression == null)
			{
				throw new ArgumentNullException("notNullableExpression");
			}
			if (typeDescription != null && notNullableExpression.Type != typeDescription)
			{
				throw new ArgumentException("Wrong type description.", "typeDescription");
			}
			if (typeDescription == null)
			{
				typeDescription = TypeDescription.GetTypeDescription(notNullableExpression.Type);
			}
			if (!typeDescription.CanBeNull)
			{
				return Expression.Convert(notNullableExpression, typeDescription.GetNullableType());
			}
			return notNullableExpression;
		}

		public static Expression MakeNullPropagationExpression(List<Expression> nullTestExpressions, Expression ifNotNullExpression)
		{
			if (nullTestExpressions == null)
			{
				throw new ArgumentNullException("nullTestExpressions");
			}
			if (ifNotNullExpression == null)
			{
				throw new ArgumentNullException("ifNotNullExpression");
			}
			Expression expression = null;
			foreach (Expression expression2 in nullTestExpressions)
			{
				TypeDescription typeDescription = TypeDescription.GetTypeDescription(expression2.Type);
				BinaryExpression binaryExpression = Expression.NotEqual(expression2, typeDescription.DefaultExpression);
				if (expression == null)
				{
					expression = binaryExpression;
				}
				else
				{
					expression = Expression.AndAlso(expression, binaryExpression);
				}
			}
			if (expression == null)
			{
				expression = ExpressionUtils.TrueConstant;
			}
			TypeDescription typeDescription2 = TypeDescription.GetTypeDescription(ifNotNullExpression.Type);
			TypeDescription typeDescription3 = (typeDescription2.CanBeNull ? typeDescription2 : TypeDescription.GetTypeDescription(typeof(Nullable<>).MakeGenericType(new Type[] { ifNotNullExpression.Type })));
			if (typeDescription3 != ifNotNullExpression.Type)
			{
				ifNotNullExpression = Expression.Convert(ifNotNullExpression, typeDescription3);
			}
			return Expression.Condition(expression, ifNotNullExpression, typeDescription3.DefaultExpression);
		}

		public static bool ExtractNullPropagationExpression(ConditionalExpression conditionalExpression, out List<Expression> nullTestExpressions, out Expression ifNotNullExpression)
		{
			if (conditionalExpression == null)
			{
				throw new ArgumentNullException("conditionalExpression");
			}
			nullTestExpressions = null;
			ifNotNullExpression = null;
			if (!ExpressionUtils.TryExtractTestTargets(conditionalExpression.Test, ref nullTestExpressions))
			{
				return false;
			}
			if (nullTestExpressions == null || nullTestExpressions.Count == 0)
			{
				return false;
			}
			ConstantExpression constantExpression = conditionalExpression.IfFalse as ConstantExpression;
			Expression expression = ((conditionalExpression.IfTrue.NodeType != ExpressionType.Convert) ? conditionalExpression.IfTrue : ((UnaryExpression)conditionalExpression.IfTrue).Operand);
			if (constantExpression == null || constantExpression.Value != null || !ExpressionLookupVisitor.Lookup(conditionalExpression.IfTrue, nullTestExpressions))
			{
				return false;
			}
			ifNotNullExpression = expression;
			return true;
		}

		private static bool TryExtractTestTargets(Expression testExpression, ref List<Expression> nullTestExpressions)
		{
			if (testExpression == null)
			{
				throw new ArgumentNullException("testExpression");
			}
			if (testExpression.NodeType == ExpressionType.NotEqual)
			{
				BinaryExpression binaryExpression = (BinaryExpression)testExpression;
				ConstantExpression constantExpression = binaryExpression.Right as ConstantExpression;
				object obj = ((constantExpression == null) ? null : constantExpression.Value);
				if (binaryExpression.Left.Type != binaryExpression.Right.Type || constantExpression == null || obj != null)
				{
					return false;
				}
				if (nullTestExpressions == null)
				{
					nullTestExpressions = new List<Expression>();
				}
				nullTestExpressions.Add(binaryExpression.Left);
				return true;
			}
			else
			{
				if (testExpression.NodeType == ExpressionType.AndAlso)
				{
					BinaryExpression binaryExpression2 = (BinaryExpression)testExpression;
					return ExpressionUtils.TryExtractTestTargets(binaryExpression2.Left, ref nullTestExpressions) && ExpressionUtils.TryExtractTestTargets(binaryExpression2.Right, ref nullTestExpressions);
				}
				return false;
			}
		}

		private static bool TryExposeConstant(Expression expression, out ConstantExpression constantExpression)
		{
			UnaryExpression unaryExpression = expression as UnaryExpression;
			while (unaryExpression != null && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked))
			{
				expression = unaryExpression.Operand;
				unaryExpression = expression as UnaryExpression;
			}
			constantExpression = null;
			constantExpression = expression as ConstantExpression;
			return constantExpression != null;
		}

		private static bool IsInRange(object value, TypeCode valueTypeCode, long minValue, ulong maxValue)
		{
			if (NumberUtils.IsSignedInteger(valueTypeCode))
			{
				long num = Convert.ToInt64(value, Constants.DefaultFormatProvider);
				if (num >= minValue && num >= 0L && num <= (long)maxValue)
				{
					return true;
				}
			}
			else if (NumberUtils.IsUnsignedInteger(valueTypeCode))
			{
				ulong num2 = Convert.ToUInt64(value, Constants.DefaultFormatProvider);
				if (num2 <= maxValue)
				{
					return true;
				}
			}
			return false;
		}

		public static readonly Expression NullConstant = Expression.Constant(null, typeof(object));

		public static readonly Expression TrueConstant = Expression.Constant(true);

		public static readonly Expression FalseConstant = Expression.Constant(false);

		public static readonly Expression NegativeSingle = Expression.Constant(-1f);

		public static readonly Expression NegativeDouble = Expression.Constant(-1.0);
	}
}
