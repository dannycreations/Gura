using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MB_DynamicAddDeleteExample : MonoBehaviour
{
	private float GaussianValue()
	{
		float num;
		float num3;
		do
		{
			num = 2f * Random.Range(0f, 1f) - 1f;
			float num2 = 2f * Random.Range(0f, 1f) - 1f;
			num3 = num * num + num2 * num2;
		}
		while (num3 >= 1f);
		num3 = Mathf.Sqrt(-2f * Mathf.Log(num3) / num3);
		return num * num3;
	}

	private void Start()
	{
		this.mbd = base.GetComponentInChildren<MB3_MeshBaker>();
		int num = 25;
		GameObject[] array = new GameObject[num * num];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.prefab);
				array[i * num + j] = gameObject.GetComponentInChildren<MeshRenderer>().gameObject;
				float num2 = Random.Range(-4f, 4f);
				float num3 = Random.Range(-4f, 4f);
				gameObject.transform.position = new Vector3(9f * (float)i + num2, 0f, 9f * (float)j + num3);
				float num4 = (float)Random.Range(0, 360);
				gameObject.transform.rotation = Quaternion.Euler(0f, num4, 0f);
				Vector3 vector = Vector3.one + Vector3.one * this.GaussianValue() * 0.15f;
				gameObject.transform.localScale = vector;
				if ((i * num + j) % 3 == 0)
				{
					this.objsInCombined.Add(array[i * num + j]);
				}
			}
		}
		this.mbd.AddDeleteGameObjects(array, null, true);
		this.mbd.Apply(null);
		this.objs = this.objsInCombined.ToArray();
		base.StartCoroutine(this.largeNumber());
	}

	private IEnumerator largeNumber()
	{
		for (;;)
		{
			yield return new WaitForSeconds(1.5f);
			this.mbd.AddDeleteGameObjects(null, this.objs, true);
			this.mbd.Apply(null);
			yield return new WaitForSeconds(1.5f);
			this.mbd.AddDeleteGameObjects(this.objs, null, true);
			this.mbd.Apply(null);
		}
		yield break;
	}

	private void OnGUI()
	{
		GUILayout.Label("Dynamically instantiates game objects. \nRepeatedly adds and removes some of them\n from the combined mesh.", new GUILayoutOption[0]);
	}

	public GameObject prefab;

	private List<GameObject> objsInCombined = new List<GameObject>();

	private MB3_MeshBaker mbd;

	private GameObject[] objs;
}
