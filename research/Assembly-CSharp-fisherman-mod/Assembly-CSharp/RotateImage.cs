using System;
using UnityEngine;

public class RotateImage : MonoBehaviour
{
	private void FixedUpdate()
	{
		base.GetComponent<RectTransform>().Rotate(new Vector3(0f, 0f, 1f), this._angle);
	}

	[SerializeField]
	private float _angle = -7f;
}
