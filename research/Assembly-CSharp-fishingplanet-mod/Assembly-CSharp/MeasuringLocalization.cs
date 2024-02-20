using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MeasuringLocalization : MonoBehaviour
{
	private void Awake()
	{
		MeasuringLocalization.measuring measure = this._measure;
		if (measure != MeasuringLocalization.measuring.length)
		{
			if (measure == MeasuringLocalization.measuring.weight)
			{
				base.GetComponent<Text>().text = MeasuringSystemManager.FishWeightSufix();
			}
		}
		else
		{
			base.GetComponent<Text>().text = MeasuringSystemManager.FishLengthSufix();
		}
	}

	[SerializeField]
	private MeasuringLocalization.measuring _measure;

	public enum measuring
	{
		length,
		weight
	}
}
