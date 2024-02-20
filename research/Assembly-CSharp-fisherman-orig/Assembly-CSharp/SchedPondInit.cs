using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class SchedPondInit : MonoBehaviour
{
	public void Init(List<Tournament> pondTournaments, int pondId, bool isEven = false)
	{
		this._currentPondTournaments = pondTournaments;
		Pond pond = CacheLibrary.MapCache.CachedPonds.First((Pond x) => x.PondId == pondId);
		this.PondNameText.text = pond.Name;
		if (this.PondImage != null)
		{
			this.PondImageLoadable.Image = this.PondImage;
			this.PondImageLoadable.Load(string.Format("Textures/Inventory/{0}", pond.PhotoBID));
		}
		this.SetColor(isEven);
		if (this._currentPondTournaments == null || pondTournaments.Count == 0)
		{
			return;
		}
		pondTournaments[0].StartDate.ToLocalTime();
		int hour = pondTournaments[0].StartDate.ToLocalTime().Hour;
		Tournament tournament = pondTournaments.FirstOrDefault((Tournament x) => x.StartDate.ToLocalTime().Hour >= 0 && x.StartDate.ToLocalTime().Hour < 4);
		if (tournament != null)
		{
			this.Time4.Init(tournament);
		}
		Tournament tournament2 = pondTournaments.FirstOrDefault((Tournament x) => x.StartDate.ToLocalTime().Hour >= 4 && x.StartDate.ToLocalTime().Hour < 8);
		if (tournament2 != null)
		{
			this.Time8.Init(tournament2);
		}
		Tournament tournament3 = pondTournaments.FirstOrDefault((Tournament x) => x.StartDate.ToLocalTime().Hour >= 8 && x.StartDate.ToLocalTime().Hour < 12);
		if (tournament3 != null)
		{
			this.Time12.Init(tournament3);
		}
		Tournament tournament4 = pondTournaments.FirstOrDefault((Tournament x) => x.StartDate.ToLocalTime().Hour >= 12 && x.StartDate.ToLocalTime().Hour < 16);
		if (tournament4 != null)
		{
			this.Time16.Init(tournament4);
		}
		Tournament tournament5 = pondTournaments.FirstOrDefault((Tournament x) => x.StartDate.ToLocalTime().Hour >= 16 && x.StartDate.ToLocalTime().Hour < 20);
		if (tournament5 != null)
		{
			this.Time20.Init(tournament5);
		}
		Tournament tournament6 = pondTournaments.FirstOrDefault((Tournament x) => x.StartDate.ToLocalTime().Hour >= 20 && x.StartDate.ToLocalTime().Hour < 24);
		if (tournament6 != null)
		{
			this.Time24.Init(tournament6);
		}
	}

	private void SetColor(bool isEven)
	{
		this.PondNameText.transform.parent.GetComponent<Image>().enabled = !isEven;
		this.Time4.GetComponent<Image>().enabled = isEven;
		this.Time8.GetComponent<Image>().enabled = !isEven;
		this.Time12.GetComponent<Image>().enabled = isEven;
		this.Time16.GetComponent<Image>().enabled = !isEven;
		this.Time20.GetComponent<Image>().enabled = isEven;
		this.Time24.GetComponent<Image>().enabled = !isEven;
		this.LastCell.GetComponent<Image>().enabled = isEven;
	}

	public Text PondNameText;

	public Image PondImage;

	private ResourcesHelpers.AsyncLoadableImage PondImageLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public SchedTournamentInit Time4;

	public SchedTournamentInit Time8;

	public SchedTournamentInit Time12;

	public SchedTournamentInit Time16;

	public SchedTournamentInit Time20;

	public SchedTournamentInit Time24;

	public GameObject LastCell;

	private List<Tournament> _currentPondTournaments;
}
