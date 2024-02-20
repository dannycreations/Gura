using System;
using UnityEngine;

public class LensFlareFixedDistance : MonoBehaviour
{
	private void Start()
	{
		if (this.Flare == null)
		{
			this.Flare = base.GetComponent<LensFlare>();
		}
		if (this.Flare == null)
		{
			Debug.LogWarning("No LensFlare on " + base.name + ", destroying.", this);
			Object.Destroy(this);
			return;
		}
		this.Size = this.Flare.brightness;
	}

	private void Update()
	{
		if (Camera.main != null)
		{
			float num = Mathf.Sqrt(Vector3.Distance(base.transform.position, Camera.main.transform.position));
			this.Flare.brightness = this.Size / num;
		}
	}

	private float Size;

	public LensFlare Flare;
}
