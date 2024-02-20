using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal sealed class MemberDescription : IEquatable<MemberDescription>, IEquatable<MemberInfo>, IComparable<MemberDescription>
	{
		public MemberDescription(TypeDescription declaringType, PropertyInfo property)
		{
			if (declaringType == null)
			{
				throw new ArgumentNullException("declaringType");
			}
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			this.member = property;
			this.hashCode = property.GetHashCode();
			this.Name = property.Name;
			this.DeclaringType = declaringType;
			this.ResultType = property.PropertyType;
			this.IsStatic = (property.GetGetMethod(true) ?? property.GetSetMethod(true)).IsStatic;
			this.IsPropertyOrField = true;
			object obj = (((property.Attributes & PropertyAttributes.HasDefault) == PropertyAttributes.None) ? null : property.GetConstantValue());
			this.ConstantValueExpression = ((obj == null) ? null : Expression.Constant(obj));
			MethodInfo getMethod = property.GetGetMethod(false);
			if (getMethod == null)
			{
				return;
			}
			this.member = getMethod;
			this.hashCode = getMethod.GetHashCode();
			this.parameters = getMethod.GetParameters();
			this.parametersByName = this.parameters.ToDictionary(new Func<ParameterInfo, string>(MemberDescription.GetParameterName), StringComparer.Ordinal);
			this.returnParameter = getMethod.ReturnParameter;
		}

		public MemberDescription(TypeDescription declaringType, FieldInfo field)
		{
			if (declaringType == null)
			{
				throw new ArgumentNullException("declaringType");
			}
			if (field == null)
			{
				throw new ArgumentNullException("field");
			}
			this.member = field;
			this.hashCode = field.GetHashCode();
			this.Name = field.Name;
			this.DeclaringType = declaringType;
			this.ResultType = field.FieldType;
			object obj = (((field.Attributes & (FieldAttributes.Literal | FieldAttributes.HasDefault)) == FieldAttributes.PrivateScope) ? null : field.GetRawConstantValue());
			if (declaringType.IsEnum && obj != null)
			{
				this.ConstantValueExpression = Expression.Constant(Enum.ToObject(declaringType, obj), declaringType);
			}
			else
			{
				this.ConstantValueExpression = ((obj == null) ? null : Expression.Constant(obj));
			}
			this.IsStatic = field.IsStatic;
			this.IsPropertyOrField = true;
		}

		public MemberDescription(TypeDescription declaringType, MethodInfo method, MemberDescription genericMethodDefinition = null)
		{
			if (declaringType == null)
			{
				throw new ArgumentNullException("declaringType");
			}
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			this.Name = ((!method.IsGenericMethod) ? method.Name : NameUtils.RemoveGenericSuffix(method.Name));
			this.DeclaringType = declaringType;
			this.ResultType = method.ReturnType;
			this.member = method;
			this.parameters = method.GetParameters();
			this.parametersByName = this.parameters.ToDictionary(new Func<ParameterInfo, string>(MemberDescription.GetParameterName), StringComparer.Ordinal);
			this.returnParameter = method.ReturnParameter;
			this.hashCode = method.GetHashCode();
			if (method.IsGenericMethod)
			{
				this.GenericArguments = method.GetGenericArguments();
				this.GenericArgumentsCount = this.GenericArguments.Length;
				if (method.IsGenericMethodDefinition)
				{
					this.methodInstantiations = new Dictionary<TypeTuple, MemberDescription>();
					this.genericDefinition = this;
				}
				else
				{
					if (genericMethodDefinition == null)
					{
						throw new ArgumentNullException("genericMethodDefinition");
					}
					this.methodInstantiations = genericMethodDefinition.methodInstantiations;
					this.genericDefinition = genericMethodDefinition;
				}
			}
			this.IsMethod = true;
			this.IsStatic = method.IsStatic;
			this.IsImplicitOperator = method.IsSpecialName && this.Name == "op_Implicit";
		}

		public MemberDescription(TypeDescription declaringType, ConstructorInfo constructor)
		{
			if (declaringType == null)
			{
				throw new ArgumentNullException("declaringType");
			}
			if (constructor == null)
			{
				throw new ArgumentNullException("constructor");
			}
			this.Name = constructor.Name;
			this.DeclaringType = declaringType;
			this.ResultType = declaringType;
			this.member = constructor;
			this.parameters = constructor.GetParameters();
			this.parametersByName = this.parameters.ToDictionary(new Func<ParameterInfo, string>(MemberDescription.GetParameterName), StringComparer.Ordinal);
			this.returnParameter = null;
			this.hashCode = constructor.GetHashCode();
			this.GenericArgumentsCount = ((!constructor.IsGenericMethod) ? 0 : constructor.GetGenericArguments().Length);
			this.IsConstructor = true;
			this.IsStatic = constructor.IsStatic;
		}

		public ParameterInfo GetParameter(int parameterIndex)
		{
			if (parameterIndex < -1 || parameterIndex >= this.GetParametersCount())
			{
				throw new ArgumentOutOfRangeException("parameterIndex");
			}
			if (parameterIndex == -1)
			{
				return this.returnParameter;
			}
			return this.parameters[parameterIndex];
		}

		public Type GetParameterType(int parameterIndex)
		{
			if (parameterIndex < -1 || parameterIndex >= this.GetParametersCount())
			{
				throw new ArgumentOutOfRangeException("parameterIndex");
			}
			if (parameterIndex == -1)
			{
				return (this.returnParameter == null) ? null : this.returnParameter.ParameterType;
			}
			return this.parameters[parameterIndex].ParameterType;
		}

		public string GetParameterName(int parameterIndex)
		{
			if (parameterIndex < -1 || parameterIndex >= this.GetParametersCount())
			{
				throw new ArgumentOutOfRangeException("parameterIndex");
			}
			if (parameterIndex == -1)
			{
				return (this.returnParameter == null) ? null : MemberDescription.GetParameterName(this.returnParameter);
			}
			return MemberDescription.GetParameterName(this.parameters[parameterIndex]);
		}

		public int GetParametersCount()
		{
			if (this.parameters == null)
			{
				return 0;
			}
			return this.parameters.Length;
		}

		public MemberDescription MakeGenericMethod(Type[] genericArguments)
		{
			if (genericArguments == null)
			{
				throw new ArgumentNullException("genericArguments");
			}
			if (!this.IsMethod)
			{
				throw new InvalidOperationException(string.Format("Can't instantiate not method '{0}'.", this.member));
			}
			if (this.GenericArgumentsCount <= 0)
			{
				throw new InvalidOperationException(string.Format("Can't instantiate non-generic method '{0}'.", this.member));
			}
			TypeTuple typeTuple = new TypeTuple(genericArguments);
			MemberDescription memberDescription = null;
			object obj = this.methodInstantiations;
			lock (obj)
			{
				if (this.methodInstantiations.TryGetValue(typeTuple, out memberDescription))
				{
					return memberDescription;
				}
			}
			MethodInfo methodInfo = this.genericDefinition.MakeGenericMethod(genericArguments);
			memberDescription = new MemberDescription(this.DeclaringType, methodInfo, this.genericDefinition);
			object obj2 = this.methodInstantiations;
			lock (obj2)
			{
				this.methodInstantiations[typeTuple] = memberDescription;
			}
			return memberDescription;
		}

		public bool TryMakeAccessor(Expression target, out Expression expression)
		{
			if (!this.IsStatic && target == null)
			{
				throw new ArgumentNullException("target");
			}
			FieldInfo fieldInfo = this.member as FieldInfo;
			MethodInfo methodInfo = this.member as MethodInfo;
			if (fieldInfo != null)
			{
				expression = Expression.Field(target, fieldInfo);
			}
			else if (methodInfo != null && this.parameters.Length == 0)
			{
				expression = Expression.Property(target, methodInfo);
			}
			else
			{
				expression = null;
			}
			return expression != null;
		}

		public bool TryMakeConversion(Expression valueExpression, out Expression expression, bool checkedConversion)
		{
			if (valueExpression == null)
			{
				throw new ArgumentNullException("valueExpression");
			}
			expression = null;
			MethodInfo methodInfo = this.member as MethodInfo;
			if (methodInfo == null)
			{
				return false;
			}
			TypeDescription typeDescription = TypeDescription.GetTypeDescription(valueExpression.Type);
			TypeDescription typeDescription2 = TypeDescription.GetTypeDescription(methodInfo.ReturnType);
			bool isNullable = typeDescription.IsNullable;
			if (!typeDescription2.CanBeNull)
			{
				typeDescription2 = ((!isNullable) ? typeDescription2 : typeDescription2.GetNullableType());
			}
			expression = ((!checkedConversion) ? Expression.Convert(valueExpression, typeDescription2, methodInfo) : Expression.ConvertChecked(valueExpression, typeDescription2, methodInfo));
			return true;
		}

		public bool TryMakeCall(Expression target, ArgumentsTree argumentsTree, BindingContext bindingContext, out Expression expression, out float expressionQuality)
		{
			if (argumentsTree == null)
			{
				throw new ArgumentNullException("argumentsTree");
			}
			if (bindingContext == null)
			{
				throw new ArgumentNullException("bindingContext");
			}
			if (!this.IsStatic && !this.IsConstructor && target == null)
			{
				throw new ArgumentNullException("target");
			}
			expression = null;
			expressionQuality = 0f;
			if (!(this.member is MethodBase))
			{
				return false;
			}
			if (argumentsTree.Count > this.parameters.Length)
			{
				return false;
			}
			int num = this.parameters.Length - this.parameters.Count((ParameterInfo p) => p.IsOptional);
			if (argumentsTree.Count < num)
			{
				return false;
			}
			float num2 = 0f;
			Expression[] array = null;
			foreach (string text in argumentsTree.Keys)
			{
				ParameterInfo parameterInfo = null;
				int num3;
				if (MemberDescription.HasDigitsOnly(text))
				{
					num3 = int.Parse(text, Constants.DefaultFormatProvider);
					if (num3 >= this.parameters.Length)
					{
						return false;
					}
					parameterInfo = this.parameters[num3];
					if (argumentsTree.ContainsKey(parameterInfo.Name))
					{
						return false;
					}
				}
				else
				{
					if (!this.parametersByName.TryGetValue(text, out parameterInfo))
					{
						return false;
					}
					num3 = parameterInfo.Position;
				}
				TypeDescription typeDescription = TypeDescription.GetTypeDescription(parameterInfo.ParameterType);
				Expression expression2 = null;
				Exception ex = null;
				if (!AnyBinder.TryBindInNewScope(argumentsTree[text], bindingContext, typeDescription, out expression2, out ex))
				{
					return false;
				}
				float num4 = 0f;
				if (!ExpressionUtils.TryMorphType(ref expression2, typeDescription, out num4) || num4 <= 0f)
				{
					return false;
				}
				num2 += num4;
				if (array == null)
				{
					array = new Expression[this.parameters.Length];
				}
				array[num3] = expression2;
			}
			if (this.parameters.Length > 0)
			{
				if (array == null)
				{
					array = new Expression[this.parameters.Length];
				}
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == null)
					{
						ParameterInfo parameterInfo2 = this.parameters[i];
						if (!parameterInfo2.IsOptional)
						{
							return false;
						}
						TypeDescription typeDescription2 = TypeDescription.GetTypeDescription(parameterInfo2.ParameterType);
						array[i] = typeDescription2.DefaultExpression;
						num2 += 1f;
					}
				}
				expressionQuality = num2 / (float)this.parameters.Length;
			}
			else
			{
				expressionQuality = 1f;
			}
			if (this.member is MethodInfo)
			{
				if (this.IsStatic)
				{
					expression = Expression.Call((MethodInfo)this.member, array);
				}
				else
				{
					expression = Expression.Call(target, (MethodInfo)this.member, array);
				}
				return true;
			}
			if (this.member is ConstructorInfo)
			{
				expression = Expression.New((ConstructorInfo)this.member, array);
				return true;
			}
			expressionQuality = 0f;
			return false;
		}

		private static bool HasDigitsOnly(string argName)
		{
			foreach (char c in argName)
			{
				if (!char.IsDigit(c))
				{
					return false;
				}
			}
			return true;
		}

		private static string GetParameterName(ParameterInfo parameter)
		{
			if (parameter == null)
			{
				throw new ArgumentNullException("parameter");
			}
			string text = parameter.Name;
			if (string.IsNullOrEmpty(text))
			{
				text = parameter.Member.Name + "_" + parameter.Position.ToString();
			}
			return text;
		}

		public override bool Equals(object obj)
		{
			if (obj is MemberDescription)
			{
				return this.Equals(obj as MemberDescription);
			}
			return obj is MemberInfo && this.Equals(obj as MemberInfo);
		}

		public override int GetHashCode()
		{
			return this.hashCode;
		}

		public bool Equals(MemberInfo other)
		{
			return other != null && this.member.Equals(other);
		}

		public bool Equals(MemberDescription other)
		{
			return !(other == null) && this.member.Equals(other.member);
		}

		public int CompareTo(MemberDescription other)
		{
			if (other == null)
			{
				return 1;
			}
			int num = other.GetParametersCount().CompareTo(this.GetParametersCount());
			return (num == 0) ? string.CompareOrdinal(this.Name, other.Name) : num;
		}

		public static implicit operator MemberInfo(MemberDescription memberDescription)
		{
			if (memberDescription == null)
			{
				return null;
			}
			return memberDescription.member;
		}

		public static implicit operator MethodInfo(MemberDescription memberDescription)
		{
			if (memberDescription == null)
			{
				return null;
			}
			return (MethodInfo)memberDescription.member;
		}

		public static bool operator ==(MemberDescription member1, MemberDescription member2)
		{
			return object.ReferenceEquals(member1, member2) || (!object.ReferenceEquals(member1, null) && !object.ReferenceEquals(member2, null) && member1.Equals(member2));
		}

		public static bool operator !=(MemberDescription type1, MemberDescription type2)
		{
			return !(type1 == type2);
		}

		public override string ToString()
		{
			return this.member.ToString();
		}

		public const float QUALITY_EXACT_MATCH = 1f;

		public const float QUALITY_INCOMPATIBLE = 0f;

		private readonly int hashCode;

		private readonly MemberInfo member;

		private readonly ParameterInfo[] parameters;

		private readonly Dictionary<string, ParameterInfo> parametersByName;

		private readonly ParameterInfo returnParameter;

		private readonly Dictionary<TypeTuple, MemberDescription> methodInstantiations;

		private readonly MemberDescription genericDefinition;

		public readonly string Name;

		public readonly Type ResultType;

		public readonly TypeDescription DeclaringType;

		public readonly bool IsMethod;

		public readonly bool IsConstructor;

		public readonly bool IsPropertyOrField;

		public readonly bool IsStatic;

		public readonly bool IsImplicitOperator;

		public readonly Type[] GenericArguments;

		public readonly int GenericArgumentsCount;

		public readonly Expression ConstantValueExpression;
	}
}
