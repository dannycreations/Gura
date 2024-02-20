using System;
using UnityEngine;

namespace InControl
{
	public class Touch
	{
		internal Touch()
		{
			this.fingerId = Touch.FingerID_None;
			this.phase = 3;
		}

		internal void Reset()
		{
			this.fingerId = Touch.FingerID_None;
			this.phase = 3;
			this.tapCount = 0;
			this.position = Vector2.zero;
			this.deltaPosition = Vector2.zero;
			this.lastPosition = Vector2.zero;
			this.deltaTime = 0f;
			this.updateTick = 0UL;
			this.type = TouchType.Direct;
			this.altitudeAngle = 0f;
			this.azimuthAngle = 0f;
			this.maximumPossiblePressure = 0f;
			this.pressure = 0f;
			this.radius = 0f;
			this.radiusVariance = 0f;
		}

		internal void SetWithTouchData(Touch touch, ulong updateTick, float deltaTime)
		{
			this.phase = touch.phase;
			this.tapCount = touch.tapCount;
			this.altitudeAngle = touch.altitudeAngle;
			this.azimuthAngle = touch.azimuthAngle;
			this.maximumPossiblePressure = touch.maximumPossiblePressure;
			this.pressure = touch.pressure;
			this.radius = touch.radius;
			this.radiusVariance = touch.radiusVariance;
			Vector2 vector = touch.position;
			if (vector.x < 0f)
			{
				vector.x = (float)Screen.width + vector.x;
			}
			if (this.phase == null)
			{
				this.deltaPosition = Vector2.zero;
				this.lastPosition = vector;
				this.position = vector;
			}
			else
			{
				if (this.phase == 2)
				{
					this.phase = 1;
				}
				this.deltaPosition = vector - this.lastPosition;
				this.lastPosition = this.position;
				this.position = vector;
			}
			this.deltaTime = deltaTime;
			this.updateTick = updateTick;
		}

		internal bool SetWithMouseData(ulong updateTick, float deltaTime)
		{
			if (Input.touchCount > 0)
			{
				return false;
			}
			Vector2 vector;
			vector..ctor(Mathf.Round(Input.mousePosition.x), Mathf.Round(Input.mousePosition.y));
			if (Input.GetMouseButtonDown(0))
			{
				this.phase = 0;
				this.tapCount = 1;
				this.type = TouchType.Mouse;
				this.deltaPosition = Vector2.zero;
				this.lastPosition = vector;
				this.position = vector;
				this.deltaTime = deltaTime;
				this.updateTick = updateTick;
				return true;
			}
			if (Input.GetMouseButtonUp(0))
			{
				this.phase = 3;
				this.tapCount = 1;
				this.type = TouchType.Mouse;
				this.deltaPosition = vector - this.lastPosition;
				this.lastPosition = this.position;
				this.position = vector;
				this.deltaTime = deltaTime;
				this.updateTick = updateTick;
				return true;
			}
			if (Input.GetMouseButton(0))
			{
				this.phase = 1;
				this.tapCount = 1;
				this.type = TouchType.Mouse;
				this.deltaPosition = vector - this.lastPosition;
				this.lastPosition = this.position;
				this.position = vector;
				this.deltaTime = deltaTime;
				this.updateTick = updateTick;
				return true;
			}
			return false;
		}

		public static readonly int FingerID_None = -1;

		public static readonly int FingerID_Mouse = -2;

		public int fingerId;

		public TouchPhase phase;

		public int tapCount;

		public Vector2 position;

		public Vector2 deltaPosition;

		public Vector2 lastPosition;

		public float deltaTime;

		public ulong updateTick;

		public TouchType type;

		public float altitudeAngle;

		public float azimuthAngle;

		public float maximumPossiblePressure;

		public float pressure;

		public float radius;

		public float radiusVariance;
	}
}
