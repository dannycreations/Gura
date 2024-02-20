using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GameDevWare.Dynamic.Expressions
{
	internal static class NameUtils
	{
		public static string[] GetTypeNames(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			string name = type.Name;
			if (string.IsNullOrEmpty(name))
			{
				return NameUtils.EmptyNames;
			}
			if (type == typeof(Array))
			{
				return new string[]
				{
					name,
					name + "`1"
				};
			}
			if (type.IsGenericType)
			{
				return new string[]
				{
					NameUtils.WriteName(type, null, false).ToString(),
					NameUtils.RemoveGenericSuffix(NameUtils.WriteName(type, null, false)).ToString()
				};
			}
			if (type == typeof(byte))
			{
				return new string[]
				{
					"byte",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(sbyte))
			{
				return new string[]
				{
					"sbyte",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(short))
			{
				return new string[]
				{
					"short",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(ushort))
			{
				return new string[]
				{
					"ushort",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(int))
			{
				return new string[]
				{
					"int",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(uint))
			{
				return new string[]
				{
					"uint",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(long))
			{
				return new string[]
				{
					"long",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(ulong))
			{
				return new string[]
				{
					"ulong",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(float))
			{
				return new string[]
				{
					"float",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(double))
			{
				return new string[]
				{
					"double",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(decimal))
			{
				return new string[]
				{
					"decimal",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(char))
			{
				return new string[]
				{
					"char",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(string))
			{
				return new string[]
				{
					"string",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(object))
			{
				return new string[]
				{
					"object",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			if (type == typeof(void))
			{
				return new string[]
				{
					"void",
					NameUtils.WriteName(type, null, false).ToString()
				};
			}
			return new string[] { NameUtils.WriteName(type, null, false).ToString() };
		}

		public static string[] GetTypeFullNames(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			string fullName = type.FullName;
			if (string.IsNullOrEmpty(fullName))
			{
				return NameUtils.EmptyNames;
			}
			if (type == typeof(Array))
			{
				return new string[]
				{
					fullName,
					fullName + "`1"
				};
			}
			if (type.IsGenericType)
			{
				return new string[]
				{
					NameUtils.WriteFullName(type, null, false).ToString(),
					NameUtils.RemoveGenericSuffix(NameUtils.WriteFullName(type, null, false)).ToString()
				};
			}
			return new string[] { NameUtils.WriteFullName(type, null, false).ToString() };
		}

		public static StringBuilder WriteFullName(Type type, StringBuilder builder = null, bool writeGenericArguments = false)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (builder == null)
			{
				builder = new StringBuilder();
			}
			Type[] array = ((!type.IsGenericType || !writeGenericArguments) ? Type.EmptyTypes : type.GetGenericArguments());
			int num = 0;
			NameUtils.TypeNestingEnumerator typeNestingEnumerator = new NameUtils.TypeNestingEnumerator(type);
			foreach (Type type2 in typeNestingEnumerator)
			{
				if (!string.IsNullOrEmpty(type2.Namespace) && !type2.IsNested)
				{
					builder.Append(type.Namespace);
					builder.Append('.');
				}
				int num2 = ((!type2.IsGenericType || !writeGenericArguments) ? 0 : (type2.GetGenericArguments().Length - num));
				NameUtils.WriteNameInternal(type2, new ArraySegment<Type>(array, num, num2), builder);
				if (type2 != type)
				{
					builder.Append('.');
				}
				num += num2;
			}
			return builder;
		}

		public static StringBuilder WriteName(Type type, StringBuilder builder = null, bool writeGenericArguments = false)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (builder == null)
			{
				builder = new StringBuilder();
			}
			Type[] array = ((!type.IsGenericType || !writeGenericArguments) ? Type.EmptyTypes : type.GetGenericArguments());
			int num = 0;
			NameUtils.TypeNestingEnumerator typeNestingEnumerator = new NameUtils.TypeNestingEnumerator(type);
			foreach (Type type2 in typeNestingEnumerator)
			{
				int num2 = ((!type2.IsGenericType || !writeGenericArguments) ? 0 : type2.GetGenericArguments().Length);
				NameUtils.WriteNameInternal(type2, new ArraySegment<Type>(array, num, num2), builder);
				if (type2 != type)
				{
					builder.Append('.');
				}
				num += num2;
			}
			return builder;
		}

		public static string RemoveGenericSuffix(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			int i = name.IndexOf('`');
			int num = 0;
			if (i < 0)
			{
				return name;
			}
			StringBuilder stringBuilder = new StringBuilder(name.Length);
			while (i >= 0)
			{
				stringBuilder.Append(name, num, i - num);
				i++;
				while (i < name.Length && char.IsDigit(name[i]))
				{
					i++;
				}
				num = i;
				i = name.IndexOf('`', num);
			}
			return stringBuilder.ToString();
		}

		public static StringBuilder RemoveGenericSuffix(StringBuilder builder)
		{
			return NameUtils.RemoveGenericSuffix(builder, 0, builder.Length);
		}

		public static StringBuilder RemoveGenericSuffix(StringBuilder builder, int startIndex, int count)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (startIndex < 0 || startIndex > builder.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex");
			}
			if (count < 0 || startIndex + count > builder.Length)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (count == 0 || startIndex == builder.Length)
			{
				return builder;
			}
			int num = startIndex + count;
			int i = builder.IndexOf('`', startIndex, count);
			int num2 = i;
			while (i >= 0)
			{
				i++;
				while (i < num && char.IsDigit(builder[i]))
				{
					i++;
				}
				int num3 = i - num2;
				builder.Remove(num2, num3);
				num -= num3;
				i = builder.IndexOf('`', num2, num - num2);
				num2 = i;
			}
			return builder;
		}

		private static int IndexOf(this StringBuilder builder, char character, int startIndex, int count)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (startIndex < 0 || startIndex > builder.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex");
			}
			if (count < 0 || startIndex + count > builder.Length)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (count == 0 || startIndex == builder.Length)
			{
				return -1;
			}
			int i = startIndex;
			int num = startIndex + count;
			while (i < num)
			{
				if (builder[i] == character)
				{
					return i;
				}
				i++;
			}
			return -1;
		}

		private static Type GetDeclaringType(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return type.DeclaringType;
		}

		private static void WriteNameInternal(Type type, ArraySegment<Type> genericArguments, StringBuilder builder)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			builder.Append(type.Name);
			if (genericArguments.Count > 0)
			{
				builder.Append("<");
				for (int i = genericArguments.Offset; i < genericArguments.Offset + genericArguments.Count; i++)
				{
					if (!genericArguments.Array[i].IsGenericParameter)
					{
						NameUtils.WriteFullName(genericArguments.Array[i], builder, true);
					}
					builder.Append(',');
				}
				builder.Length--;
				builder.Append(">");
			}
		}

		private static readonly string[] EmptyNames = new string[0];

		public struct TypeNestingEnumerator : IEnumerator<Type>, IEnumerable<Type>, IEnumerator, IDisposable, IEnumerable
		{
			public TypeNestingEnumerator(Type type)
			{
				this.type = type;
				this.current = null;
			}

			public bool MoveNext()
			{
				if (this.current == null)
				{
					this.Reset();
					return true;
				}
				if (this.current == this.type)
				{
					return false;
				}
				Type declaringType = this.type;
				while (declaringType != null && NameUtils.GetDeclaringType(declaringType) != this.current)
				{
					declaringType = NameUtils.GetDeclaringType(declaringType);
				}
				this.current = declaringType;
				return declaringType != null;
			}

			public void Reset()
			{
				this.current = this.type;
				while (NameUtils.GetDeclaringType(this.current) != null)
				{
					this.current = NameUtils.GetDeclaringType(this.current);
				}
			}

			public Type Current
			{
				get
				{
					return this.current;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return this.current;
				}
			}

			public NameUtils.TypeNestingEnumerator GetEnumerator()
			{
				return this;
			}

			IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
			{
				return this;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this;
			}

			public void Dispose()
			{
			}

			private readonly Type type;

			private Type current;
		}
	}
}
