using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraPathData
{
	public CameraPathData(List<CameraPoint> points, float movementSpeed, float rotationSpeed)
	{
		if (points.Count > 1)
		{
			CameraPathData.Transition[] array = new CameraPathData.Transition[points.Count - 1];
			float num;
			for (int i = 1; i < points.Count; i++)
			{
				CameraPoint cameraPoint = points[i - 1];
				CameraPoint cameraPoint2 = points[i];
				num = ((cameraPoint2.MovementSpeed >= 0f) ? cameraPoint2.MovementSpeed : movementSpeed);
				float num2 = ((cameraPoint2.RotationSpeed >= 0f) ? cameraPoint2.RotationSpeed : rotationSpeed);
				array[i - 1] = new CameraPathData.Transition(cameraPoint.Position, cameraPoint2.Position, num, cameraPoint.Rotation, cameraPoint2.Rotation, num2, cameraPoint2.Duration);
			}
			this.Sections = new CameraPathData.SpeedFunctions[array.Length];
			num = ((points[0].MovementSpeed >= 0f) ? points[0].MovementSpeed : movementSpeed);
			float num3 = ((points[0].RotationSpeed >= 0f) ? points[0].RotationSpeed : rotationSpeed);
			CameraPathData.SpeedFunction speedFunction = new CameraPathData.SpeedFunction(num);
			CameraPathData.SpeedFunction speedFunction2 = new CameraPathData.SpeedFunction(0f);
			for (int j = 0; j < array.Length - 1; j++)
			{
				CameraPathData.Transition transition = array[j];
				CameraPathData.Transition transition2 = array[j + 1];
				speedFunction = new CameraPathData.SpeedFunction(transition.V, speedFunction.V3, transition2.V);
				speedFunction2 = new CameraPathData.SpeedFunction(transition.W, speedFunction2.V3, transition2.W);
				this.Sections[j] = new CameraPathData.SpeedFunctions(transition.PFrom, transition.PTo, transition.RFrom, transition.RTo, speedFunction, speedFunction2, transition.T);
			}
			CameraPathData.Transition transition3 = array[array.Length - 1];
			CameraPoint cameraPoint3 = points[points.Count - 1];
			speedFunction = new CameraPathData.SpeedFunction(transition3.V, speedFunction.V3, (!cameraPoint3.KeepSpeedTillTheEnd) ? 0f : transition3.V);
			speedFunction2 = new CameraPathData.SpeedFunction(transition3.W, speedFunction2.V3, 0f);
			this.Sections[array.Length - 1] = new CameraPathData.SpeedFunctions(transition3.PFrom, transition3.PTo, transition3.RFrom, transition3.RTo, speedFunction, speedFunction2, transition3.T);
		}
		else
		{
			this.Sections = new CameraPathData.SpeedFunctions[0];
		}
	}

	public readonly CameraPathData.SpeedFunctions[] Sections;

	public class Mover
	{
		public Mover(CameraPathData.SpeedFunctions speedF, float timePassed)
		{
			this._speedF = speedF;
			this._s = (speedF.PFrom - speedF.PTo).magnitude;
			this._angle = Quaternion.Angle(speedF.RFrom, speedF.RTo);
			Vector2 vector = speedF.CalcSpeed(0f);
			this._prevV = vector.x;
			this._prevW = vector.y;
			this._lastT = 0f;
			this.Update(timePassed);
		}

		public float Duration
		{
			get
			{
				return this._speedF.Duration;
			}
		}

		public void Update(float t)
		{
			float num = t - this._lastT;
			this._lastT = t;
			Vector2 vector = this._speedF.CalcSpeed(t);
			float num2 = (this._prevV + vector.x) * 0.5f;
			this._prevV = vector.x;
			this._curS += num2 * num;
			float num3 = (this._prevW + vector.y) * 0.5f;
			this._prevW = vector.y;
			this._curAngle += num3 * num;
		}

		private float MovementPrc
		{
			get
			{
				return ((double)Mathf.Abs(this._s) <= 1E-06) ? 0f : Mathf.Clamp01(this._curS / this._s);
			}
		}

		private float RotationPrc
		{
			get
			{
				return ((double)Mathf.Abs(this._angle) <= 1E-06) ? 0f : Mathf.Clamp01(this._curAngle / this._angle);
			}
		}

		public Vector3 Position
		{
			get
			{
				return Vector3.Lerp(this._speedF.PFrom, this._speedF.PTo, this.MovementPrc);
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return Quaternion.Slerp(this._speedF.RFrom, this._speedF.RTo, this.RotationPrc);
			}
		}

		private CameraPathData.SpeedFunctions _speedF;

		private float _lastT;

		private float _s;

		private float _prevV;

		private float _curS;

		private float _angle;

		private float _prevW;

		private float _curAngle;
	}

	public class SpeedFunctions
	{
		public SpeedFunctions(Vector3 pFrom, Vector3 pTo, Quaternion rFrom, Quaternion rTo, CameraPathData.SpeedFunction movement, CameraPathData.SpeedFunction rotation, float duration)
		{
			this.PFrom = pFrom;
			this.PTo = pTo;
			this.RFrom = rFrom;
			this.RTo = rTo;
			this._movement = movement;
			this._rotation = rotation;
			this.Duration = duration;
		}

		public Vector3 ForwardFrom
		{
			get
			{
				return this.RFrom * Vector3.forward;
			}
		}

		public Vector3 ForwardTo
		{
			get
			{
				return this.RTo * Vector3.forward;
			}
		}

		public CameraPathData.Mover CreateMover(float timePassed)
		{
			return new CameraPathData.Mover(this, timePassed);
		}

		public Vector2 CalcSpeed(float time)
		{
			float num = Mathf.Clamp01(time / this.Duration);
			return new Vector2(this._movement.CalcV(num), this._rotation.CalcV(num));
		}

		public override string ToString()
		{
			return string.Format("Movement: {0}\nRotation: {1}\n", this._movement, this._rotation);
		}

		public readonly Vector3 PFrom;

		public readonly Vector3 PTo;

		public readonly Quaternion RFrom;

		public readonly Quaternion RTo;

		private readonly CameraPathData.SpeedFunction _movement;

		private readonly CameraPathData.SpeedFunction _rotation;

		public readonly float Duration;
	}

	public struct SpeedFunction
	{
		public SpeedFunction(float v)
		{
			this.V3 = v;
			this.V2 = v;
			this.V1 = v;
		}

		public SpeedFunction(float v, float v1, float nextMiddleV)
		{
			if ((double)Math.Abs(v) > 1E-06)
			{
				this.V1 = v1;
				this.V3 = nextMiddleV;
				this.V2 = (8f * v - this.V1 - this.V3) / 6f;
			}
			else
			{
				this.V1 = (this.V2 = (this.V3 = 0f));
			}
		}

		public float Vmedium
		{
			get
			{
				return (this.V1 + 6f * this.V2 + this.V3) / 8f;
			}
		}

		public float CalcV(float t)
		{
			if (t < 0.25f)
			{
				return Mathf.Lerp(this.V1, this.V2, t * 4f);
			}
			if (t < 0.75f)
			{
				return this.V2;
			}
			return Mathf.Lerp(this.V2, this.V3, 1f - (1f - t) * 4f);
		}

		public override string ToString()
		{
			return string.Format("V1 = {0:f2}, V2 = {1:f2}, V3 = {2:f2}, V = {3:f2}", new object[] { this.V1, this.V2, this.V3, this.Vmedium });
		}

		public readonly float V1;

		public readonly float V2;

		public readonly float V3;
	}

	private struct Transition
	{
		public Transition(Vector3 pFrom, Vector3 pTo, float v, Quaternion rFrom, Quaternion rTo, float w, float duration = -1f)
		{
			this.PFrom = pFrom;
			this.PTo = pTo;
			this.RFrom = rFrom;
			this.RTo = rTo;
			float magnitude = (pFrom - pTo).magnitude;
			float num = Quaternion.Angle(rFrom, rTo);
			if (duration >= 0f)
			{
				this.T = duration;
			}
			else if (magnitude > 0f)
			{
				if (v > 0f)
				{
					this.T = magnitude / v;
				}
				else
				{
					if (num <= 0f || w <= 0f)
					{
						throw new Exception("Zero speed for non-zero movement");
					}
					this.T = num / w;
				}
			}
			else
			{
				if (num <= 0f)
				{
					throw new Exception("Impossible to calculate transition time when movement distance, rotation angle and duration are 0");
				}
				if (w <= 0f)
				{
					throw new Exception("Zero rotation speed for non-zero rotation");
				}
				this.T = num / w;
			}
			this.V = magnitude / this.T;
			this.W = num / this.T;
		}

		public readonly Vector3 PFrom;

		public readonly Vector3 PTo;

		public readonly Quaternion RFrom;

		public readonly Quaternion RTo;

		public readonly float V;

		public readonly float W;

		public readonly float T;
	}
}
