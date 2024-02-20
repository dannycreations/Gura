using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TournamentDetailsPrecondition : MonoBehaviour
{
	public void Init(List<TournamentPrecondition> preconditions, TournamentPrecondition tp)
	{
		Text value = this._value;
		int num = 30;
		this._value.resizeTextMaxSize = num;
		value.fontSize = num;
		this._value.text = TournamentDetailsPrecondition.PreconditionConstructor(tp);
		if (preconditions != null && preconditions.Any((TournamentPrecondition x) => x.PreconditionType == tp.PreconditionType))
		{
			this._inCorrect.SetActive(true);
		}
		else
		{
			this._correct.SetActive(true);
		}
	}

	public static string PreconditionConstructor(TournamentPrecondition precondition)
	{
		switch (precondition.PreconditionType)
		{
		case TournamentPreconditionType.Tournament:
			return string.Format(ScriptLocalization.Get("TournamentPreconditionText"), precondition.TournamentTemplateId);
		case TournamentPreconditionType.Title:
			return string.Format(ScriptLocalization.Get("TitlePreconditionText"), precondition.TournamentTemplateId);
		case TournamentPreconditionType.MinLevel:
			return string.Format(UgcConsts.GetGrey(ScriptLocalization.Get("UGC_MinLevelDetails")), TournamentDetailsPrecondition.LevelColored(precondition.Level));
		case TournamentPreconditionType.MaxLevel:
			return string.Format(UgcConsts.GetGrey(ScriptLocalization.Get("UGC_MaxLevelDetails")), TournamentDetailsPrecondition.LevelColored(precondition.Level));
		default:
			return string.Empty;
		}
	}

	private static string LevelColored(int? lvl)
	{
		return string.Format("<color=#F7F7F7FF> <b>{0}</b></color>", lvl);
	}

	[SerializeField]
	private GameObject _correct;

	[SerializeField]
	private GameObject _inCorrect;

	[SerializeField]
	private Text _value;

	private const int TextFontSize = 30;
}
