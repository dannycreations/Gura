using System;
using UnityEngine;

[ExecuteInEditMode]
public class BoxMeashurer : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		this.transforms = string.Concat(new object[]
		{
			"Position: { X: ",
			base.transform.position.x,
			", Y: ",
			base.transform.position.y,
			", Z: ",
			base.transform.position.z,
			" },  Rotation: { X: ",
			base.transform.eulerAngles.x,
			", Y: ",
			base.transform.eulerAngles.y,
			", Z: ",
			base.transform.eulerAngles.z,
			" },  Scale:    { X: ",
			base.transform.localScale.x,
			", Y: ",
			base.transform.localScale.y,
			", Z: ",
			base.transform.localScale.z,
			" }"
		});
	}

	public string transforms;
}
