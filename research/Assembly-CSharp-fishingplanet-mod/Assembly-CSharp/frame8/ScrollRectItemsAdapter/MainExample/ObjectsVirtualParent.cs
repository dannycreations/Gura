using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.MainExample
{
	public class ObjectsVirtualParent : MonoBehaviour
	{
		private void OnEnable()
		{
			if (this.delayInit)
			{
				base.StartCoroutine(this.InitCoroutine());
			}
			else
			{
				this.Init();
			}
		}

		private IEnumerator InitCoroutine()
		{
			yield return null;
			yield return null;
			this.Init();
			yield break;
		}

		private void Init()
		{
			if (this.virtualChildren.Length == 0)
			{
				List<GameObject> list = new List<GameObject>();
				if (this.lazyInitSaveMemory)
				{
					for (int i = 0; i < this.count; i++)
					{
						GameObject gameObject = Object.Instantiate<GameObject>(this.prefabIfLazyInit);
						gameObject.transform.SetParent(this.realParent.transform, false);
						list.Add(gameObject);
						gameObject.SetActive(true);
					}
				}
				else
				{
					for (int j = this.startIndexIfNoLazyInit; j < this.startIndexIfNoLazyInit + this.count; j++)
					{
						GameObject gameObject2 = this.realParent.transform.GetChild(j).gameObject;
						list.Add(gameObject2);
						gameObject2.SetActive(true);
					}
				}
				this.virtualChildren = list.ToArray();
			}
			else
			{
				foreach (GameObject gameObject3 in this.virtualChildren)
				{
					gameObject3.SetActive(true);
				}
			}
		}

		private void OnDisable()
		{
			base.StopAllCoroutines();
			if (this.lazyInitSaveMemory)
			{
				foreach (GameObject gameObject in this.virtualChildren)
				{
					gameObject.SetActive(false);
					Object.Destroy(gameObject);
				}
				this.virtualChildren = new GameObject[0];
			}
			else
			{
				foreach (GameObject gameObject2 in this.virtualChildren)
				{
					gameObject2.SetActive(false);
				}
			}
		}

		public GameObject realParent;

		public int startIndexIfNoLazyInit;

		public int count;

		public GameObject prefabIfLazyInit;

		public bool lazyInitSaveMemory;

		public GameObject[] virtualChildren;

		public bool delayInit = true;
	}
}
