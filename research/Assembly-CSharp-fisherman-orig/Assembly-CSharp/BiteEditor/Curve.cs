using System;
using UnityEngine;

namespace BiteEditor
{
	public class Curve : MonoBehaviour
	{
		public SimpleCurve Shape
		{
			get
			{
				return new SimpleCurve(this._shape, this._curvePoints);
			}
		}

		public int ExportId;

		[SerializeField]
		private AnimationCurve _shape;

		[SerializeField]
		private int _curvePoints = 10;
	}
}
