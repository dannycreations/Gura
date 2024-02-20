using System;
using UnityEngine;

namespace BiteEditor
{
	public class TimeChartCurve : MonoBehaviour
	{
		public AnimationCurve Chart
		{
			get
			{
				return this._chart;
			}
		}

		[SerializeField]
		private AnimationCurve _chart;
	}
}
