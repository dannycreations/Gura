using System;
using I2.Loc;

public sealed class CompassDirectionHelper
{
	public CompassDirectionHelper()
	{
		this._directions[0] = new CompassDirectionHelper.Direction
		{
			Left = ScriptLocalization.Get("W"),
			Right = ScriptLocalization.Get("E"),
			DegreeLeft = this._stepDegree,
			DegreeRight = 360f - this._stepDegree
		};
		this._directions[1] = new CompassDirectionHelper.Direction
		{
			Left = ScriptLocalization.Get("E"),
			Right = ScriptLocalization.Get("W"),
			DegreeLeft = 180f - this._stepDegree,
			DegreeRight = 180f + this._stepDegree
		};
		this._directions[2] = new CompassDirectionHelper.Direction
		{
			Left = ScriptLocalization.Get("S"),
			Right = ScriptLocalization.Get("N"),
			DegreeLeft = 90f - this._stepDegree,
			DegreeRight = 90f + this._stepDegree
		};
		this._directions[3] = new CompassDirectionHelper.Direction
		{
			Left = ScriptLocalization.Get("N"),
			Right = ScriptLocalization.Get("S"),
			DegreeLeft = 270f - this._stepDegree,
			DegreeRight = 270f + this._stepDegree
		};
	}

	public void Get2Directions(float degree, out string left, out string right)
	{
		left = this._directions[0].Left;
		right = this._directions[0].Right;
		for (int i = this._directions.Length - 1; i > 0; i--)
		{
			CompassDirectionHelper.Direction direction = this._directions[i];
			if (degree >= direction.DegreeLeft && degree < direction.DegreeRight)
			{
				left = direction.Left;
				right = direction.Right;
				break;
			}
		}
	}

	public const string North = "N";

	public const string South = "S";

	public const string East = "E";

	public const string West = "W";

	private readonly float _stepDegree = 45f;

	private readonly CompassDirectionHelper.Direction[] _directions = new CompassDirectionHelper.Direction[4];

	private class Direction
	{
		public string Left { get; set; }

		public string Right { get; set; }

		public float DegreeLeft { get; set; }

		public float DegreeRight { get; set; }
	}
}
