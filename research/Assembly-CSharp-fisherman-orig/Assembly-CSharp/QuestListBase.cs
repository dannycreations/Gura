using System;
using UnityEngine;
using UnityEngine.UI;

public class QuestListBase : MonoBehaviour
{
	public QuestListBase.State CurrentState { get; protected set; }

	public void UpdateState(QuestListBase.State state = QuestListBase.State.None)
	{
		this.CurrentState = state;
		this._groupImage.text = ((state != QuestListBase.State.Active) ? ((state != QuestListBase.State.New) ? string.Empty : "\ue781") : "\ue780");
		Color colorByState = this.GetColorByState(state);
		this._nameText.color = colorByState;
		this._groupImage.color = colorByState;
	}

	public int SiblingIndex
	{
		get
		{
			return base.gameObject.transform.GetSiblingIndex();
		}
	}

	protected Color GetColorByState(QuestListBase.State state)
	{
		Color color;
		ColorUtility.TryParseHtmlString((state != QuestListBase.State.Active) ? "#F7F7F7FF" : "#FFDD77FF", ref color);
		return color;
	}

	[SerializeField]
	protected Text _nameText;

	[SerializeField]
	protected Text _groupImage;

	[SerializeField]
	protected Font _noneFont;

	[SerializeField]
	protected Font _highlightFont;

	protected const string ActiveIco = "\ue780";

	protected const string NewIco = "\ue781";

	public enum State : byte
	{
		None,
		Active,
		New
	}
}
