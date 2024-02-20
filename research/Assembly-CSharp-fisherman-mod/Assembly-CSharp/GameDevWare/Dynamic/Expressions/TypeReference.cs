using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GameDevWare.Dynamic.Expressions
{
	public sealed class TypeReference : IEquatable<TypeReference>
	{
		private TypeReference()
		{
			this.typeName = new ReadOnlyCollection<string>(new string[] { string.Empty });
			this.typeArguments = new ReadOnlyCollection<TypeReference>(new TypeReference[0]);
			this.fullName = string.Empty;
		}

		public TypeReference(IList<string> typeName, IList<TypeReference> typeArguments)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			if (typeName.Count == 0)
			{
				throw new ArgumentOutOfRangeException("typeName");
			}
			if (typeArguments == null)
			{
				throw new ArgumentNullException("typeArguments");
			}
			for (int i = 0; i < typeName.Count; i++)
			{
				if (string.IsNullOrEmpty(typeName[i]))
				{
					throw new ArgumentException("Type's name contains empty parts.", "typeName");
				}
			}
			for (int j = 0; j < typeArguments.Count; j++)
			{
				if (typeArguments[j] == null)
				{
					throw new ArgumentException("Type's generic arguments contains null values.", "typeArguments");
				}
			}
			this.typeName = (typeName as ReadOnlyCollection<string>) ?? new ReadOnlyCollection<string>(typeName);
			this.typeArguments = (typeArguments as ReadOnlyCollection<TypeReference>) ?? new ReadOnlyCollection<TypeReference>(typeArguments);
			this.hashCode = TypeReference.ComputeHashCode(this);
			if (typeName.Count == 1)
			{
				this.fullName = typeName[0];
			}
		}

		public string FullName
		{
			get
			{
				string text;
				if ((text = this.fullName) == null)
				{
					text = (this.fullName = this.CombineParts(this.typeName.Count, null));
				}
				return text;
			}
		}

		public string Name
		{
			get
			{
				return this.typeName[this.typeName.Count - 1];
			}
		}

		public string Namespace
		{
			get
			{
				return this.CombineParts(this.typeName.Count - 1, null);
			}
		}

		public ReadOnlyCollection<TypeReference> TypeArguments
		{
			get
			{
				return this.typeArguments;
			}
		}

		private string CombineParts(int count, StringBuilder builder = null)
		{
			if (count > this.typeName.Count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (count == 0)
			{
				return string.Empty;
			}
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				num += this.typeName[i].Length;
				if (i != 0)
				{
					num++;
				}
			}
			builder = builder ?? new StringBuilder(num);
			if (builder.Capacity - builder.Length < num)
			{
				builder.Capacity += num;
			}
			for (int j = 0; j < count; j++)
			{
				if (j != 0)
				{
					builder.Append('.');
				}
				builder.Append(this.typeName[j]);
			}
			return builder.ToString();
		}

		private void Format(StringBuilder builder)
		{
			this.CombineParts(this.typeName.Count, builder);
			if (this.typeArguments.Count > 0)
			{
				builder.Append('<');
				for (int i = 0; i < this.typeArguments.Count; i++)
				{
					if (i != 0)
					{
						builder.Append(", ");
					}
					this.typeArguments[i].Format(builder);
				}
				builder.Append('>');
			}
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as TypeReference);
		}

		public bool Equals(TypeReference other)
		{
			return !(other == null) && (object.ReferenceEquals(this, other) || (this.typeName.Count == other.typeName.Count && this.typeName.SequenceEqual(other.typeName) && this.typeArguments.Count == other.typeArguments.Count && this.typeArguments.SequenceEqual(other.typeArguments)));
		}

		public override int GetHashCode()
		{
			return this.hashCode;
		}

		public static bool operator ==(TypeReference x, TypeReference y)
		{
			return object.ReferenceEquals(x, y) || object.Equals(x, y);
		}

		public static bool operator !=(TypeReference x, TypeReference y)
		{
			return !object.Equals(x, y);
		}

		private static int ComputeHashCode(TypeReference typeReference)
		{
			if (typeReference == null)
			{
				throw new ArgumentNullException("typeReference");
			}
			int num = 0;
			for (int i = 0; i < typeReference.typeName.Count; i++)
			{
				num += typeReference.typeName[i].GetHashCode();
			}
			for (int j = 0; j < typeReference.typeArguments.Count; j++)
			{
				num += typeReference.typeArguments[j].GetHashCode();
			}
			return num;
		}

		public override string ToString()
		{
			if (object.ReferenceEquals(this, TypeReference.Empty))
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(1000);
			this.Format(stringBuilder);
			return stringBuilder.ToString();
		}

		public static readonly TypeReference Empty = new TypeReference();

		public static readonly IList<TypeReference> EmptyTypeArguments = TypeReference.Empty.TypeArguments;

		private string fullName;

		private readonly int hashCode;

		private readonly ReadOnlyCollection<string> typeName;

		private readonly ReadOnlyCollection<TypeReference> typeArguments;
	}
}
