using System;
using ObjectModel;
using UnityEngine;

namespace Boats
{
	public class StaminaController
	{
		public StaminaController(StaminaSettings settings, float fastSpeedK)
		{
			this._settings = settings;
			this._fastSpeedK = fastSpeedK;
		}

		public float Value
		{
			get
			{
				return this._stamina;
			}
		}

		private bool IsFastRowing
		{
			get
			{
				return ControlsController.ControlsActions.RunHotkey.IsPressed;
			}
		}

		public float RowingK
		{
			get
			{
				return this._currentSpeedK;
			}
		}

		private float TargetSpeedK
		{
			get
			{
				if (this.IsFastRowing)
				{
					if (this._stamina > this._settings.FatigueFrom)
					{
						return this._fastSpeedK;
					}
					if (this._stamina > this._settings.MinSpeedFrom)
					{
						float num = (this._stamina - this._settings.MinSpeedFrom) / (this._settings.FatigueFrom - this._settings.MinSpeedFrom);
						return num * this._fastSpeedK + (1f - num);
					}
				}
				return 1f;
			}
		}

		public void Update(bool notRowing, bool notRowingLongEnough)
		{
			if (GameFactory.Player != null && GameFactory.Player.IsSailing)
			{
				if (ControlsController.ControlsActions.RunHotkey.WasPressed)
				{
					if (!StaticUserData.IS_IN_TUTORIAL)
					{
						PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.Sprint, true, null);
					}
				}
				else if (ControlsController.ControlsActions.RunHotkey.WasReleased && !StaticUserData.IS_IN_TUTORIAL)
				{
					PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.Sprint, false, null);
				}
			}
			if (notRowing)
			{
				if (notRowingLongEnough)
				{
					this._stamina = Mathf.Clamp01(this._stamina + this._settings.IdleStaminaSpeed * Time.deltaTime);
				}
				else
				{
					this._stamina = Mathf.Clamp01(this._stamina - this._settings.NormalRowingFatigueSpeed * 0.5f * Time.deltaTime);
				}
			}
			else
			{
				bool isPressed = ControlsController.ControlsActions.RunHotkey.IsPressed;
				float num = ((!isPressed) ? this._settings.NormalRowingFatigueSpeed : this._settings.FastRowingFatigueSpeed);
				this._stamina = Mathf.Clamp01(this._stamina - num * Time.deltaTime);
				if (isPressed)
				{
					this._currentSpeedK = Mathf.Min(this._currentSpeedK + this._speedAcceleration * Time.deltaTime, this.TargetSpeedK);
				}
				else
				{
					this._currentSpeedK = Mathf.Max(this._currentSpeedK - this._speedAcceleration * Time.deltaTime, this.TargetSpeedK);
				}
			}
		}

		public void Restore()
		{
			this._stamina = 1f;
		}

		private readonly StaminaSettings _settings;

		private readonly float _fastSpeedK;

		private float _stamina = 1f;

		private float _speedAcceleration = 1f;

		private float _currentSpeedK = 1f;
	}
}
