using System;
using UnityEngine;

namespace InControl
{
	public class TouchSwipeControl : TouchControl
	{
		public override void CreateControl()
		{
		}

		public override void DestroyControl()
		{
			if (this.currentTouch != null)
			{
				this.TouchEnded(this.currentTouch);
				this.currentTouch = null;
			}
		}

		public override void ConfigureControl()
		{
			this.worldActiveArea = TouchManager.ConvertToWorld(this.activeArea, this.areaUnitType);
		}

		public override void DrawGizmos()
		{
			Utility.DrawRectGizmo(this.worldActiveArea, Color.yellow);
		}

		private void Update()
		{
			if (this.dirty)
			{
				this.ConfigureControl();
				this.dirty = false;
			}
		}

		public override void SubmitControlState(ulong updateTick, float deltaTime)
		{
			Vector3 vector = TouchControl.SnapTo(this.currentVector, this.snapAngles);
			base.SubmitAnalogValue(this.target, vector, 0f, 1f, updateTick, deltaTime);
			base.SubmitButtonState(this.upTarget, this.fireButtonTarget && this.nextButtonTarget == this.upTarget, updateTick, deltaTime);
			base.SubmitButtonState(this.downTarget, this.fireButtonTarget && this.nextButtonTarget == this.downTarget, updateTick, deltaTime);
			base.SubmitButtonState(this.leftTarget, this.fireButtonTarget && this.nextButtonTarget == this.leftTarget, updateTick, deltaTime);
			base.SubmitButtonState(this.rightTarget, this.fireButtonTarget && this.nextButtonTarget == this.rightTarget, updateTick, deltaTime);
			base.SubmitButtonState(this.tapTarget, this.fireButtonTarget && this.nextButtonTarget == this.tapTarget, updateTick, deltaTime);
			if (this.fireButtonTarget && this.nextButtonTarget != TouchControl.ButtonTarget.None)
			{
				this.fireButtonTarget = !this.oneSwipePerTouch;
				this.lastButtonTarget = this.nextButtonTarget;
				this.nextButtonTarget = TouchControl.ButtonTarget.None;
			}
		}

		public override void CommitControlState(ulong updateTick, float deltaTime)
		{
			base.CommitAnalog(this.target);
			base.CommitButton(this.upTarget);
			base.CommitButton(this.downTarget);
			base.CommitButton(this.leftTarget);
			base.CommitButton(this.rightTarget);
			base.CommitButton(this.tapTarget);
		}

		public override void TouchBegan(Touch touch)
		{
			if (this.currentTouch != null)
			{
				return;
			}
			this.beganPosition = TouchManager.ScreenToWorldPoint(touch.position);
			if (this.worldActiveArea.Contains(this.beganPosition))
			{
				this.lastPosition = this.beganPosition;
				this.currentTouch = touch;
				this.currentVector = Vector2.zero;
				this.fireButtonTarget = true;
				this.nextButtonTarget = TouchControl.ButtonTarget.None;
				this.lastButtonTarget = TouchControl.ButtonTarget.None;
			}
		}

		public override void TouchMoved(Touch touch)
		{
			if (this.currentTouch != touch)
			{
				return;
			}
			Vector3 vector = TouchManager.ScreenToWorldPoint(touch.position);
			Vector3 vector2 = vector - this.lastPosition;
			if (vector2.magnitude >= this.sensitivity)
			{
				this.lastPosition = vector;
				this.currentVector = vector2.normalized;
				if (this.fireButtonTarget)
				{
					TouchControl.ButtonTarget buttonTargetForVector = this.GetButtonTargetForVector(this.currentVector);
					if (buttonTargetForVector != this.lastButtonTarget)
					{
						this.nextButtonTarget = buttonTargetForVector;
					}
				}
			}
		}

		public override void TouchEnded(Touch touch)
		{
			if (this.currentTouch != touch)
			{
				return;
			}
			this.currentTouch = null;
			this.currentVector = Vector2.zero;
			Vector3 vector = TouchManager.ScreenToWorldPoint(touch.position);
			if ((this.beganPosition - vector).magnitude < this.sensitivity)
			{
				this.fireButtonTarget = true;
				this.nextButtonTarget = this.tapTarget;
				this.lastButtonTarget = TouchControl.ButtonTarget.None;
				return;
			}
			this.fireButtonTarget = false;
			this.nextButtonTarget = TouchControl.ButtonTarget.None;
			this.lastButtonTarget = TouchControl.ButtonTarget.None;
		}

		private TouchControl.ButtonTarget GetButtonTargetForVector(Vector2 vector)
		{
			Vector2 vector2 = TouchControl.SnapTo(vector, TouchControl.SnapAngles.Four);
			if (vector2 == Vector2.up)
			{
				return this.upTarget;
			}
			if (vector2 == Vector2.right)
			{
				return this.rightTarget;
			}
			if (vector2 == -Vector2.up)
			{
				return this.downTarget;
			}
			if (vector2 == -Vector2.right)
			{
				return this.leftTarget;
			}
			return TouchControl.ButtonTarget.None;
		}

		public Rect ActiveArea
		{
			get
			{
				return this.activeArea;
			}
			set
			{
				if (this.activeArea != value)
				{
					this.activeArea = value;
					this.dirty = true;
				}
			}
		}

		public TouchUnitType AreaUnitType
		{
			get
			{
				return this.areaUnitType;
			}
			set
			{
				if (this.areaUnitType != value)
				{
					this.areaUnitType = value;
					this.dirty = true;
				}
			}
		}

		[Header("Position")]
		[SerializeField]
		private TouchUnitType areaUnitType;

		[SerializeField]
		private Rect activeArea = new Rect(25f, 25f, 50f, 50f);

		[Range(0f, 1f)]
		public float sensitivity = 0.1f;

		[Header("Analog Target")]
		public TouchControl.AnalogTarget target;

		public TouchControl.SnapAngles snapAngles;

		[Header("Button Targets")]
		public TouchControl.ButtonTarget upTarget;

		public TouchControl.ButtonTarget downTarget;

		public TouchControl.ButtonTarget leftTarget;

		public TouchControl.ButtonTarget rightTarget;

		public TouchControl.ButtonTarget tapTarget;

		public bool oneSwipePerTouch;

		private Rect worldActiveArea;

		private Vector3 currentVector;

		private Vector3 beganPosition;

		private Vector3 lastPosition;

		private Touch currentTouch;

		private bool fireButtonTarget;

		private TouchControl.ButtonTarget nextButtonTarget;

		private TouchControl.ButtonTarget lastButtonTarget;

		private bool dirty;
	}
}
