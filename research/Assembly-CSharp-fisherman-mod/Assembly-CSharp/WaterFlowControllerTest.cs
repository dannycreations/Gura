using System;
using UnityEngine;

public class WaterFlowControllerTest : MonoBehaviour
{
	internal void Start()
	{
	}

	internal void Update()
	{
		if (GameFactory.WaterFlow != null)
		{
			base.GetComponent<Rigidbody>().velocity = GameFactory.WaterFlow.GetStreamSpeed(base.transform.position);
		}
	}
}
