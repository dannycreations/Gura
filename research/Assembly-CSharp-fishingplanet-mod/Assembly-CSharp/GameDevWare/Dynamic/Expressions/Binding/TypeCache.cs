using System;
using System.Collections.Generic;

namespace GameDevWare.Dynamic.Expressions.Binding
{
	internal sealed class TypeCache
	{
		public TypeCache(TypeCache parentCache = null)
		{
			this.parentCache = parentCache;
			this.types = new Dictionary<Type, TypeDescription>();
		}

		public Dictionary<Type, TypeDescription>.ValueCollection Values
		{
			get
			{
				return this.types.Values;
			}
		}

		public bool TryGetValue(Type type, out TypeDescription typeDescription)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (this.types.TryGetValue(type, out typeDescription))
			{
				return true;
			}
			if (this.parentCache == null)
			{
				return false;
			}
			object obj = this.parentCache;
			bool flag;
			lock (obj)
			{
				flag = this.parentCache.TryGetValue(type, out typeDescription);
			}
			return flag;
		}

		public bool TryAdd(Type type, ref TypeDescription typeDescription)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (typeDescription == null)
			{
				throw new ArgumentNullException("typeDescription");
			}
			TypeDescription typeDescription2 = null;
			if (this.TryGetValue(type, out typeDescription2))
			{
				typeDescription = typeDescription2;
				return false;
			}
			this.types[type] = typeDescription;
			return true;
		}

		public void Add(Type type, TypeDescription typeDescription)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (typeDescription == null)
			{
				throw new ArgumentNullException("typeDescription");
			}
			if (!this.TryAdd(type, ref typeDescription))
			{
				throw new ArgumentException(string.Format("TypeDescription for types '{0}' is already exists in cache.", typeDescription), "type");
			}
		}

		public TypeDescription GetOrCreateTypeDescription(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			TypeDescription typeDescription = null;
			if (this.TryGetValue(type, out typeDescription))
			{
				return typeDescription;
			}
			typeDescription = new TypeDescription(type, this);
			return typeDescription;
		}

		public void Merge(TypeCache otherCache)
		{
			if (otherCache == null)
			{
				throw new ArgumentNullException("otherCache");
			}
			foreach (KeyValuePair<Type, TypeDescription> keyValuePair in otherCache.types)
			{
				this.types[keyValuePair.Key] = keyValuePair.Value;
			}
		}

		public override string ToString()
		{
			return this.types.ToString();
		}

		private readonly Dictionary<Type, TypeDescription> types;

		private readonly TypeCache parentCache;
	}
}
