using System;
using UnityEngine;

public class SimpleTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.tag == this._tag)
		{
			this.ETriggerEnter(collider);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (collider.gameObject.tag == this._tag)
		{
			this.ETriggerExit(collider);
		}
	}

	[SerializeField]
	private string _tag = "AmbientObject";

	public Action<Collider> ETriggerEnter = delegate
	{
	};

	public Action<Collider> ETriggerExit = delegate
	{
	};
}
