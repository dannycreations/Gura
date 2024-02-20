using System;
using System.Collections.Generic;
using System.Linq;

namespace GameDevWare.Dynamic.Expressions
{
	public class KnownTypeResolver : ITypeResolver
	{
		public KnownTypeResolver(params Type[] knownTypes)
			: this(knownTypes)
		{
		}

		public KnownTypeResolver(IEnumerable<Type> knownTypes)
			: this(knownTypes, null, TypeDiscoveryOptions.All)
		{
		}

		public KnownTypeResolver(IEnumerable<Type> knownTypes, ITypeResolver otherTypeResolver)
			: this(knownTypes, otherTypeResolver, TypeDiscoveryOptions.All)
		{
		}

		public KnownTypeResolver(IEnumerable<Type> knownTypes, ITypeResolver otherTypeResolver, TypeDiscoveryOptions options)
		{
			if (knownTypes == null)
			{
				knownTypes = Type.EmptyTypes;
			}
			this.knownTypesByFullName = new Dictionary<string, List<Type>>(StringComparer.Ordinal);
			this.knownTypesByName = new Dictionary<string, List<Type>>(StringComparer.Ordinal);
			this.knownTypes = KnownTypeResolver.GetKnownTypes(knownTypes, null, options);
			foreach (Type type in this.knownTypes)
			{
				foreach (string text in NameUtils.GetTypeNames(type))
				{
					List<Type> list = null;
					if (!this.knownTypesByName.TryGetValue(text, out list))
					{
						this.knownTypesByName.Add(text, list = new List<Type>());
					}
					list.Add(type);
				}
				foreach (string text2 in NameUtils.GetTypeFullNames(type))
				{
					List<Type> list2 = null;
					if (!this.knownTypesByFullName.TryGetValue(text2, out list2))
					{
						this.knownTypesByFullName.Add(text2, list2 = new List<Type>());
					}
					list2.Add(type);
				}
			}
			this.otherTypeResolver = otherTypeResolver;
		}

		public bool TryGetType(TypeReference typeReference, out Type foundType)
		{
			foundType = null;
			int num = 0;
			bool flag = typeReference.TypeArguments.Count > 0;
			List<Type> list = null;
			if (this.knownTypesByFullName.TryGetValue(typeReference.FullName, out list))
			{
				foreach (Type type in list)
				{
					if (flag == type.IsGenericType)
					{
						if (!flag || type.GetGenericArguments().Length == typeReference.TypeArguments.Count)
						{
							if (foundType != type)
							{
								num++;
							}
							foundType = type;
						}
					}
				}
			}
			if (this.knownTypesByName.TryGetValue(typeReference.FullName, out list))
			{
				foreach (Type type2 in list)
				{
					if (flag == type2.IsGenericType)
					{
						if (!flag || type2.GetGenericArguments().Length == typeReference.TypeArguments.Count)
						{
							if (foundType != type2)
							{
								num++;
							}
							foundType = type2;
						}
					}
				}
			}
			if (foundType == null && (string.Equals(typeReference.FullName, KnownTypeResolver.ArrayFullName, StringComparison.Ordinal) || string.Equals(typeReference.Name, KnownTypeResolver.ArrayName, StringComparison.Ordinal)))
			{
				foundType = typeof(Array);
				num = 1;
			}
			if (foundType == null && this.otherTypeResolver != null)
			{
				this.otherTypeResolver.TryGetType(typeReference, out foundType);
				num = ((foundType == null) ? 0 : 1);
			}
			if (num != 1)
			{
				foundType = null;
				return false;
			}
			if (foundType == typeof(Array))
			{
				if (typeReference.TypeArguments.Count == 1)
				{
					Type type3 = null;
					if (!this.TryGetType(typeReference.TypeArguments[0], out type3))
					{
						return false;
					}
					foundType = type3.MakeArrayType();
					return true;
				}
				else
				{
					if (typeReference.TypeArguments.Count == 0)
					{
						return true;
					}
					foundType = null;
				}
			}
			else if (foundType != null && typeReference.TypeArguments.Count > 0)
			{
				Type[] genericArguments;
				if (foundType.IsGenericType && (genericArguments = foundType.GetGenericArguments()).Length == typeReference.TypeArguments.Count)
				{
					Type[] array = new Type[genericArguments.Length];
					bool flag2 = true;
					bool flag3 = true;
					for (int i = 0; i < array.Length; i++)
					{
						TypeReference typeReference2 = typeReference.TypeArguments[i];
						if (typeReference2 == TypeReference.Empty)
						{
							array[i] = genericArguments[i];
						}
						else if (this.TryGetType(typeReference2, out array[i]))
						{
							flag3 = false;
						}
						else
						{
							flag2 = false;
						}
					}
					if (!foundType.IsGenericTypeDefinition)
					{
						foundType = foundType.GetGenericTypeDefinition() ?? foundType;
					}
					if (flag2)
					{
						foundType = ((!flag3) ? foundType.MakeGenericType(array) : foundType);
						return true;
					}
				}
				foundType = null;
			}
			return foundType != null;
		}

