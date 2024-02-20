using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCurves : MonoBehaviour
{
	public List<ClipData> ClipsData
	{
		get
		{
			return this._clipsData;
		}
	}

	private void Awake()
	{
		for (int i = 0; i < this._clipsData.Count; i++)
		{
			ClipData clipData = this._clipsData[i];
			for (int j = 0; j < clipData.Curves.Count; j++)
			{
				ClipData.CurveRecord curveRecord = clipData.Curves[j];
				if (!this._curves.ContainsKey(curveRecord.Name))
				{
					this._curves[curveRecord.Name] = curveRecord.Curve;
				}
				else
				{
					LogHelper.Error("Duplicated curve {0} found", new object[] { curveRecord.Name });
				}
			}
			for (int k = 0; k < clipData.AllClipStartTimes.Count; k++)
			{
				this._timeline[clipData.AllClipStartTimes[k].ClipName] = clipData.AllClipStartTimes[k].StartTime;
			}
		}
	}

	public float GetCurveValueForLocalTime(string curveName, string clipName, float localTime)
	{
		if (this._curves.ContainsKey(curveName) && this._timeline.ContainsKey(clipName))
		{
			AnimationCurve animationCurve = this._curves[curveName];
			return animationCurve.Evaluate(localTime + this._timeline[clipName]);
		}
		return 0f;
	}

	[HideInInspector]
	[SerializeField]
	private List<ClipData> _clipsData = new List<ClipData>();

	private Dictionary<string, AnimationCurve> _curves = new Dictionary<string, AnimationCurve>();

	private Dictionary<string, float> _timeline = new Dictionary<string, float>();
}
