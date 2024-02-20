using System;
using Newtonsoft.Json;

namespace BiteEditor.ObjectModel
{
	public class Curve
	{
		public Curve(int id, string unityName, SimpleCurve curveShape)
		{
			this.Id = id;
			this.UnityName = unityName;
			this.CurveShape = curveShape;
		}

		public Curve()
		{
		}

		[JsonProperty]
		public int Id { get; private set; }

		[JsonProperty]
		public string UnityName { get; private set; }

		[JsonProperty]
		public SimpleCurve CurveShape { get; private set; }
	}
}
