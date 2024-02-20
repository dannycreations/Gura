using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nss.Udt.Pooling
{
	public class Pool : MonoBehaviour
	{
		public void Initialize()
		{
			this.initialSize = this.size;
			this.poolRoot = base.gameObject.transform;
			base.name = string.Format("pool-{0}", this.prefab.name);
			this.pooledObjects = new List<GameObject>();
			for (int i = 0; i < this.size; i++)
			{
				GameObject gameObject = this.AddToPool();
				if (this.hideInHierarchy)
				{
					gameObject.hideFlags = 1;
				}
			}
			this.currentIndex = 0;
		}

		public GameObject GetNext(bool activate)
		{
			GameObject gameObject = this.GetNextAvailable();
			if (gameObject == null)
			{
				gameObject = this.IncreasePool();
			}
			if (gameObject == null && !this.suppressLimitErrors)
			{
				Debug.LogError("POOL: Unable to increase pool size.  All instances used and limit reached.");
				return null;
			}
			gameObject.SetActive(activate);
			gameObject.SendMessage("OnInstantiated", 1);
			return gameObject;
		}

		public GameObject GetNext(Vector3 position, bool activate)
		{
			GameObject next = this.GetNext(false);
			next.transform.position = position;
			next.SetActive(activate);
			next.SendMessage("OnInstantiated", 1);
			return next;
		}

		public GameObject GetNext(Vector3 position, Transform parent, bool activate)
		{
			GameObject next = this.GetNext(position, false);
			next.transform.parent = parent;
			next.SetActive(activate);
			next.SendMessage("OnInstantiated", 1);
			return next;
		}

		public void Destroy(GameObject pooledItem)
		{
			pooledItem.SendMessage("OnDestroyed", 1);
			pooledItem.transform.parent = base.transform;
			pooledItem.SetActive(false);
		}

		public void DestroyAll()
		{
			if (this.pooledObjects != null)
			{
				for (int i = 0; i < this.pooledObjects.Count; i++)
				{
					Object.Destroy(this.pooledObjects[i]);
				}
				this.pooledObjects.Clear();
			}
		}

		private void Update()
		{
			if (this.shrinkBack)
			{
				this.Shrink();
			}
		}

		private void Shrink()
		{
			int num = this.size - this.initialSize;
			if (num > 0)
			{
				for (int i = 0; i < this.size - 1; i++)
				{
					if (!this.pooledObjects[i].activeInHierarchy)
					{
						Object.Destroy(this.pooledObjects[i]);
						this.pooledObjects.RemoveAt(i);
						this.size--;
						break;
					}
				}
			}
		}

		private GameObject GetNextAvailable()
		{
			if (this.currentIndex >= this.size)
			{
				this.currentIndex = 0;
			}
			for (int i = this.currentIndex; i < this.size; i++)
			{
				if (!this.pooledObjects[i].activeInHierarchy)
				{
					this.currentIndex++;
					return this.pooledObjects[i];
				}
			}
			return null;
		}

		private GameObject IncreasePool()
		{
			if (this.limit && this.size >= this.limitSize)
			{
				return null;
			}
			GameObject gameObject = this.AddToPool();
			this.size++;
			return gameObject;
		}

		private GameObject AddToPool()
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.prefab);
			gameObject.SetActive(false);
			gameObject.transform.parent = this.poolRoot;
			PooledItem pooledItem = gameObject.GetComponent<PooledItem>();
			if (pooledItem == null)
			{
				pooledItem = gameObject.AddComponent<PooledItem>();
			}
			pooledItem.sourcePool = base.name;
			this.pooledObjects.Add(gameObject);
			return gameObject;
		}

		public GameObject prefab;

		public int size = 1;

		public bool limit;

		public bool suppressLimitErrors;

		public int limitSize;

		public bool hideInHierarchy = true;

		public bool shrinkBack = true;

		private Transform poolRoot;

		private int currentIndex;

		private int initialSize;

		private List<GameObject> pooledObjects;
	}
}
