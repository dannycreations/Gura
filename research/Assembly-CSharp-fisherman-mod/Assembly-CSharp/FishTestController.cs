using System;
using UnityEngine;

public class FishTestController : MonoBehaviour
{
	internal void Start()
	{
		this.FishModels = new GameObject[this.Fish.Length];
		int num = 0;
		foreach (string text in this.Fish)
		{
			GameObject gameObject = (GameObject)Resources.Load(text, typeof(GameObject));
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
			gameObject2.transform.position = new Vector3((float)num * 0.5f, 0f, 0f);
			FishController component = gameObject2.GetComponent<FishController>();
			component.enabled = false;
			this.FishModels[num] = gameObject2;
			num++;
		}
	}

	internal void Update()
	{
		foreach (GameObject gameObject in this.FishModels)
		{
			FishAnimationController component = gameObject.GetComponent<FishAnimationController>();
			FishController component2 = gameObject.GetComponent<FishController>();
			component.SetAnimation(this.Animation);
			if (this.Animation == "hang" || this.Animation == "beating" || this.Animation == "shake")
			{
				gameObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
			}
			else
			{
				gameObject.transform.rotation = Quaternion.identity;
			}
		}
	}

	public string[] Fish;

	private GameObject[] FishModels;

	public string Animation;
}
