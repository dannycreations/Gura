using System;
using UnityEngine;

public class SplineBendMarker : MonoBehaviour
{
	public void Init(SplineBend script, int mnum)
	{
		this.splineScript = script;
		this.num = mnum;
		this.up = base.transform.up;
		this.position = script.transform.InverseTransformPoint(base.transform.position);
		SplineBendMarker splineBendMarker = null;
		SplineBendMarker splineBendMarker2 = null;
		if (this.num > 0)
		{
			splineBendMarker2 = this.splineScript.markers[this.num - 1];
		}
		if (this.num < this.splineScript.markers.Length - 1)
		{
			splineBendMarker = this.splineScript.markers[this.num + 1];
		}
		if (splineBendMarker2 != null)
		{
			this.dist = splineBendMarker2.dist + SplineBend.GetBeizerLength(splineBendMarker2, this);
		}
		else
		{
			this.dist = 0f;
		}
		if (this.splineScript.closed && this.num == this.splineScript.markers.Length - 1)
		{
			splineBendMarker = this.splineScript.markers[this.splineScript.markers.Length - 2];
			splineBendMarker = this.splineScript.markers[1];
		}
		if (splineBendMarker != null)
		{
			if (this.subPoints == null)
			{
				this.subPoints = new Vector3[10];
			}
			float num = 1f / (float)(this.subPoints.Length - 1);
			for (int i = 0; i < this.subPoints.Length; i++)
			{
				this.subPoints[i] = SplineBend.AlignPoint(this, splineBendMarker, num * (float)i, Vector3.zero);
			}
			float num2 = 0f;
			this.subPointPercents[0] = 0f;
			float num3 = 1f / (float)(this.subPoints.Length - 1);
			for (int j = 1; j < this.subPoints.Length; j++)
			{
				this.subPointPercents[j] = num2 + (this.subPoints[j - 1] - this.subPoints[j]).magnitude;
				num2 = this.subPointPercents[j];
				this.subPointMustPercents[j] = num3 * (float)j;
			}
			for (int k = 1; k < this.subPoints.Length; k++)
			{
				this.subPointPercents[k] = this.subPointPercents[k] / num2;
			}
			for (int l = 0; l < this.subPoints.Length - 1; l++)
			{
				this.subPointFactors[l] = num3 / (this.subPointPercents[l + 1] - this.subPointPercents[l]);
			}
		}
		Vector3 vector = Vector3.zero;
		if (splineBendMarker != null)
		{
			vector = script.transform.InverseTransformPoint(splineBendMarker.transform.position);
		}
		SplineBendMarker.MarkerType markerType = this.type;
		if (markerType != SplineBendMarker.MarkerType.Smooth)
		{
			if (markerType != SplineBendMarker.MarkerType.Transform)
			{
				if (markerType == SplineBendMarker.MarkerType.Corner)
				{
					if (splineBendMarker2 != null)
					{
						this.prewHandle = (splineBendMarker2.position - this.position) * 0.333f;
					}
					else
					{
						this.prewHandle = Vector3.zero;
					}
					if (splineBendMarker != null)
					{
						this.nextHandle = (vector - this.position) * 0.333f;
					}
					else
					{
						this.nextHandle = Vector3.zero;
					}
				}
			}
			else
			{
				if (splineBendMarker2 != null)
				{
					float magnitude = (this.position - splineBendMarker2.position).magnitude;
					this.prewHandle = -base.transform.forward * base.transform.localScale.z * magnitude * 0.4f;
				}
				if (splineBendMarker != null)
				{
					float magnitude2 = (this.position - vector).magnitude;
					this.nextHandle = base.transform.forward * base.transform.localScale.z * magnitude2 * 0.4f;
				}
			}
		}
		else if (splineBendMarker == null)
		{
			this.prewHandle = (splineBendMarker2.position - this.position) * 0.333f;
			this.nextHandle = -this.prewHandle * 0.99f;
		}
		else if (splineBendMarker2 == null)
		{
			this.nextHandle = (vector - this.position) * 0.333f;
			this.prewHandle = -this.nextHandle * 0.99f;
		}
		else
		{
			this.nextHandle = Vector3.Slerp(-(splineBendMarker2.position - this.position) * 0.333f, (vector - this.position) * 0.333f, 0.5f);
			this.prewHandle = Vector3.Slerp((splineBendMarker2.position - this.position) * 0.333f, -(vector - this.position) * 0.333f, 0.5f);
		}
		if ((double)(this.nextHandle - this.prewHandle).sqrMagnitude < 0.01)
		{
			this.nextHandle += new Vector3(0.001f, 0f, 0f);
		}
	}

	public SplineBend splineScript;

	private int num;

	public SplineBendMarker.MarkerType type;

	[HideInInspector]
	public Vector3 position;

	[HideInInspector]
	public Vector3 up;

	[HideInInspector]
	public Vector3 prewHandle;

	[HideInInspector]
	public Vector3 nextHandle;

	[HideInInspector]
	public float dist;

	[HideInInspector]
	public float percent;

	[HideInInspector]
	public Vector3[] subPoints = new Vector3[10];

	[HideInInspector]
	public float[] subPointPercents = new float[10];

	[HideInInspector]
	public float[] subPointFactors = new float[10];

	[HideInInspector]
	public float[] subPointMustPercents = new float[10];

	public bool expandWithScale;

	[HideInInspector]
	public Vector3 oldPos;

	[HideInInspector]
	public Vector3 oldScale;

	[HideInInspector]
	public Quaternion oldRot;

	public enum MarkerType
	{
		Smooth,
		Transform,
		Beizer,
		BeizerCorner,
		Corner,
		Simple
	}
}
