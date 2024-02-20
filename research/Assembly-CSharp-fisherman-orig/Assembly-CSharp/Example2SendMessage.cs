using System;
using UnityEngine;

public class Example2SendMessage : MonoBehaviour
{
	public void createCubes()
	{
		Material material = GameObject.Find("Cube1").GetComponent<MeshRenderer>().material;
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				GameObject gameObject = GameObject.CreatePrimitive(3);
				gameObject.AddComponent<Rigidbody>();
				gameObject.GetComponent<MeshRenderer>().material = material;
				gameObject.transform.position = new Vector3((float)(j + 5), (float)(i + 2), 0f);
			}
		}
	}
}
