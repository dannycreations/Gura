using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TournamentStatInit : MonoBehaviour
{
	public void Init(TournamentSerieStat stat)
	{
		this.TournamentTitle.text = stat.SerieName;
		this.UserTournamentStatus.text = stat.Place.Value.ToString();
		this.TournamentLogoLoadable.Image = this.TournamentLogo;
		this.TournamentLogoLoadable.Load(string.Format("Textures/Inventory/{0}", stat.LogoBID));
	}

	public Text TournamentTitle;

	public Text UserTournamentStatus;

	public Image TournamentLogo;

	private ResourcesHelpers.AsyncLoadableImage TournamentLogoLoadable = new ResourcesHelpers.AsyncLoadableImage();
}
