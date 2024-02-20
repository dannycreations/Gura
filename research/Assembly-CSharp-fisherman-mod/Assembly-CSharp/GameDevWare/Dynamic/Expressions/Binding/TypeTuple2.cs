using System;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal struct TypeTuple2 : IEquatable<TypeTuple2>
	{
		public TypeTuple2(Type type1, Type type2)
		{
			if (type1 == null)
			{
				throw new ArgumentNullException("type1");
			}
			if (type2 == null)
			{
				throw new ArgumentNullException("type2");
			}
			this.Type1 = type1;
			this.Type2 = type2;
			this.hashCode = 17;
			this.hashCode = this.hashCode * 23 + this.Type1.GetHashCode();
			this.hashCode = this.hashCode * 23 + this.Type2.GetHashCode();
		}

		public bool Equals(TypeTuple2 other)
		{
			return this.Type1 == other.Type1 && this.Type2 == other.Type2;
		}

		public override int GetHashCode()
		{
			return this.hashCode;
		}

		public override bool Equals(object obj)
		{
			return obj is TypeTuple2 && this.Equals((TypeTuple2)obj);
		}

		public override string ToString()
		{
			if (this.Type1 != null && this.Type2 != null)
			{
				return this.Type1.Name + "/" + this.Type2.Name;
			}
			return "empty";
		}

		private readonly int hashCode;

		public readonly Type Type1;

		public readonly Type Type2;
	}
}
