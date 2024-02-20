using System;
using UnityEngine;

public class DontDestroyObject : MonoBehaviour
{
	private void Start()
	{
		Object.DontDestroyOnLoad(this);
	}
}
