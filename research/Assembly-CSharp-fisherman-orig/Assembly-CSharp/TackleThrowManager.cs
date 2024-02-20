using System;
using Phy;
using UnityEngine;

public class TackleThrowManager
{
	public TackleThrowManager(TackleThrowData throwData)
	{
		this.throwData = throwData;
	}

	public static VerticalParabola GetPath(Vector3 startPoint, Vector3 finishPoint, float startAngle, out float FlightDuration)
	{
		float magnitude = (finishPoint - startPoint).magnitude;
		float num = Mathf.Sqrt(magnitude * 9.81f / Mathf.Sin(2f * startAngle));
		FlightDuration = magnitude / (num * Mathf.Cos(startAngle));
		float num2 = startPoint.y + magnitude * 0.5f * Mathf.Tan(startAngle) - 9.81f * magnitude * magnitude * 0.25f / (2f * Mathf.Pow(num * Mathf.Cos(startAngle), 2f));
		return new VerticalParabola(startPoint, finishPoint, num2);
	}

	public VerticalParabola GetPath(Vector3 WindVelocity)
	{
		Vector3 vector = GameFactory.Player.transform.position + (new Vector3(this.directXCos, 0f, this.directZCos) + WindVelocity * this.throwData.Windage * 0.01f) * this.castLength;
		vector.y = 0f;
		this.DestinationPoint = vector;
		return TackleThrowManager.GetPath(GameFactory.Player.transform.position, vector, this.angleCast, out this.FlightDuration);
	}

	public void Init(float currentTipPositionY)
	{
		this.castLength = this.throwData.CastLength + this.throwData.AccuracyValue * Random.Range(-1f, 1f);
		float num = Mathf.Tan(this.throwData.AccuracyValue / this.throwData.CastLength) * 57.29578f;
		this.castLength = Mathf.Max(this.castLength, this.throwData.RodLength);
		this.castMaxLength = this.throwData.MaxCastLength;
		this.angleCast = 0.7853982f * Mathf.Pow(this.castLength / this.castMaxLength, 0.3f);
		this.h = currentTipPositionY;
		this.vX = 5f + Mathf.Tan(this.angleCast) * this.castLength * Mathf.Sqrt(9.81f * this.throwData.Windage / 2f / (this.castLength * Mathf.Tan(this.angleCast) + this.h));
		this.vXdyn = this.vX * (1f - (1f - this.throwData.Windage) / this.castLength * 0f);
		this.xCur = 0f;
		this.directXCos = Mathf.Cos(Vector3.Angle(Quaternion.AngleAxis(num, Vector3.up) * this.throwData.Direction, new Vector3(1f, 0f, 0f)) * 3.1415927f / 180f);
		this.directZCos = Mathf.Cos(Vector3.Angle(Quaternion.AngleAxis(num, Vector3.up) * this.throwData.Direction, new Vector3(0f, 0f, 1f)) * 3.1415927f / 180f);
		this.throwData.IsThrowing = false;
	}

	private const float WindDeviationFactor = 0.01f;

	private readonly TackleThrowData throwData;

	private float castLength;

	public float angleCast;

	private float castMaxLength;

	private float vX;

	private float vXdyn;

	private float xCur;

	private float h;

	private float directXCos;

	private float directZCos;

	public float FlightDuration;

	public Vector3 DestinationPoint;
}
