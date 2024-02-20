using System;
using Assets.Scripts.UI._2D.PlayerProfile;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ActiveTournamentDetails : BaseTournamentDetails
{
	protected override void Awake()
	{
		base.Awake();
		this.scrollBar.value = 1f;
		this._nicknameText.text = PlayerProfileHelper.UsernameCaption;
	}

	public override void Init(Tournament tournament)
	{
		base.Init(tournament);
		this.scrollBar.value = 1f;
	}

	[SerializeField]
	private Text _nicknameText;

	public Scrollbar scrollBar;
}
