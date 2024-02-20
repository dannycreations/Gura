using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClipData
{
	public AnimationClip CurveClip
	{
		get
		{
			return this._curveClip;
		}
	}

	public GameObject Fbx
	{
		get
		{
			return this._fbx;
		}
	}

	public List<ClipData.CurveRecord> Curves
	{
		get
		{
			return this._curves;
		}
	}

	public List<ClipData.AnimationRecord> AllClipStartTimes
	{
		get
		{
			return this._allClipStartTimes;
		}
	}

	[SerializeField]
	private AnimationClip _curveClip;

	[SerializeField]
	private GameObject _fbx;

	[SerializeField]
	private List<ClipData.CurveRecord> _curves = new List<ClipData.CurveRecord>();

	[SerializeField]
	private List<ClipData.AnimationRecord> _allClipStartTimes = new List<ClipData.AnimationRecord>();

	[Serializable]
	public class CurveRecord
	{
		public CurveRecord(string name, AnimationCurve curve)
		{
			this._name = name;
			this._curve = curve;
		}

		public string Name
		{
			get
			{
				return this._name;
			}
		}

		public AnimationCurve Curve
		{
			get
			{
				return this._curve;
			}
		}

		[SerializeField]
		private string _name;

		[SerializeField]
		private AnimationCurve _curve;
	}

	[Serializable]
	public class AnimationRecord
	{
		public AnimationRecord(string clipName, float startTime)
		{
			this._clipName = clipName;
			this._startTime = startTime;
		}

		public string ClipName
		{
			get
			{
				return this._clipName;
			}
		}

		public float StartTime
		{
			get
			{
				return this._startTime;
			}
		}

		[SerializeField]
		private string _clipName;

		[SerializeField]
		private float _startTime;
	}
}
