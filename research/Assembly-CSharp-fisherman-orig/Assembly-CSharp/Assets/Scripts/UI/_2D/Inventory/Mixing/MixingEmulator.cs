using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Assets.Scripts.UI._2D.Common;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI._2D.Inventory.Mixing
{
	public class MixingEmulator : IUpdateable, IDisposable
	{
		public MixingEmulator(Text progressPrc, Image progress, int duration, BorderedButton buttonCancel, bool isPlaySound = true)
		{
			this._isPlaySound = isPlaySound;
			this._isTimer = true;
			this._buttonCancel = buttonCancel;
			this._finishTime = (float)duration;
			this._progressPrc = progressPrc;
			this._progress = progress;
			this.UpdateState(MixingEmulator.States.Start, true);
			this._startTime = DateTime.UtcNow;
		}

		public MixingEmulator(Chum chum, Text progressPrc, Image progress, BorderedButton buttonOk, BorderedButton buttonCancel, TextMeshProUGUI noPremText, TextMeshProUGUI premText)
		{
			this._progressPrc = progressPrc;
			this._progress = progress;
			this._buttonOk = buttonOk;
			this._buttonCancel = buttonCancel;
			this._noPremText = noPremText;
			this._premText = premText;
			this._chum = chum;
			this._mixTime = new Dictionary<ItemSubTypes, float[]>
			{
				{
					ItemSubTypes.ChumMethodMix,
					new float[]
					{
						Inventory.ChumMethodMixMixTime,
						Inventory.ChumMethodMixMixTimePremium
					}
				},
				{
					ItemSubTypes.ChumCarpbaits,
					new float[]
					{
						Inventory.ChumCarpbaitsMixTime,
						Inventory.ChumCarpbaitsMixTimePremium
					}
				},
				{
					ItemSubTypes.ChumGroundbaits,
					new float[]
					{
						Inventory.ChumGroundbaitsMixTime,
						Inventory.ChumGroundbaitsMixTimePremium
					}
				}
			};
			this.InitMixTime();
			if (PhotonConnectionFactory.Instance.Profile.HasPremium)
			{
				noPremText.color = new Color(noPremText.color.r, noPremText.color.g, noPremText.color.b, 0.4f);
			}
			else
			{
				premText.color = new Color(premText.color.r, premText.color.g, premText.color.b, 0.4f);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ChangeState = delegate
		{
		};

		public void Update()
		{
			if (this.InState(MixingEmulator.States.Start) || this.InState(MixingEmulator.States.Process))
			{
				this._time = (float)DateTime.UtcNow.Subtract(this._startTime).TotalSeconds;
				float num = ((!this._isTimer) ? (this._time / this._finishTime) : ((this._timerValue > 0) ? (1f - (float)this._timerValue / this._finishTime) : 0f));
				if (this._isTimer && this._timerValue <= 0)
				{
					num = 1f;
				}
				this._progress.fillAmount = Mathf.Min(num, 1f);
				if (this._isTimer)
				{
					int num2 = (int)(this._finishTime - this._time);
					if (num2 != this._timerValue)
					{
						this._timerValue = num2;
						this._progressPrc.text = this._timerValue.ToString();
						if (this._isPlaySound)
						{
							UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.CoundownClip, SettingsManager.InterfaceVolume);
						}
					}
				}
				else
				{
					this._progressPrc.text = string.Format("{0}%", (int)(num * 100f));
				}
				if (this._time >= this._blockTime && !this.InState(MixingEmulator.States.Process))
				{
					if (this._chum != null)
					{
						this.MixChum();
					}
					this.UpdateState(MixingEmulator.States.Process, false);
				}
				else if (num >= 1f)
				{
					this._progress.color = this._finishColor;
					this.UpdateState(MixingEmulator.States.Ready, false);
				}
			}
			else if (this.InState(MixingEmulator.States.Ready))
			{
				this._time = (float)DateTime.UtcNow.Subtract(this._startTime).TotalSeconds;
				if (this._time >= this._finishTime + 0.5f)
				{
					if (this._transferItemFunc != null && this._chumResult != null)
					{
						this._transferItemFunc(this._chumResult, null);
					}
					this.UpdateState(MixingEmulator.States.Finish, true);
				}
			}
		}

		public void Init(Func<InventoryItem, InventoryItem, bool> transferItemFunc)
		{
			this._transferItemFunc = transferItemFunc;
			this.UpdateState(MixingEmulator.States.Start, true);
			this._startTime = DateTime.UtcNow;
		}

		public bool InState(MixingEmulator.States s)
		{
			return this._state == s;
		}

		public void Dispose()
		{
			this._transferItemFunc = null;
			this.Unsubscribe();
		}

		public void FullStop()
		{
			this._state = MixingEmulator.States.FullStop;
		}

		private void InitMixTime()
		{
			ItemSubTypes itemSubTypes = InventoryHelper.GetChumType(this._chum);
			if (!this._mixTime.ContainsKey(itemSubTypes))
			{
				Debug.LogErrorFormat("MixingEmulator:InitMixTime - cvan't found chumType:{0}", new object[] { itemSubTypes });
				itemSubTypes = ItemSubTypes.ChumGroundbaits;
			}
			float num = this._mixTime[itemSubTypes][0];
			float num2 = this._mixTime[itemSubTypes][1];
			this._finishTime = ((!PhotonConnectionFactory.Instance.Profile.HasPremium) ? num : num2);
			this._blockTime = 1.5f;
			string text = MeasuringSystemManager.Seconds();
			this._noPremText.text = string.Format("{0}: {1}{2}", ScriptLocalization.Get("StandartAccountMessage"), num.ToString(CultureInfo.InvariantCulture), text);
			this._premText.text = string.Format("{0}: {1}{2}", ScriptLocalization.Get("PremiumAccountCaption"), num2.ToString(CultureInfo.InvariantCulture), text);
		}

		private void UpdateState(MixingEmulator.States newState, bool resetTimer = true)
		{
			if (resetTimer)
			{
				this._time = 0f;
			}
			this._state = newState;
			if (this._buttonOk != null)
			{
				this._buttonOk.interactable = this.InState(MixingEmulator.States.Finish);
			}
			if (this._buttonCancel != null)
			{
				this._buttonCancel.interactable = this.InState(MixingEmulator.States.None) || this.InState(MixingEmulator.States.Start) || (this.InState(MixingEmulator.States.Process) && this._isTimer);
			}
			this.ChangeState();
		}

		private void MixChum()
		{
			if (this.InState(MixingEmulator.States.FullStop))
			{
				return;
			}
			PhotonConnectionFactory.Instance.OnChumMixFailure += this.Instance_OnChumMixFailure;
			PhotonConnectionFactory.Instance.OnChumMix += this.Instance_OnChumMix;
			PhotonConnectionFactory.Instance.MixChum(this._chum);
		}

		private void Instance_OnChumMixFailure(Failure failure)
		{
			this.Unsubscribe();
			Debug.LogErrorFormat("OnChumMixFailure - ErrorCode:{0} ErrorMessage:{1}", new object[] { failure.ErrorCode, failure.ErrorMessage });
		}

		private void Instance_OnChumMix(Chum chum)
		{
			this.Unsubscribe();
			this._chumResult = chum;
		}

		private void Unsubscribe()
		{
			PhotonConnectionFactory.Instance.OnChumMixFailure -= this.Instance_OnChumMixFailure;
			PhotonConnectionFactory.Instance.OnChumMix -= this.Instance_OnChumMix;
		}

		private Text _progressPrc;

		private Image _progress;

		private BorderedButton _buttonOk;

		private BorderedButton _buttonCancel;

		private TextMeshProUGUI _noPremText;

		private TextMeshProUGUI _premText;

		private readonly Color _finishColor = new Color(0.49411765f, 0.827451f, 0.12941177f);

		private float _finishTime;

		private float _blockTime;

		private MixingEmulator.States _state;

		private float _time;

		private DateTime _startTime;

		private Func<InventoryItem, InventoryItem, bool> _transferItemFunc;

		private Chum _chum;

		private Chum _chumResult;

		private const float BlockTime = 1.5f;

		private const float AccountAlpha = 0.4f;

		private readonly Dictionary<ItemSubTypes, float[]> _mixTime;

		private bool _isTimer;

		private int _timerValue;

		private bool _isPlaySound;

		public enum States : byte
		{
			None,
			Start,
			Process,
			Ready,
			Finish,
			FullStop
		}
	}
}
