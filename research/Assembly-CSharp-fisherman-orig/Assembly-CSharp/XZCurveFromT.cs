using System;
using UnityEngine;

public class XZCurveFromT : MonoBehaviour
{
	public Vector2 Evaluate(float t)
	{
		t = Mathf.Clamp01(t);
		return new Vector2(this._xCurve.Evaluate(t), this._zCurve.Evaluate(t));
	}

	[SerializeField]
	private AnimationCurve _xCurve;

	[SerializeField]
	private AnimationCurve _zCurve;
}
