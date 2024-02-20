using System;
using UnityEngine;

public class ZoneMessageReceiver : MonoBehaviour
{
	private void OnZoneEnter(Collider other)
	{
		Debug.Log("Enter Message Received: Triggered by " + other.gameObject.name);
	}

	private void OnZoneStay(Collider other)
	{
		Debug.Log("Stay Message Received: Triggered by " + other.gameObject.name);
	}

	private void OnZoneExit(Collider other)
	{
		Debug.Log("Exit Message Received: Triggered by " + other.gameObject.name);
	}
}
