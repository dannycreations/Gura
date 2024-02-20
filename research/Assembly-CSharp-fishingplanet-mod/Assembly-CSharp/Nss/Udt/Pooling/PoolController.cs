using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nss.Udt.Pooling
{
	public class PoolController : MonoBehaviour
	{
		private void OnEnable()
		{
			this.Initialize();
		}

		private void OnDisable()
		{
			PoolController.Instance = null;
			this.DestroyAll();
		}

		public GameObject GetNext(string poolName, bool activate)
		{
			poolName = string.Format("pool-{0}", poolName);
			Pool pool = this.poolTable[poolName] as Pool;
			if (pool == null)
			{
				Debug.LogError(string.Format("POOL: Pool doesn't exist. [poolName: '{0}']", poolName));
				return null;
			}
			return pool.GetNext(activate);
		}

		public GameObject GetNext(string poolName, Vector3 position, bool activate)
		{
			poolName = string.Format("pool-{0}", poolName);
			Pool pool = this.poolTable[poolName] as Pool;
			if (pool == null)
			{
				Debug.LogError(string.Format("POOL: Pool doesn't exist. [poolName: '{0}']", poolName));
				return null;
			}
			return pool.GetNext(position, activate);
		}

		public GameObject GetNext(string poolName, Vector3 position, Transform parent, bool activate)
		{
			poolName = string.Format("pool-{0}", poolName);
			Pool pool = this.poolTable[poolName] as Pool;
			if (pool == null)
			{
				Debug.LogError(string.Format("POOL: Pool doesn't exist. [poolName: '{0}']", poolName));
				return null;
			}
			return pool.GetNext(position, parent, activate);
		}

		public void DestroyAll()
		{
			for (int i = 0; i < this.pools.Count; i++)
			{
				this.pools[i].DestroyAll();
			}
			this.pools.Clear();
			this.poolTable.Clear();
		}

		public void DestroyPool(string poolName)
		{
			Pool pool = this.poolTable[poolName] as Pool;
			if (pool == null)
			{
				Debug.LogError(string.Format("POOL: Pool doesn't exist. [poolName: '{0}']", poolName));
				return;
			}
			pool.DestroyAll();
		}

		public void Destroy(GameObject pooledItem, float delay)
		{
			base.StartCoroutine(this.Despawn(pooledItem, delay));
		}

		public void Destroy(GameObject pooledItem)
		{
			PooledItem component = pooledItem.GetComponent<PooledItem>();
			if (component == null)
			{
				Debug.LogError(string.Format("POOL: PoolItem doesn't exist on GameObject. [GameObject: '{0}']", pooledItem));
				return;
			}
			Pool pool = this.poolTable[component.sourcePool] as Pool;
			if (pool == null)
			{
				Debug.LogError(string.Format("POOL: Pool doesn't exist. [poolName: '{0}']", component.sourcePool));
				return;
			}
			pool.Destroy(pooledItem);
		}

		private void Initialize()
		{
			if (PoolController.Instance != null)
			{
				Debug.LogError("POOL: There can only be one PoolController, detected multiple instances.");
				return;
			}
			if (this.pools != null && this.poolTable != null)
			{
				PoolController.Instance = this;
				return;
			}
			this.pools = new List<Pool>();
			this.poolTable = new Hashtable();
			List<Pool> list = new List<Pool>();
			this.pools.AddRange(base.GetComponentsInChildren<Pool>(true));
			for (int i = 0; i < this.pools.Count; i++)
			{
				if (this.pools[i].prefab == null)
				{
					list.Add(this.pools[i]);
				}
				else
				{
					this.pools[i].name = string.Format("pool-{0}", this.pools[i].prefab.name);
					this.poolTable.Add(this.pools[i].name, this.pools[i]);
					this.pools[i].Initialize();
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				this.pools.Remove(list[j]);
			}
			list.Clear();
			PoolController.Instance = this;
		}

		private IEnumerator Despawn(GameObject pooledItem, float delay)
		{
			yield return new WaitForSeconds(delay);
			this.Destroy(pooledItem);
			yield break;
		}

		public static PoolController Instance;

		public const string ROOT_NAME = "__Pools__";

		private List<Pool> pools;

		private Hashtable poolTable;
	}
}
