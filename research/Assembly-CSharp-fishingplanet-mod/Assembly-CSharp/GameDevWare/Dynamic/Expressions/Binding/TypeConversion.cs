using System;
using System.Collections.Generic;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal sealed class TypeConversion
	{
		static TypeConversion()
		{
			TypeConversion.SetNaturalConversion(0.5f, typeof(float), new Type[] { typeof(double) });
			TypeConversion.SetNaturalConversion(0.5f, typeof(byte), new Type[]
			{
				typeof(ushort),
				typeof(short),
				typeof(uint),
				typeof(int),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(char), new Type[]
			{
				typeof(ushort),
				typeof(uint),
				typeof(int),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(ushort), new Type[]
			{
				typeof(uint),
				typeof(int),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(uint), new Type[]
			{
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(ulong), new Type[]
			{
				typeof(float),
				typeof(double)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(sbyte), new Type[]
			{
				typeof(short),
				typeof(int),
				typeof(long),
				typeof(float),
				typeof(double)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(short), new Type[]
			{
				typeof(int),
				typeof(long),
				typeof(float),
				typeof(double)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(int), new Type[]
			{
				typeof(long),
				typeof(float),
				typeof(double)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(long), new Type[]
			{
				typeof(float),
				typeof(double)
			});
			TypeConversion.SetNaturalConversion(0f, typeof(double), new Type[]
			{
				typeof(float),
				typeof(long),
				typeof(ulong),
				typeof(int),
				typeof(uint),
				typeof(short),
				typeof(char),
				typeof(ushort),
				typeof(sbyte),
				typeof(byte)
			});
			TypeConversion.SetNaturalConversion(0f, typeof(float), new Type[]
			{
				typeof(long),
				typeof(ulong),
				typeof(int),
				typeof(uint),
				typeof(short),
				typeof(char),
				typeof(ushort),
				typeof(sbyte),
				typeof(byte)
			});
			TypeConversion.SetNaturalConversion(0f, typeof(byte), new Type[] { typeof(sbyte) });
			TypeConversion.SetNaturalConversion(0f, typeof(char), new Type[]
			{
				typeof(sbyte),
				typeof(byte)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(ushort), new Type[]
			{
				typeof(char),
				typeof(short),
				typeof(byte),
				typeof(sbyte)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(uint), new Type[]
			{
				typeof(int),
				typeof(char),
				typeof(ushort),
				typeof(short),
				typeof(byte),
				typeof(sbyte)
			});
			TypeConversion.SetNaturalConversion(0.5f, typeof(ulong), new Type[]
			{
				typeof(long),
				typeof(uint),
				typeof(int),
				typeof(char),
				typeof(ushort),
				typeof(short),
				typeof(byte),
				typeof(sbyte)
			});
			TypeConversion.SetNaturalConversion(0f, typeof(sbyte), new Type[]
			{
				typeof(ulong),
				typeof(uint),
				typeof(char),
				typeof(ushort),
				typeof(byte)
			});
			TypeConversion.SetNaturalConversion(0f, typeof(short), new Type[]
			{
				typeof(ulong),
				typeof(uint),
				typeof(char),
				typeof(ushort),
				typeof(sbyte),
				typeof(byte)
			});
			TypeConversion.SetNaturalConversion(0f, typeof(int), new Type[]
			{
				typeof(ulong),
				typeof(uint),
				typeof(short),
				typeof(char),
				typeof(ushort),
				typeof(sbyte),
				typeof(byte)
			});
			TypeConversion.SetNaturalConversion(0f, typeof(long), new Type[]
			{
				typeof(ulong),
				typeof(int),
				typeof(uint),
				typeof(short),
				typeof(char),
				typeof(ushort),
				typeof(sbyte),
				typeof(byte)
			});
		}

		public TypeConversion(float quality, bool isNatural, MemberDescription implicitConversion = null, MemberDescription explicitConversion = null)
		{
			this.Quality = quality;
			this.IsNatural = isNatural;
			this.Implicit = implicitConversion;
			this.Explicit = explicitConversion;
		}

		public TypeConversion Expand(MemberDescription implicitConversion, MemberDescription explicitConversion)
		{
			implicitConversion = implicitConversion ?? this.Implicit;
			explicitConversion = explicitConversion ?? this.Explicit;
			if (implicitConversion == this.Implicit && explicitConversion == this.Explicit)
			{
				return this;
			}
			float num = Math.Max(this.Quality, (!(this.Implicit != null)) ? 0f : 0.5f);
			return new TypeConversion(num, this.IsNatural, implicitConversion, explicitConversion);
		}

		public static bool TryGetTypeConversion(Type fromType, Type toType, out TypeConversion typeConversion)
		{
			if (fromType == null)
			{
				throw new ArgumentNullException("fromType");
			}
			if (toType == null)
			{
				throw new ArgumentNullException("toType");
			}
			TypeTuple2 typeTuple = new TypeTuple2(fromType, toType);
			object conversions = TypeConversion.Conversions;
			bool flag;
			lock (conversions)
			{
				flag = TypeConversion.Conversions.TryGetValue(typeTuple, out typeConversion);
			}
			return flag;
		}

		internal static void UpdateConversions(IEnumerable<TypeDescription> typeDescriptions)
		{
			if (typeDescriptions == null)
			{
				throw new ArgumentNullException("typeDescriptions");
			}
			object conversions = TypeConversion.Conversions;
			lock (conversions)
			{
				foreach (TypeDescription typeDescription in typeDescriptions)
				{
					foreach (MemberDescription memberDescription in typeDescription.Conversions)
					{
						TypeTuple2 typeTuple = new TypeTuple2(memberDescription.GetParameterType(0), memberDescription.GetParameterType(-1));
						MemberDescription memberDescription2 = ((!memberDescription.IsImplicitOperator) ? memberDescription : null);
						MemberDescription memberDescription3 = ((!memberDescription.IsImplicitOperator) ? null : memberDescription);
						TypeConversion typeConversion = null;
						float num = ((!memberDescription.IsImplicitOperator) ? 0f : 0.5f);
						TypeConversion typeConversion2;
						if (!TypeConversion.Conversions.TryGetValue(typeTuple, out typeConversion))
						{
							typeConversion2 = new TypeConversion(num, false, memberDescription3, memberDescription2);
						}
						else
						{
							typeConversion2 = typeConversion.Expand(memberDescription3, memberDescription2);
						}
						if (typeConversion2 != typeConversion)
						{
							TypeConversion.Conversions[typeTuple] = typeConversion2;
						}
					}
					foreach (TypeDescription typeDescription2 in typeDescription.BaseTypes)
					{
						TypeTuple2 typeTuple2 = new TypeTuple2(typeDescription, typeDescription2);
						float num2 = ((!(typeDescription2 == typeDescription)) ? 0.9f : 1f);
						TypeConversion typeConversion3 = null;
						if (TypeConversion.Conversions.TryGetValue(typeTuple2, out typeConversion3))
						{
							typeConversion3 = new TypeConversion(num2, true, typeConversion3.Implicit, typeConversion3.Explicit);
						}
						else
						{
							typeConversion3 = new TypeConversion(num2, true, null, null);
						}
						TypeConversion.Conversions[typeTuple2] = typeConversion3;
					}
					foreach (TypeDescription typeDescription3 in typeDescription.Interfaces)
					{
						TypeTuple2 typeTuple3 = new TypeTuple2(typeDescription, typeDescription3);
						float num3 = ((!(typeDescription3 == typeDescription)) ? 0.9f : 1f);
						TypeConversion typeConversion4 = null;
						if (TypeConversion.Conversions.TryGetValue(typeTuple3, out typeConversion4))
						{
							typeConversion4 = new TypeConversion(num3, true, typeConversion4.Implicit, typeConversion4.Explicit);
						}
						else
						{
							typeConversion4 = new TypeConversion(num3, true, null, null);
						}
						TypeConversion.Conversions[typeTuple3] = typeConversion4;
					}
					if (typeDescription.IsEnum)
					{
						TypeTuple2 typeTuple4 = new TypeTuple2(typeDescription, typeDescription.UnderlyingType);
						TypeTuple2 typeTuple5 = new TypeTuple2(typeDescription.UnderlyingType, typeDescription);
						TypeConversion.Conversions[typeTuple4] = new TypeConversion(0.7f, true, null, null);
						TypeConversion.Conversions[typeTuple5] = new TypeConversion(0.7f, true, null, null);
					}
				}
			}
		}

		private static void SetNaturalConversion(float quality, Type fromType, params Type[] toTypes)
		{
			if (fromType == null)
			{
				throw new ArgumentNullException("fromType");
			}
			if (toTypes == null)
			{
				throw new ArgumentNullException("toTypes");
			}
			foreach (Type type in toTypes)
			{
				TypeTuple2 typeTuple = new TypeTuple2(fromType, type);
				TypeConversion.Conversions.Add(typeTuple, new TypeConversion(quality, true, null, null));
			}
		}

		public override string ToString()
		{
			return string.Format("Quality: {0}, Is Natural: {1}, Implicit: {2}, Explicit: {3}", new object[]
			{
				this.Quality.ToString(),
				this.IsNatural.ToString(),
				this.Implicit,
				this.Explicit
			});
		}

		public const float QUALITY_SAME_TYPE = 1f;

		public const float QUALITY_INHERITANCE_HIERARCHY = 0.9f;

		public const float QUALITY_IN_PLACE_CONVERSION = 0.7f;

		public const float QUALITY_IMPLICIT_CONVERSION = 0.5f;

		public const float QUALITY_NUMBER_EXPANSION = 0.5f;

		public const float QUALITY_PRECISION_CONVERSION = 0.4f;

		public const float QUALITY_EXPLICIT_CONVERSION = 0f;

		public const float QUALITY_NO_CONVERSION = 0f;

		private static readonly Dictionary<TypeTuple2, TypeConversion> Conversions = new Dictionary<TypeTuple2, TypeConversion>(EqualityComparer<TypeTuple2>.Default);

		public readonly float Quality;

		public readonly bool IsNatural;

		public readonly MemberDescription Implicit;

		public readonly MemberDescription Explicit;
	}
}