		public bool IsKnownType(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return this.knownTypes.Contains(type) || (this.otherTypeResolver != null && this.otherTypeResolver.IsKnownType(type));
		}

		private static HashSet<Type> GetKnownTypes(IEnumerable<Type> types, HashSet<Type> collection, TypeDiscoveryOptions options)
		{
			HashSet<Type> hashSet = collection ?? new HashSet<Type>(KnownTypeResolver.BuildInTypes);
			foreach (Type type in types)
			{
				Type[] array = Type.EmptyTypes;
				Type type2 = type;
				if (type2.HasElementType)
				{
					type2 = type2.GetElementType() ?? type2;
				}
				if (type2.IsGenericType)
				{
					array = type2.GetGenericArguments();
					type2 = type2.GetGenericTypeDefinition() ?? type2;
				}
				if (!type2.IsGenericParameter)
				{
					bool flag = !hashSet.Add(type2);
					if (type2.IsGenericType && (options & TypeDiscoveryOptions.GenericArguments) != TypeDiscoveryOptions.Default)
					{
						KnownTypeResolver.GetKnownTypes(array, hashSet, options);
					}
					if (!flag)
					{
						if ((options & TypeDiscoveryOptions.Interfaces) != TypeDiscoveryOptions.Default)
						{
							KnownTypeResolver.GetKnownTypes(type2.GetInterfaces(), hashSet, options);
						}
						if ((options & TypeDiscoveryOptions.KnownTypes) != TypeDiscoveryOptions.Default)
						{
							KnownTypeResolver.GetKnownTypes(from ExpressionKnownTypeAttribute a in type2.GetCustomAttributes(typeof(ExpressionKnownTypeAttribute), true)
								select a.Type, hashSet, options);
						}
						if ((options & TypeDiscoveryOptions.DeclaringTypes) != TypeDiscoveryOptions.Default)
						{
							List<Type> list = null;
							for (Type type3 = type2.DeclaringType; type3 != null; type3 = type3.DeclaringType)
							{
								if (list == null)
								{
									list = new List<Type>(10);
								}
								list.Add(type3);
							}
							if (list != null)
							{
								KnownTypeResolver.GetKnownTypes(list, hashSet, options);
							}
						}
					}
				}
			}
			return hashSet;
		}

		public override string ToString()
		{
			return base.GetType().Name + ": " + string.Join(", ", this.knownTypesByName.Keys.ToArray<string>()) + ((this.otherTypeResolver == null) ? string.Empty : (" -> " + this.otherTypeResolver));
		}

		public static readonly KnownTypeResolver Default = new KnownTypeResolver(new Type[0]);

		private static readonly HashSet<Type> BuildInTypes = new HashSet<Type>
		{
			typeof(object),
			typeof(bool),
			typeof(char),
			typeof(sbyte),
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(DateTime),
			typeof(TimeSpan),
			typeof(string),
			typeof(Math),
			typeof(Array),
			typeof(Nullable<>),
			typeof(Func<>),
			typeof(Func<, >),
			typeof(Func<, , >),
			typeof(Func<, , , >),
			typeof(Func<, , , , >)
		};

		private static readonly string ArrayName = typeof(Array).Name + "`1";

		private static readonly string ArrayFullName = typeof(Array).FullName + "`1";

		private readonly Dictionary<string, List<Type>> knownTypesByFullName;

		private readonly Dictionary<string, List<Type>> knownTypesByName;

		private readonly HashSet<Type> knownTypes;

		private readonly ITypeResolver otherTypeResolver;
	}
}
