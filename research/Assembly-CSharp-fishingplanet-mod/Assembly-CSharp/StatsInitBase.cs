using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsInitBase : MonoBehaviour
{
	protected virtual void OnDestroy()
	{
		base.StopAllCoroutines();
	}

	public virtual void Init<T>(List<T> list, Action onActive)
	{
		int threadsCount = this.ThreadsCount;
		float num = 0f;
		int i = 0;
		while (i < list.Count)
		{
			base.StartCoroutine(this.AddRange<T>(i, i + threadsCount, num, list, onActive));
			i += threadsCount;
			num += 2f;
		}
	}

	public virtual void Clear()
	{
		base.StopAllCoroutines();
		for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.ContentPanel.transform.GetChild(i).gameObject);
		}
	}

	protected virtual void Add(int i)
	{
	}

	protected virtual IEnumerator AddRange<T>(int start, int end, float t, List<T> list, Action onActive)
	{
		yield return new WaitForSeconds(t);
		for (int i = start; i < end; i++)
		{
			if (i > list.Count - 1)
			{
				if (onActive != null)
				{
					onActive();
				}
				break;
			}
			this.Add(i);
		}
		yield break;
	}

	public GameObject ContentPanel;

	protected int ThreadsCount = 8;
}
