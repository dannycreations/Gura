using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop
{
	public class PremiumShopMouseOverMovement
	{
		public PremiumShopMouseOverMovement(RectTransform mouseOverAndMovement, List<RectTransform> rtItems, Camera cam, Func<bool> isMouse, Func<bool> isHome, Func<bool> isLastInVisibleRect, float xBig0, float diff0, float diff, RectTransform mouseOverAndMovementLeft)
		{
			this._mouseOverAndMovementLeft = mouseOverAndMovementLeft;
			this._mouseOverAndMovement = mouseOverAndMovement;
			this._rtItems = rtItems;
			this._camera = cam;
			this._isMouse = isMouse;
			this._isHome = isHome;
			this._isLastInVisibleRect = isLastInVisibleRect;
			this._xBig0 = xBig0;
			this._diff0 = diff0;
			this._diff = diff;
			this.CachePositions();
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnStart = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int, float> OnMoveAllItems = delegate(int index, float diff)
		{
		};

		public void Update(float mouseOverAndMovementTimeForStartAnim, float mouseOverAndMovementAnimSpeed)
		{
			if (this._mouseOverAndMovementLeft != null && this._isMouse() && !this._isHome() && (this._movementType == PremiumShopMouseOverMovement.MovementTypes.Left || !this._isMenuVisible))
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(this._mouseOverAndMovementLeft, Input.mousePosition) && !this._isPausedLeft && !this._isPausedRight)
				{
					this._movementIdxLast = null;
					this._mouseOverAndMovementTime += Time.deltaTime;
					if (this._mouseOverAndMovementTime >= mouseOverAndMovementTimeForStartAnim)
					{
						this._movementType = PremiumShopMouseOverMovement.MovementTypes.Left;
						if (this._rtItems[0].anchoredPosition.x >= this._xBig0)
						{
							this._mouseOverAndMovementTime = 0f;
							this.Stop();
							return;
						}
						if (this._mousePositionXLeft == null)
						{
							this._mousePositionXLeft = new float?(Input.mousePosition.x);
						}
						float num = Mathf.Max(0f, Input.mousePosition.x);
						float speed2 = (mouseOverAndMovementAnimSpeed - mouseOverAndMovementAnimSpeed * 2f) * (num / this._mousePositionXLeft.Value) + mouseOverAndMovementAnimSpeed * 2f;
						this._rtItems.ForEach(delegate(RectTransform rt)
						{
							rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + speed2, rt.anchoredPosition.y);
						});
					}
				}
				else if (this._mouseOverAndMovementTime <= 0f || this._movementType == PremiumShopMouseOverMovement.MovementTypes.Left)
				{
				}
			}
			else if (this._mouseOverAndMovementTime > 0f && this._movementType == PremiumShopMouseOverMovement.MovementTypes.Left)
			{
				this.Stop();
			}
			if (this._mouseOverAndMovement != null && this._isMouse() && !this._isHome() && (this._movementType == PremiumShopMouseOverMovement.MovementTypes.Right || !this._isLastInVisibleRect()))
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(this._mouseOverAndMovement, Input.mousePosition) && !this._isPausedLeft && !this._isPausedRight)
				{
					this._mouseOverAndMovementTime += Time.deltaTime;
					if (this._mouseOverAndMovementTime >= mouseOverAndMovementTimeForStartAnim)
					{
						if (this._isMenuVisible)
						{
							this.OnStart();
						}
						this._movementType = PremiumShopMouseOverMovement.MovementTypes.Right;
						if (this._isLastInVisibleRect())
						{
							if (this._movementIdxLast == null)
							{
								int num2;
								float num3;
								this.GetMovementIdx(out num2, out num3, false);
								if (num2 > -1)
								{
									this._movementIdxLast = new int?(num2);
								}
							}
							if (this._movementIdxLast != null && this._rtItems[0].anchoredPosition.x <= this._movementPos[this._movementIdxLast.Value])
							{
								return;
							}
						}
						if (this._mousePositionXRight == null)
						{
							this._mousePositionXRight = new float?(Input.mousePosition.x);
						}
						float num4 = Mathf.Min((float)Screen.width, Input.mousePosition.x);
						float num5 = (float)Screen.width - this._mousePositionXRight.Value;
						float num6 = (float)Screen.width - num4;
						float num7 = 100f - 100f * num6 / num5;
						float num8 = mouseOverAndMovementAnimSpeed * 2f - mouseOverAndMovementAnimSpeed;
						float speed = num8 * num7 / 100f + mouseOverAndMovementAnimSpeed;
						this._rtItems.ForEach(delegate(RectTransform rt)
						{
							rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - speed, rt.anchoredPosition.y);
						});
					}
				}
				else if (this._mouseOverAndMovementTime <= 0f || this._movementType == PremiumShopMouseOverMovement.MovementTypes.Right)
				{
				}
			}
			else if (this._mouseOverAndMovementTime > 0f && this._movementType == PremiumShopMouseOverMovement.MovementTypes.Right)
			{
				this.Stop();
			}
		}

		public void Stop()
		{
			if (this._movementType != PremiumShopMouseOverMovement.MovementTypes.None)
			{
				bool flag = this._movementType == PremiumShopMouseOverMovement.MovementTypes.Left;
				this._movementType = PremiumShopMouseOverMovement.MovementTypes.None;
				float num;
				int value;
				if (this._movementIdxLast != null)
				{
					num = Math.Abs(Math.Abs(this._movementPos[this._movementIdxLast.Value]) - Math.Abs(this._rtItems[0].anchoredPosition.x));
					value = this._movementIdxLast.Value;
					this._movementIdxLast = null;
				}
				else
				{
					this.GetMovementIdx(out value, out num, flag);
				}
				this.OnMoveAllItems(value, -num);
			}
		}

		public void SetMenuVisible(bool flag)
		{
			this.ForceStop();
			this._isMenuVisible = flag;
		}

		public bool IsMenuVisible
		{
			get
			{
				return this._isMenuVisible;
			}
		}

		public void ForceStop()
		{
			this._movementType = PremiumShopMouseOverMovement.MovementTypes.None;
			this._mouseOverAndMovementTime = 0f;
			this._movementIdxLast = null;
			this._isMenuVisible = true;
		}

		public void SetPause(bool v, bool isLeft)
		{
			if (isLeft)
			{
				this._isPausedLeft = v;
			}
			else
			{
				this._isPausedRight = v;
			}
		}

		private void GetMovementIdx(out int i, out float diff, bool isLeft)
		{
			i = -1;
			diff = 0f;
			float x = this._rtItems[0].anchoredPosition.x;
			if (isLeft)
			{
				if (x >= this._xBig0)
				{
					diff = Math.Abs(Math.Abs(this._xBig0) - Math.Abs(x));
					diff *= -1f;
					i = 0;
				}
				else
				{
					foreach (KeyValuePair<int, float> keyValuePair in this._movementPos.Reverse<KeyValuePair<int, float>>())
					{
						if (keyValuePair.Value > x)
						{
							diff = Math.Abs(Math.Abs(keyValuePair.Value) - Math.Abs(x));
							diff *= -1f;
							i = keyValuePair.Key;
							break;
						}
					}
				}
			}
			else
			{
				foreach (KeyValuePair<int, float> keyValuePair2 in this._movementPos)
				{
					if (keyValuePair2.Value < x)
					{
						diff = Math.Abs(Math.Abs(keyValuePair2.Value) - Math.Abs(x));
						i = keyValuePair2.Key;
						break;
					}
				}
			}
		}

		private void CachePositions()
		{
			if (this._movementPos.Count == 0)
			{
				for (int i = 0; i < this._rtItems.Count; i++)
				{
					if (i == 0)
					{
						this._movementPos[i] = this._xBig0;
					}
					else if (i != 1)
					{
						if (i == 2)
						{
							this._movementPos[i] = this._movementPos[0] - this._diff0 - this._diff;
						}
						else
						{
							this._movementPos[i] = this._movementPos[i - 1] - this._diff;
						}
					}
				}
			}
		}

		public const bool IsMovementEnabled = true;

		private const float MouseOverAndMovementAnimSpeedKoef = 2f;

		private readonly Dictionary<int, float> _movementPos = new Dictionary<int, float>();

		private int? _movementIdxLast;

		private bool _isMenuVisible = true;

		private PremiumShopMouseOverMovement.MovementTypes _movementType;

		private float _mouseOverAndMovementTime;

		private readonly RectTransform _mouseOverAndMovement;

		private readonly RectTransform _mouseOverAndMovementLeft;

		private readonly Camera _camera;

		private readonly List<RectTransform> _rtItems;

		private readonly Func<bool> _isMouse;

		private readonly Func<bool> _isHome;

		private readonly Func<bool> _isLastInVisibleRect;

		private readonly float _xBig0;

		private readonly float _diff0;

		private readonly float _diff;

		private bool _isPausedLeft;

		private bool _isPausedRight;

		private float? _mousePositionXLeft;

		private float? _mousePositionXRight;

		private enum MovementTypes : byte
		{
			None,
			Left,
			Right
		}
	}
}
