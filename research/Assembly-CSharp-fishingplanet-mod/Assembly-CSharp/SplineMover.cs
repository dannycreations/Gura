using System;
using System.Diagnostics;
using SWS;
using UnityEngine;

public class SplineMover : MonoBehaviour, ISplineMover
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action EEndOfSpline = delegate
	{
	};

	private void Awake()
	{
		base.enabled = false;
	}

	public void ChangeSpeed(float speed)
	{
		this._curSpeed = Mathf.Max(0f, speed);
	}

	public void Pause(float seconds = 0f)
	{
		this._isPaused = true;
	}

	public void Resume()
	{
		this._isPaused = false;
	}

	public void SetPath(PathManager path, float timePrc, float movementSpeed)
	{
		base.enabled = true;
		this._points = path.BuildPathPoints();
		this._distances = new float[this._points.Length - 1];
		this._totalDistance = 0f;
		for (int i = 1; i < this._points.Length; i++)
		{
			this._totalDistance += (this._points[i] - this._points[i - 1]).magnitude;
			this._distances[i - 1] = this._totalDistance;
		}
		this._curDistance = timePrc * this._totalDistance;
		this._path = path;
		this._timePrc = timePrc;
		this._curSpeed = Mathf.Max(0f, movementSpeed);
	}

	private SplineMover.Indices FindNearestIndex(float dist)
	{
		if (dist < 0f)
		{
			return new SplineMover.Indices(0, 0);
		}
		for (int i = 0; i < this._distances.Length; i++)
		{
			if (dist < this._distances[i])
			{
				return new SplineMover.Indices(i, i + 1);
			}
		}
		return new SplineMover.Indices(this._points.Length - 1, this._points.Length - 1);
	}

	private void Update()
	{
		if (!this._isPaused)
		{
			this._curDistance += this._curSpeed * Time.deltaTime;
			SplineMover.Indices indices = this.FindNearestIndex(this._curDistance);
			if (indices.From == indices.To)
			{
				base.transform.position = this._points[indices.From];
				this._timePrc = (float)((indices.From != 0) ? 1 : 0);
			}
			else
			{
				Vector3 vector = this._points[indices.From];
				Vector3 vector2 = this._points[indices.To];
				float num = this._distances[(indices.From <= 0) ? 0 : (indices.From - 1)];
				float num2 = this._distances[indices.To - 1];
				float num3 = (this._curDistance - num) / (num2 - num);
				Vector3 vector3 = Vector3.Lerp(vector, vector2, num3);
				Vector3 vector4 = vector3 - base.transform.position;
				float magnitude = vector4.magnitude;
				if (magnitude > 0f)
				{
					float num4 = -Mathf.Asin(vector4.y / magnitude) * 57.29578f;
					Vector3 vector5;
					vector5..ctor(vector4.x, 0f, vector4.z);
					float num5 = Vector3.SignedAngle(Vector3.forward, vector5, Vector3.up);
					if (!float.IsNaN(num5))
					{
						base.transform.rotation = Quaternion.Euler(num4, num5, 0f);
					}
					base.transform.position = vector3;
				}
				this._timePrc = this._curDistance / this._totalDistance;
			}
			if (this._curDistance > this._totalDistance)
			{
				base.enabled = false;
				this.EEndOfSpline();
			}
		}
	}

	[SerializeField]
	private PathManager _path;

	[SerializeField]
	private float _curDistance;

	[SerializeField]
	private float _timePrc;

	[SerializeField]
	private float _curSpeed;

	[SerializeField]
	private bool _isPaused;

	private Vector3[] _points;

	private float[] _distances;

	private float _totalDistance;

	private struct Indices
	{
		public Indices(int from, int to)
		{
			this.From = from;
			this.To = to;
		}

		public readonly int From;

		public readonly int To;
	}
}
