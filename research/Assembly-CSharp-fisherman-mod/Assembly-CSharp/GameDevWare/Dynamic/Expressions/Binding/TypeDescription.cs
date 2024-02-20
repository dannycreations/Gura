using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal sealed class TypeDescription : IComparable<TypeDescription>, IComparable, IEquatable<TypeDescription>, IEquatable<Type>
	{
		static TypeDescription()
		{
			Array.ConvertAll<Type, TypeDescription>(new Type[]
			{
				typeof(char?),
				typeof(string),
				typeof(float?),
				typeof(double?),
				typeof(decimal?),
				typeof(byte?),
				typeof(sbyte?),
				typeof(short?),
				typeof(ushort?),
				typeof(int?),
				typeof(uint?),
				typeof(long?),
				typeof(ulong?),
				typeof(Enum),
				typeof(MulticastDelegate)
			}, new Converter<Type, TypeDescription>(TypeDescription.GetTypeDescription));
		}

		public TypeDescription(Type type, TypeCache cache)
		{
			TypeDescription $this = this;
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			this.type = type;
			this.hashCode = type.GetHashCode();
			this.Name = NameUtils.RemoveGenericSuffix(NameUtils.WriteName(type, null, false)).ToString();
			cache.Add(type, this);
			Type type2 = null;
			if (type.IsEnum)
			{
				type2 = Enum.GetUnderlyingType(type);
			}
			else if (type.IsValueType)
			{
				type2 = Nullable.GetUnderlyingType(type);
			}
			this.BaseType = ((type.BaseType == null) ? null : cache.GetOrCreateTypeDescription(type.BaseType));
			this.UnderlyingType = ((type2 == null) ? null : cache.GetOrCreateTypeDescription(type2));
			this.BaseTypes = TypeDescription.GetBaseTypes(this, 0);
			this.Interfaces = Array.ConvertAll<Type, TypeDescription>(type.GetInterfaces(), (Type t) => cache.GetOrCreateTypeDescription(t));
			this.GenericArguments = ((!type.IsGenericType) ? TypeDescription.EmptyTypes : Array.ConvertAll<Type, TypeDescription>(type.GetGenericArguments(), (Type t) => cache.GetOrCreateTypeDescription(t)));
			this.IsNullable = Nullable.GetUnderlyingType(type) != null;
			this.IsNumber = NumberUtils.IsNumber(type);
			this.CanBeNull = this.IsNullable || !type.IsValueType;
			this.IsEnum = type.IsEnum;
			this.IsDelegate = typeof(Delegate).IsAssignableFrom(type) && type != typeof(Delegate) && type != typeof(MulticastDelegate);
			this.HasGenericParameters = type.ContainsGenericParameters;
			this.DefaultExpression = Expression.Constant((!type.IsValueType || this.IsNullable) ? null : Activator.CreateInstance(type), type);
			this.TypeCode = Type.GetTypeCode(type);
			this.MembersByName = this.GetMembersByName(ref this.Indexers);
			MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
			MemberDescription[] array = new MemberDescription[methods.Length];
			this.ImplicitConvertTo = this.GetOperators(methods, array, "op_Implicit", new int?(0));
			this.ImplicitConvertFrom = this.GetOperators(methods, array, "op_Implicit", new int?(-1));
			this.ExplicitConvertTo = this.GetOperators(methods, array, "op_Explicit", new int?(0));
			this.ExplicitConvertFrom = this.GetOperators(methods, array, "op_Explicit", new int?(-1));
			this.Addition = this.GetOperators(methods, array, "op_Addition", null);
			this.Division = this.GetOperators(methods, array, "op_Division", null);
			this.Equality = this.GetOperators(methods, array, "op_Equality", null);
			this.GreaterThan = this.GetOperators(methods, array, "op_GreaterThan", null);
			this.GreaterThanOrEqual = this.GetOperators(methods, array, "op_GreaterThanOrEqual", null);
			this.Inequality = this.GetOperators(methods, array, "op_Inequality", null);
			this.LessThan = this.GetOperators(methods, array, "op_LessThan", null);
			this.LessThanOrEqual = this.GetOperators(methods, array, "op_LessThanOrEqual", null);
			this.Modulus = this.GetOperators(methods, array, "op_Modulus", null);
			this.Multiply = this.GetOperators(methods, array, "op_Multiply", null);
			this.Subtraction = this.GetOperators(methods, array, "op_Subtraction", null);
			this.UnaryNegation = this.GetOperators(methods, array, "op_UnaryNegation", null);
			this.UnaryPlus = this.GetOperators(methods, array, "op_UnaryPlus", null);
			this.BitwiseAnd = this.GetOperators(methods, array, "op_BitwiseAnd", null);
			this.BitwiseOr = this.GetOperators(methods, array, "op_BitwiseOr", null);
			this.Conversions = TypeDescription.Combine<MemberDescription>(new MemberDescription[][] { this.ImplicitConvertTo, this.ImplicitConvertFrom, this.ExplicitConvertTo, this.ExplicitConvertFrom });
			this.Constructors = Array.ConvertAll<ConstructorInfo, MemberDescription>(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public), (ConstructorInfo ctr) => new MemberDescription($this, ctr));
			Array.Sort<MemberDescription>(this.Conversions);
			Array.Sort<MemberDescription>(this.Constructors);
			if (this.IsNullable && this.UnderlyingType != null)
			{
				this.UnderlyingType.nullableType = this;
			}
		}

		private MemberDescription[] GetOperators(MethodInfo[] methods, MemberDescription[] methodsDescriptions, string operatorName, int? compareParameterIndex = null)
		{
			if (methods == null)
			{
				throw new ArgumentNullException("methods");
			}
			if (methodsDescriptions == null)
			{
				throw new ArgumentNullException("methodsDescriptions");
			}
			if (operatorName == null)
			{
				throw new ArgumentNullException("operatorName");
			}
			List<MemberDescription> list = null;
			for (int i = 0; i < methods.Length; i++)
			{
				MethodInfo methodInfo = methods[i];
				if (!(methodInfo.Name != operatorName))
				{
					if (methodsDescriptions[i] == null)
					{
						methodsDescriptions[i] = new MemberDescription(this, methodInfo, null);
					}
					MemberDescription memberDescription = methodsDescriptions[i];
					if (compareParameterIndex == null || memberDescription.GetParameterType(compareParameterIndex.Value) == this.type)
					{
						if (list == null)
						{
							list = new List<MemberDescription>();
						}
						list.Add(new MemberDescription(this, methodInfo, null));
					}
				}
			}
			return (list == null) ? TypeDescription.EmptyMembers : list.ToArray();
		}

		private Dictionary<string, MemberDescription[]> GetMembersByName(ref MemberDescription[] indexers)
		{
			List<MemberInfo> declaredMembers = TypeDescription.GetDeclaredMembers(this.type);
			Dictionary<string, HashSet<MemberDescription>> dictionary = new Dictionary<string, HashSet<MemberDescription>>(((!(this.BaseType != null)) ? 0 : this.BaseType.MembersByName.Count) + declaredMembers.Count);
			foreach (MemberInfo memberInfo in declaredMembers)
			{
				MemberDescription memberDescription = null;
				MethodInfo methodInfo = memberInfo as MethodInfo;
				FieldInfo fieldInfo = memberInfo as FieldInfo;
				PropertyInfo propertyInfo = memberInfo as PropertyInfo;
				if (propertyInfo != null)
				{
					ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
					if (indexParameters.Length == 0)
					{
						memberDescription = new MemberDescription(this, propertyInfo);
					}
					else if (indexers == null)
					{
						indexers = new MemberDescription[]
						{
							new MemberDescription(this, propertyInfo)
						};
					}
					else
					{
						TypeDescription.Add<MemberDescription>(ref indexers, new MemberDescription(this, propertyInfo));
					}
				}
				else if (fieldInfo != null)
				{
					memberDescription = new MemberDescription(this, fieldInfo);
				}
				else if (methodInfo != null && !methodInfo.IsSpecialName)
				{
					memberDescription = new MemberDescription(this, methodInfo, null);
				}
				if (!(memberDescription == null))
				{
					HashSet<MemberDescription> hashSet = null;
					if (!dictionary.TryGetValue(memberDescription.Name, out hashSet))
					{
						hashSet = (dictionary[memberDescription.Name] = new HashSet<MemberDescription>());
					}
					hashSet.Add(memberDescription);
				}
			}
			if (this.BaseType != null)
			{
				foreach (KeyValuePair<string, MemberDescription[]> keyValuePair in this.BaseType.MembersByName)
				{
					string key = keyValuePair.Key;
					MemberDescription[] value = keyValuePair.Value;
					HashSet<MemberDescription> hashSet2 = null;
					if (!dictionary.TryGetValue(key, out hashSet2))
					{
						hashSet2 = (dictionary[key] = new HashSet<MemberDescription>());
					}
					foreach (MemberDescription memberDescription2 in value)
					{
						hashSet2.Add(memberDescription2);
					}
				}
			}
			Dictionary<string, MemberDescription[]> dictionary2 = new Dictionary<string, MemberDescription[]>(dictionary.Count, StringComparer.Ordinal);
			foreach (KeyValuePair<string, HashSet<MemberDescription>> keyValuePair2 in dictionary)
			{
				MemberDescription[] array2 = keyValuePair2.Value.ToArray<MemberDescription>();
				Array.Sort<MemberDescription>(array2);
				dictionary2.Add(keyValuePair2.Key, array2);
			}
			return dictionary2;
		}

		private static List<MemberInfo> GetDeclaredMembers(Type type)
		{
			List<MemberInfo> list = new List<MemberInfo>(type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public));
			if (type.IsInterface)
			{
				foreach (Type type2 in type.GetInterfaces())
				{
					list.AddRange(type2.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public));
				}
			}
			return list;
		}

		private static TypeDescription[] GetBaseTypes(TypeDescription type, int depth)
		{
			TypeDescription[] array;
			if (type.BaseType != null)
			{
				array = TypeDescription.GetBaseTypes(type.BaseType, depth + 1);
			}
			else
			{
				array = new TypeDescription[depth + 1];
			}
			array[depth] = type;
			return array;
		}

		private static void Add<T>(ref T[] array, T element) where T : class
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			Array.Resize<T>(ref array, array.Length + 1);
			array[array.Length - 1] = element;
		}

		private static T[] Combine<T>(params T[][] arrays)
		{
			int num = 0;
			foreach (T[] array in arrays)
			{
				num += array.Length;
			}
			if (arrays.Length == 0)
			{
				return arrays[0];
			}
			T[] array2 = new T[num];
			int num2 = 0;
			foreach (T[] array3 in arrays)
			{
				array3.CopyTo(array2, num2);
				num2 += array3.Length;
			}
			return array2;
		}

		public MemberDescription[] GetMembers(string memberName)
		{
			if (memberName == null)
			{
				throw new ArgumentNullException("memberName");
			}
			MemberDescription[] array = null;
			if (this.MembersByName.TryGetValue(memberName, out array))
			{
				return array;
			}
			return TypeDescription.EmptyMembers;
		}

		public TypeDescription GetNullableType()
		{
			if (this.IsNullable)
			{
				return this;
			}
			if (!this.type.IsValueType)
			{
				throw new InvalidOperationException();
			}
			if (this.nullableType != null)
			{
				return this.nullableType;
			}
			return this.nullableType = TypeDescription.GetTypeDescription(typeof(Nullable<>).MakeGenericType(new Type[] { this.type }));
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as TypeDescription);
		}

		public override int GetHashCode()
		{
			return this.hashCode;
		}

		public int CompareTo(TypeDescription other)
		{
			if (other == null)
			{
				return 1;
			}
			return this.hashCode.CompareTo(other.hashCode);
		}

		public int CompareTo(object obj)
		{
			return this.CompareTo(obj as TypeDescription);
		}

		public bool Equals(TypeDescription other)
		{
			return !(other == null) && (object.ReferenceEquals(this, other) || this.type == other.type);
		}

		public bool Equals(Type other)
		{
			return other != null && (object.ReferenceEquals(this.type, other) || this.type == other);
		}

		public static bool operator ==(TypeDescription type1, TypeDescription type2)
		{
			return object.ReferenceEquals(type1, type2) || (!object.ReferenceEquals(type1, null) && !object.ReferenceEquals(type2, null) && type1.Equals(type2));
		}

		public static bool operator !=(TypeDescription type1, TypeDescription type2)
		{
			return !(type1 == type2);
		}

		public static bool operator ==(TypeDescription type1, Type type2)
		{
			return (!object.ReferenceEquals(type1, null) && object.ReferenceEquals(type1.type, type2)) || (!object.ReferenceEquals(type1, null) && !object.ReferenceEquals(type2, null) && type1.type == type2);
		}

		public static bool operator !=(Type type1, TypeDescription type2)
		{
			return !(type2 == type1);
		}

		public static bool operator ==(Type type1, TypeDescription type2)
		{
			return (!object.ReferenceEquals(type2, null) && object.ReferenceEquals(type2.type, type1)) || (!object.ReferenceEquals(type1, null) && !object.ReferenceEquals(type2, null) && type2.type == type1);
		}

		public static bool operator !=(TypeDescription type1, Type type2)
		{
			return !(type1 == type2);
		}

		public static implicit operator Type(TypeDescription typeDescription)
		{
			if (typeDescription == null)
			{
				return null;
			}
			return typeDescription.type;
		}

		public static TypeDescription GetTypeDescription(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			object types = TypeDescription.Types;
			lock (types)
			{
				TypeDescription typeDescription = null;
				if (TypeDescription.Types.TryGetValue(type, out typeDescription))
				{
					return typeDescription;
				}
			}
			TypeCache typeCache = new TypeCache(TypeDescription.Types);
			TypeDescription orCreateTypeDescription = typeCache.GetOrCreateTypeDescription(type);
			object types2 = TypeDescription.Types;
			lock (types2)
			{
				TypeDescription.Types.Merge(typeCache);
			}
			TypeConversion.UpdateConversions(typeCache.Values);
			return orCreateTypeDescription;
		}

		public override string ToString()
		{
			return this.type.ToString();
		}

		private static readonly TypeCache Types = new TypeCache(null);

		public static readonly MemberDescription[] EmptyMembers = new MemberDescription[0];

		public static readonly TypeDescription[] EmptyTypes = new TypeDescription[0];

		public static readonly TypeDescription ObjectType = TypeDescription.GetTypeDescription(typeof(object));

		public static readonly TypeDescription Int32Type = TypeDescription.GetTypeDescription(typeof(int));

		private readonly Type type;

		private readonly int hashCode;

		private TypeDescription nullableType;

		public readonly Dictionary<string, MemberDescription[]> MembersByName;

		public readonly MemberDescription[] ImplicitConvertTo;

		public readonly MemberDescription[] ImplicitConvertFrom;

		public readonly MemberDescription[] ExplicitConvertTo;

		public readonly MemberDescription[] ExplicitConvertFrom;

		public readonly MemberDescription[] Conversions;

		public readonly MemberDescription[] Addition;

		public readonly MemberDescription[] Division;

		public readonly MemberDescription[] Equality;

		public readonly MemberDescription[] GreaterThan;

		public readonly MemberDescription[] GreaterThanOrEqual;

		public readonly MemberDescription[] Inequality;

		public readonly MemberDescription[] LessThan;

		public readonly MemberDescription[] LessThanOrEqual;

		public readonly MemberDescription[] Modulus;

		public readonly MemberDescription[] Multiply;

		public readonly MemberDescription[] Subtraction;

		public readonly MemberDescription[] UnaryNegation;

		public readonly MemberDescription[] UnaryPlus;

		public readonly MemberDescription[] BitwiseAnd;

		public readonly MemberDescription[] BitwiseOr;

		public readonly MemberDescription[] Indexers;

		public readonly MemberDescription[] Constructors;

		public readonly string Name;

		public readonly TypeCode TypeCode;

		public readonly Expression DefaultExpression;

		public readonly bool IsNullable;

		public readonly bool CanBeNull;

		public readonly bool IsEnum;

		public readonly bool IsNumber;

		public readonly bool IsDelegate;

		public readonly bool HasGenericParameters;

		public readonly TypeDescription BaseType;

		public readonly TypeDescription UnderlyingType;

		public readonly TypeDescription[] BaseTypes;

		public readonly TypeDescription[] Interfaces;

		public readonly TypeDescription[] GenericArguments;
	}
}
