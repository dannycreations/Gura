using System;
using System.Diagnostics;
using I2.Loc;
using InControl;
using UnityEngine.UI;

public class TournamentDetailsMessage : BaseTournamentDetails
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<MessageBoxEventArgs> CloseOnClick;

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(false);
		MessageFactory.MessageBoxQueue.Enqueue(this);
	}

	private void OnEnable()
	{
		this.ShowHints();
	}

	public void CloseClick()
	{
		this.Close();
		if (this.CloseOnClick != null)
		{
			this.CloseOnClick(this, new MessageBoxEventArgs(base.gameObject));
		}
	}

	private void ShowHints()
	{
		for (int i = 0; i < this.hints.Length; i++)
		{
			this.hints[i].text = string.Empty;
			this.hints[i].gameObject.SetActive(false);
		}
		this.hints[0].gameObject.SetActive(true);
		this.hints[1].gameObject.SetActive(true);
		this.hints[0].text = HotkeyIcons.KeyMappings[InputControlType.DPadLeft] + " " + ScriptLocalization.Get("ChangeCompetitionTab");
		this.hints[1].text = HotkeyIcons.KeyMappings[InputControlType.LeftStickDown] + " " + ScriptLocalization.Get("ScrollList");
	}

	public Text[] hints;
}
