using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nss.Udt.Referee
{
	public class RefereeManager<T> : RefereeManagerBase where T : MonoBehaviour
	{
		public void OnEnable()
		{
			if (RefereeManager<T>.Instance == null)
			{
				RefereeManager<T>.Instance = this;
			}
		}

		public void Add(T reference)
		{
			this.References.Add(reference);
		}

		public void Remove(T reference)
		{
			this.References.Remove(reference);
		}

		public T FindByTag(string tag, bool ignoreCase = false)
		{
			if (ignoreCase)
			{
				return this.References.Find((T t) => t.tag.Equals(tag, StringComparison.OrdinalIgnoreCase));
			}
			return this.References.Find((T t) => t.tag.Equals(tag));
		}

		public List<T> FindAllByTag(string tag, bool ignoreCase = false)
		{
			if (ignoreCase)
			{
				return this.References.FindAll((T t) => t.tag.Equals(tag, StringComparison.OrdinalIgnoreCase));
			}
			return this.References.FindAll((T t) => t.tag.Equals(tag));
		}

		public T FindByName(string name, bool ignoreCase = false)
		{
			if (ignoreCase)
			{
				return this.References.Find((T t) => t.name.Equals(name, StringComparison.OrdinalIgnoreCase));
			}
			return this.References.Find((T t) => t.name.Equals(name));
		}

		public List<T> FindAllByName(string name, bool ignoreCase = false)
		{
			if (ignoreCase)
			{
				return this.References.FindAll((T t) => t.name.Equals(name, StringComparison.OrdinalIgnoreCase));
			}
			return this.References.FindAll((T t) => t.name.Equals(name));
		}

		public static RefereeManager<T> Instance;

		public List<T> References = new List<T>();
	}
}
