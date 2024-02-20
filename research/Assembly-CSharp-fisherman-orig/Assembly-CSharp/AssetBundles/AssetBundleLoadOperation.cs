using System;
using System.Collections;

namespace AssetBundles
{
	public abstract class AssetBundleLoadOperation : IEnumerator
	{
		public object Current
		{
			get
			{
				return null;
			}
		}

		public bool MoveNext()
		{
			return !this.IsDone();
		}

		public void Reset()
		{
		}

		public abstract bool Update();

		public abstract bool IsDone();
	}
}
