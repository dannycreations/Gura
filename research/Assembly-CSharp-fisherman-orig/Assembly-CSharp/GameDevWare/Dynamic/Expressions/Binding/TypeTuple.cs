using System;
using System.Text;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal struct TypeTuple : IEquatable<TypeTuple>
	{
		public TypeTuple(params Type[] types)
		{
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			this.Types = types;
			this.hashCode = 17;
			foreach (Type type in types)
			{
				if (type == null)
				{
					throw new ArgumentException("One of array's element is null.", "types");
				}
				this.hashCode = this.hashCode * 23 + type.GetHashCode();
			}
		}

		public bool Equals(TypeTuple other)
		{
			if (this.Types == other.Types)
			{
				return true;
			}
			if (this.Types == null || other.Types == null)
			{
				return false;
			}
			if (this.Types.Length != other.Types.Length)
			{
				return false;
			}
			for (int i = 0; i < this.Types.Length; i++)
			{
				if (this.Types[i] != other.Types[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return this.hashCode;
		}

		public override bool Equals(object obj)
		{
			return obj is TypeTuple && this.Equals((TypeTuple)obj);
		}

		public override string ToString()
		{
			if (this.Types != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Type type in this.Types)
				{
					stringBuilder.Append(type.Name).Append(", ");
				}
				if (stringBuilder.Length > 2)
				{
					stringBuilder.Length -= 2;
				}
				return stringBuilder.ToString();
			}
			return "empty";
		}

		private readonly int hashCode;

		public readonly Type[] Types;
	}
}
