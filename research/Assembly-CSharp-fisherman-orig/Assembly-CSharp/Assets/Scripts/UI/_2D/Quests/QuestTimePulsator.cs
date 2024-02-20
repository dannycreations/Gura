using System;
using System.Collections;
using Assets.Scripts.UI._2D.Common;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI._2D.Quests
{
	public class QuestTimePulsator : IUpdateable, IDisposable
	{
		public QuestTimePulsator(MissionOnClient currentQuest, Text timerValue, QuestTimePulsator.PulsationData o)
		{
			this._currentQuest = currentQuest;
			this._timerValue = timerValue;
			this._pulsationData = o;
		}

		public QuestTimePulsator(MissionOnClient currentQuest, TextMeshProUGUI timerValue, QuestTimePulsator.PulsationData o)
		{
			this._currentQuest = currentQuest;
			this._timerValueTmp = timerValue;
			this._pulsationData = o;
		}

		public QuestTimePulsator(MissionOnClient currentQuest, Text timerValue)
			: this(currentQuest, timerValue, new QuestTimePulsator.PulsationData(Color.red, Color.white, 0f, 1f, 10f, true))
		{
		}

		public QuestTimePulsator(MissionOnClient currentQuest, TextMeshProUGUI timerValue)
			: this(currentQuest, timerValue, new QuestTimePulsator.PulsationData(Color.red, Color.white, 0f, 1f, 10f, true))
		{
		}

		public void Reset()
		{
			this.EnableEndWarning(this.GoWithColorPulsation, false);
			this.SetText(string.Empty);
			this.SetColor(this._pulsationData.Color0);
			this._previousTimeText = string.Empty;
		}

		public void Update()
		{
			this.UpdateTimeToComplete();
		}

		public void Dispose()
		{
			this.Reset();
			GameObject goWithColorPulsation = this.GoWithColorPulsation;
			if (goWithColorPulsation != null)
			{
				TextColorPulsation component = goWithColorPulsation.GetComponent<TextColorPulsation>();
				if (component != null)
				{
					component.StopAllCoroutines();
				}
			}
		}

		private void UpdateTimeToComplete()
		{
			if (this._currentQuest != null && this._currentQuest.TimeToComplete != null)
			{
				DateTime dateTime = this._currentQuest.StartTime.AddSeconds((double)this._currentQuest.TimeToComplete.Value);
				TimeSpan timeSpan = dateTime - TimeHelper.UtcTime();
				this.SetText(string.Format("{0} {1}", "\ue70c", timeSpan.GetFormatedMinSec()));
				if (timeSpan.TotalSeconds <= 0.0)
				{
					this.SetText("\ue70c 00:00");
					this.EnableEndWarning(this.GoWithColorPulsation, false);
					this.SetColor(this._pulsationData.StartColor);
				}
				else if (timeSpan.TotalSeconds <= (double)this._pulsationData.WarningSeconds)
				{
					this.EnableEndWarning(this.GoWithColorPulsation, true);
				}
			}
		}

		private IEnumerator CountdownTick()
		{
			while (this._previousTimeText != "\ue70c 00:00")
			{
				if (this._previousTimeText != this.Text)
				{
					if (this._pulsationData.IsPlaySound)
					{
						UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.CoundownClip, SettingsManager.InterfaceVolume);
					}
					this._previousTimeText = this.Text;
				}
				yield return new WaitForSeconds(0.1f);
			}
			LogHelper.Log("___kocha TimeEnded -> ForceProcessMissions MissionId:{0}", new object[] { this._currentQuest.MissionId });
			yield return new WaitForSeconds(0.5f);
			PhotonConnectionFactory.Instance.ForceProcessMissions();
			yield break;
			yield break;
		}

		private void EnableEndWarning(GameObject enableWarnOn, bool enable)
		{
			TextColorPulsation textColorPulsation = enableWarnOn.GetComponent<TextColorPulsation>();
			if (textColorPulsation == null)
			{
				textColorPulsation = enableWarnOn.AddComponent<TextColorPulsation>();
				textColorPulsation.enabled = false;
			}
			if (enable && textColorPulsation.enabled)
			{
				return;
			}
			textColorPulsation.enabled = enable;
			textColorPulsation.StartColor = this._pulsationData.StartColor;
			textColorPulsation.MinAlpha = this._pulsationData.MinAlpha;
			textColorPulsation.PulseTime = this._pulsationData.PulseTime;
			if (enable)
			{
				textColorPulsation.StartCoroutine(this.CountdownTick());
			}
		}

		private void SetColor(Color v)
		{
			if (this._timerValue != null)
			{
				this._timerValue.color = v;
			}
			else if (this._timerValueTmp != null)
			{
				this._timerValueTmp.color = v;
			}
		}

		private void SetText(string v)
		{
			if (this._timerValue != null)
			{
				this._timerValue.text = v;
			}
			else if (this._timerValueTmp != null)
			{
				this._timerValueTmp.text = v;
			}
		}

		private string Text
		{
			get
			{
				if (this._timerValue != null)
				{
					return this._timerValue.text;
				}
				if (this._timerValueTmp != null)
				{
					return this._timerValueTmp.text;
				}
				return null;
			}
		}

		private GameObject GoWithColorPulsation
		{
			get
			{
				if (this._timerValue != null)
				{
					return this._timerValue.gameObject;
				}
				if (this._timerValueTmp != null)
				{
					return this._timerValueTmp.gameObject;
				}
				return null;
			}
		}

		public const string TimeIco = "\ue70c";

		public const string TimeEnded = "\ue70c 00:00";

		private readonly MissionOnClient _currentQuest;

		private readonly Text _timerValue;

		private readonly TextMeshProUGUI _timerValueTmp;

		private readonly QuestTimePulsator.PulsationData _pulsationData;

		private string _previousTimeText;

		public class PulsationData
		{
			public PulsationData(Color startColor, Color color0, float minAlpha, float pulseTime, float warningSeconds, bool isPlaySound)
			{
				this.StartColor = startColor;
				this.Color0 = color0;
				this.MinAlpha = minAlpha;
				this.PulseTime = pulseTime;
				this.WarningSeconds = warningSeconds;
				this.IsPlaySound = isPlaySound;
			}

			public Color StartColor { get; private set; }

			public Color Color0 { get; private set; }

			public float MinAlpha { get; private set; }

			public float PulseTime { get; private set; }

			public float WarningSeconds { get; private set; }

			public bool IsPlaySound { get; private set; }
		}
	}
}
