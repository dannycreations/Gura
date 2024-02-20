using System;
using UnityEngine;

public class Effects : MonoBehaviour
{
	private void Start()
	{
		Effects.instance_ = this;
	}

	private void Update()
	{
	}

	public static Effects getInstance()
	{
		return Effects.instance_;
	}

	public void spawn(string effectName, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject = null;
		for (int i = 0; i < this.effects.Length; i++)
		{
			if (this.effects[i] != null && this.effects[i].name == effectName)
			{
				gameObject = this.effects[i];
				break;
			}
		}
		if (gameObject == null)
		{
			Debug.LogWarning("Particles: can't find particles " + effectName);
			return;
		}
		Object.Instantiate<GameObject>(gameObject, position, rotation);
	}

	public GameObject[] effects;

	private static Effects instance_;
}
