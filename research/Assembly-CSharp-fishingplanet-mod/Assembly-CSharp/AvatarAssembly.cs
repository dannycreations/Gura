using System;
using UnityEngine;

public class AvatarAssembly : MonoBehaviour
{
	private void Start()
	{
		this.PartModels = new GameObject[this.Parts.Length];
		int num = 0;
		foreach (string text in this.Parts)
		{
			GameObject gameObject = (GameObject)Resources.Load(text, typeof(GameObject));
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
			gameObject2.transform.parent = base.gameObject.transform;
			gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
			this.PartModels[num] = gameObject2;
			num++;
		}
	}

	private void Update()
	{
	}

	public string[] Parts;

	private GameObject[] PartModels;
}
