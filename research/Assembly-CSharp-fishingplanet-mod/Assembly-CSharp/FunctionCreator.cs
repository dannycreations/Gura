using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FunctionCreator : MonoBehaviour
{
	public void Refresh()
	{
		for (int i = 0; i < this._functions.Count; i++)
		{
			FunctionCreator.Function function = this._functions[i];
			if (function.Curve != null)
			{
				function.X = string.Join(" ", function.Curve.keys.Select((Keyframe k) => string.Format("{0:f3}", k.time)).ToArray<string>());
				function.Y = string.Join(" ", function.Curve.keys.Select((Keyframe k) => string.Format("{0:f3}", k.value)).ToArray<string>());
			}
		}
	}

	[SerializeField]
	private string _url;

	[SerializeField]
	private List<FunctionCreator.Function> _functions;

	[Serializable]
	public class Function
	{
		public string Name;

		public AnimationCurve Curve;

		public string X;

		public string Y;

		public string FunctionStr;
	}
}
