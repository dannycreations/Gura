using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class GroupedList : QuestListBase
{
	public List<int> Missions
	{
		get
		{
			return this._missionsIds;
		}
	}

	public List<QuestListItemHandler> MissionItems
	{
		get
		{
			return this._questList;
		}
	}

	private void Awake()
	{
		ColorUtility.TryParseHtmlString("#4B4C4FFF", ref this._normalColor);
		ColorUtility.TryParseHtmlString("#4B4C4F70", ref this._selectedColor);
	}

	private void Start()
	{
		base.GetComponent<RectTransform>().anchoredPosition = new Vector2(base.GetComponent<RectTransform>().anchoredPosition.x, base.GetComponent<RectTransform>().anchoredPosition.y + 5f);
		this.SetTimeRemain(this._timeRemain);
		this.GroupToggle.onValueChanged.AddListener(delegate(bool isActive)
		{
			this.UpdateBgColor();
		});
	}

	private void OnDestroy()
	{
		this.GroupToggle.onValueChanged.RemoveAllListeners();
	}

	public void REinitListeners(Action<bool> action)
	{
		this.GroupToggle.onValueChanged.RemoveAllListeners();
		this.GroupToggle.onValueChanged.AddListener(delegate(bool isActive)
		{
			this.UpdateBgColor();
			action(isActive);
		});
	}

	private void Update()
	{
		if (this._timeRemain.TotalSeconds > 0.0)
		{
			this._timeRemain = TimeSpan.FromSeconds(this._timeRemain.TotalSeconds - (double)Time.deltaTime);
			this._timeRemainText.text = this._timeRemain.GetFormated(true, true);
		}
		else if (this._timeRemainText.gameObject.activeSelf)
		{
			this.SetTimeRemain(this._timeRemain);
		}
	}

	public new void UpdateState(QuestListBase.State state = QuestListBase.State.None)
	{
		base.UpdateState(state);
		Graphic timeRemainText = this._timeRemainText;
		Color color = this._nameText.color;
		this._timeRemainIcoText.color = color;
		timeRemainText.color = color;
	}

	public void SetTimeRemain(TimeSpan timeRemain)
	{
		this._timeRemain = timeRemain;
		this._timeRemainText.gameObject.SetActive(this._timeRemain.TotalSeconds > 0.0);
		this._timeRemainIcoText.gameObject.SetActive(this._timeRemain.TotalSeconds > 0.0);
	}

	public bool UpdateMission(MissionOnClient m)
	{
		QuestListItemHandler questListItemHandler = this._questList.FirstOrDefault((QuestListItemHandler p) => p.Mission.MissionId == m.MissionId);
		if (questListItemHandler != null)
		{
			questListItemHandler.UpdateMission(m);
		}
		return questListItemHandler != null;
	}

	public MissionOnClient Find(int missionId)
	{
		QuestListItemHandler questListItemHandler = this._questList.FirstOrDefault((QuestListItemHandler p) => p.Mission.MissionId == missionId);
		return (!(questListItemHandler != null)) ? null : questListItemHandler.Mission;
	}

	public void UpdateStateElement(int missionId, QuestListBase.State state = QuestListBase.State.None)
	{
		QuestListItemHandler questListItemHandler = this._questList.FirstOrDefault((QuestListItemHandler p) => p.Mission.MissionId == missionId);
		if (questListItemHandler != null)
		{
			questListItemHandler.UpdateState(state);
		}
	}

	public void Init(string groupName, Transform clearToTransform, QuestListBase.State state = QuestListBase.State.None)
	{
		this.UpdateState(state);
		this._nameText.text = groupName;
		this.Clear(clearToTransform);
		this.GroupToggle.isOn = true;
		this.UpdateBgColor();
	}

	public void AddElement(QuestListItemHandler element, int index)
	{
		this._questList.Add(element);
		this._missionsIds.Add(element.Mission.MissionId);
		this.currentHeight += element.GetComponent<LayoutElement>().preferredHeight;
		element.transform.SetParent(this.Content, false);
		element.transform.SetSiblingIndex(index);
		this.Layout.preferredHeight = 66f + this.currentHeight;
	}

	public void Clear(Transform clearToTransform)
	{
		this.currentHeight = 0f;
		this._questList.Clear();
		this._missionsIds.Clear();
		this.GroupToggle.onValueChanged.RemoveAllListeners();
		for (int i = 0; i < this.Content.transform.childCount; i++)
		{
			this.Content.transform.GetChild(i).SetParent(clearToTransform);
		}
	}

	public void OnRollup()
	{
		this.Content.gameObject.SetActive(this.GroupToggle.isOn);
		LayoutRebuilder.ForceRebuildLayoutImmediate(this.Content as RectTransform);
		this.Layout.preferredHeight = 66f + ((!this.GroupToggle.isOn) ? 0f : this.Content.GetComponent<RectTransform>().rect.height);
	}

	public void TurnOffAllToggles()
	{
		this._questList.ForEach(delegate(QuestListItemHandler p)
		{
			p.ToggleTurnOff();
		});
	}

	private void UpdateBgColor()
	{
		this._bgImage.color = ((!this.GroupToggle.isOn) ? this._normalColor : this._selectedColor);
	}

	[SerializeField]
	private Image _bgImage;

	[SerializeField]
	protected Text _timeRemainText;

	[SerializeField]
	protected Text _timeRemainIcoText;

	private List<int> _missionsIds = new List<int>();

	public LayoutElement Layout;

	public Transform Content;

	public Toggle GroupToggle;

	private float currentHeight;

	private const float HeaderHight = 66f;

	private List<QuestListItemHandler> _questList = new List<QuestListItemHandler>();

	private Color _normalColor;

	private Color _selectedColor;

	private TimeSpan _timeRemain = TimeSpan.Zero;
}
