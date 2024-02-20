using System;
using UnityEngine;

public class HideInRetail : MonoBehaviour
{
	private void Awake()
	{
		for (int i = 0; i < this.DisabledObjects.Length; i++)
		{
			this.DisabledObjects[i].SetActive(false);
		}
	}

	public GameObject[] DisabledObjects;
}
