using System;
using UnityEngine;

public class DebugTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Trigger zone entered by: " + other.name);
	}

	private void OnTriggerStay(Collider other)
	{
		if (Time.time - this._lastTick > this._frequency)
		{
			this._lastTick = Time.time;
			Debug.Log("Trigger zone stay triggered by: " + other.name);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Debug.Log("Trigger zone exited by: " + other.name);
	}

	public string MatchTag = "Player";

	private float _frequency = 1f;

	private float _lastTick;
}
