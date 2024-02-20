using System;
using UnityEngine;

namespace Nss.Udt.Pooling.Examples
{
	public class DieOverTime : MonoBehaviour
	{
		private void OnEnable()
		{
			if (!this.preloaded)
			{
				this.preloaded = true;
				return;
			}
			PoolController.Instance.Destroy(base.gameObject, this.delay);
		}

		private void OnInstantiated()
		{
			Debug.Log("instantiated!");
			PoolController.Instance.Destroy(base.gameObject, this.delay);
		}

		private void OnDestroyed()
		{
			Debug.Log("removed!");
		}

		public float delay = 3f;

		private bool preloaded;
	}
}
