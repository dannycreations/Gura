using System;
using UnityEngine;
using UnityEngine.UI;

public class TournamentResultInit : MonoBehaviour
{
	internal void Init(TournamentFinalResult result)
	{
		this.TournamentName.text = result.TournamentName;
		this.TournamentResultInfo.text = string.Format("Your place: {0} \n Your score 1: {1} \n Your Rank: {2}", result.CurrentPlayerResult.Place, result.CurrentPlayerResult.Score, result.CurrentPlayerResult.Rank);
	}

	public void CloseWindow()
	{
		base.GetComponent<AlphaFade>().HideFinished += this.TournamentResultInit_HideFinished;
		base.GetComponent<AlphaFade>().HidePanel();
	}

	private void TournamentResultInit_HideFinished(object sender, EventArgsAlphaFade e)
	{
		Object.Destroy(this);
	}

	public Text TournamentName;

	public Text TournamentResultInfo;

	public Image LogoImage;
}
