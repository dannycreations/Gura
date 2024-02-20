using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FishcagePanelInit : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnFishReleased = delegate
	{
	};

	public void Init(int allowedFish)
	{
		this.Init(true, allowedFish);
	}

	public void Init()
	{
		this.Init(false, 0);
	}

	internal void OnEnable()
	{
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage += this.OnRelease;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed += this.OnReleaseFailed;
	}

	internal void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage -= this.OnRelease;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed -= this.OnReleaseFailed;
	}

	internal void OnRelease(Guid fishInstanceId)
	{
		if (PhotonConnectionFactory.Instance.Profile == null || PhotonConnectionFactory.Instance.Profile.FishCage == null || PhotonConnectionFactory.Instance.Profile.FishCage.Fish == null)
		{
			return;
		}
		CaughtFish caughtFish = PhotonConnectionFactory.Instance.Profile.FishCage.FindFishById(fishInstanceId);
		if (caughtFish != null)
		{
			PhotonConnectionFactory.Instance.Profile.FishCage.RemoveFish(caughtFish);
		}
		this.ReleaseFish(caughtFish);
		this.OnFishReleased();
	}

	internal void OnReleaseFailed(Failure failure)
	{
		LogHelper.Log(failure.ErrorMessage);
	}

	private void ReleaseFish(CaughtFish fishToRemove)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile != null && fishToRemove != null)
		{
			FishCageContents fishCage = profile.FishCage;
			if (fishCage != null && fishCage.Fish != null)
			{
				float num = 0f;
				float num2 = 0f;
				List<GameObject> list = new List<GameObject>();
				for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
				{
					Transform child = this.ContentPanel.transform.GetChild(i);
					ConcreteFishInTournamentFishcage fish = child.GetComponent<ConcreteFishInTournamentFishcage>();
					if (fish.Fish == null || fish.Fish == fishToRemove || !fishCage.Fish.Exists(delegate(CaughtFish p)
					{
						Guid? instanceId = p.Fish.InstanceId;
						bool flag = instanceId != null;
						Guid? instanceId2 = fish.Fish.Fish.InstanceId;
						return flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault());
					}))
					{
						list.Add(child.gameObject);
					}
					else
					{
						this.SummMoney(fish.Fish.Fish, ref num, ref num2);
					}
				}
				list.ForEach(new Action<GameObject>(Object.Destroy));
				this.UpdateTexts(this.currentAllowedFishCount, fishCage, num, num2);
			}
		}
	}

	private void Init(bool orderByWeight, int allowedFishCount)
	{
		this.Clear();
		this.currentAllowedFishCount = allowedFishCount;
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile != null)
		{
			FishCageContents fishCage = profile.FishCage;
			if (fishCage != null && fishCage.Fish != null)
			{
				float num = 0f;
				float num2 = 0f;
				List<CaughtFish> list;
				if (orderByWeight)
				{
					list = fishCage.Fish.OrderBy((CaughtFish x) => x.Fish.Weight).ToList<CaughtFish>();
				}
				else
				{
					list = fishCage.Fish;
				}
				List<CaughtFish> list2 = list;
				for (int i = 0; i < list2.Count; i++)
				{
					GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.FishItemPrefab);
					ConcreteFishInTournamentFishcage component = gameObject.GetComponent<ConcreteFishInTournamentFishcage>();
					component.Init(list2[i]);
					this.SummMoney(component.Fish.Fish, ref num, ref num2);
				}
				this.UpdateTexts(allowedFishCount, fishCage, num, num2);
			}
			else
			{
				this._addInfo.SetActive(false);
			}
		}
		this.InfoText.gameObject.SetActive(allowedFishCount > 0);
		this.FishAllowed.SetActive(allowedFishCount > 0);
	}

	private void Clear()
	{
		for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.ContentPanel.transform.GetChild(i).gameObject);
		}
	}

	private void UpdateTexts(int allowedFish, FishCageContents fishNet, float gold, float silver)
	{
		this.InfoText.text = ((PhotonConnectionFactory.Instance.CurrentTournamentId == null) ? ScriptLocalization.Get("InfoCatchReleaseText") : ScriptLocalization.Get("KeepnetReleaseTextForTournament"));
		Text totalWeight = this._totalWeight;
		float? weight = fishNet.Weight;
		string text = MeasuringSystemManager.FishWeight((weight == null) ? 0f : weight.Value).ToString("n3") + " " + MeasuringSystemManager.FishWeightSufix();
		this.TotalWeightText.text = text;
		totalWeight.text = text;
		this.AllowedFishText.text = ((allowedFish <= 0) ? string.Empty : string.Format("{0}/{1}", fishNet.Fish.Count, allowedFish));
		string text2 = ((gold <= 0f) ? string.Empty : string.Format("\ue62c  {0}", gold));
		string text3 = ((silver <= 0f) ? string.Empty : string.Format("\ue62b  {0}", silver));
		this._totalMoney.text = ((gold <= 0f) ? string.Format("{0}", text3) : string.Format("{0} {1}", text2, text3));
		this._addInfoTournament.SetActive(allowedFish > 0);
		this._addInfo.SetActive(!this._addInfoTournament.activeSelf);
		this._totalCaption.text = string.Format("{0} <color=#AAAAAAFF>({1})</color>", ScriptLocalization.Get("[TotalTravelCaption]"), fishNet.Fish.Count);
	}

	private void SummMoney(Fish f, ref float gold, ref float silver)
	{
		if (f.GoldCost != null)
		{
			gold += f.GoldCost.Value;
		}
		else
		{
			silver += f.SilverCost.Value;
		}
	}

	[SerializeField]
	private GameObject _addInfo;

	[SerializeField]
	private GameObject _addInfoTournament;

	[SerializeField]
	private Text _totalWeight;

	[SerializeField]
	private Text _totalMoney;

	[SerializeField]
	private Text _totalCaption;

	public GameObject ContentPanel;

	public GameObject FishItemPrefab;

	public Text AllowedFishText;

	public Text TotalWeightText;

	public Text InfoText;

	public GameObject FishAllowed;

	private int currentAllowedFishCount;
}
