using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Quests;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class QuestListItemHandler : QuestListBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<QuestEventArgs> IsSelectedItem;

	public MissionOnClient Mission
	{
		get
		{
			return this._currentQuest;
		}
	}

	protected virtual void Update()
	{
		if (this._timePulsator != null)
		{
			this._timePulsator.Update();
		}
	}

	protected virtual void OnDestroy()
	{
		this.ClearPulsator();
	}

	public void ClearPulsator()
	{
		if (this._timePulsator != null)
		{
			this._timePulsator.Dispose();
			this._timePulsator = null;
		}
	}

	public void Init(MissionOnClient quest)
	{
		this._currentQuest = quest;
		this._nameText.text = this._currentQuest.Name;
		this.QuestImageLdbl.Image = this.QuestImage;
		if (quest.ThumbnailBID != null)
		{
			this.QuestImageLdbl.Load(string.Format("Textures/Inventory/{0}", quest.ThumbnailBID.Value));
		}
		base.UpdateState(QuestListBase.State.None);
		this.ClearPulsator();
		if (this._currentQuest.TimeToComplete != null && !quest.IsCompleted)
		{
			this._timePulsator = new QuestTimePulsator(this._currentQuest, this.Status, new QuestTimePulsator.PulsationData(Color.red, Color.white, 0f, 1f, 10f, false));
		}
	}

	public void UpdateMission(MissionOnClient m)
	{
		this._currentQuest = m;
	}

	public void IsSelected()
	{
		if (this.Toggle.isOn && this.IsSelectedItem != null)
		{
			this.IsSelectedItem(this, new QuestEventArgs
			{
				Quest = this._currentQuest
			});
		}
	}

	public void ToggleTurnOff()
	{
		if (this.Toggle.isOn)
		{
			this.Toggle.isOn = false;
		}
	}

	public Image QuestImage;

	private ResourcesHelpers.AsyncLoadableImage QuestImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Toggle Toggle;

	public Text Status;

	private MissionOnClient _currentQuest;

	private QuestTimePulsator _timePulsator;
}
