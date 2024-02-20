using System;
using UnityEngine;

public class QuitScript : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(27))
		{
			Application.Quit();
			return;
		}
	}
}
