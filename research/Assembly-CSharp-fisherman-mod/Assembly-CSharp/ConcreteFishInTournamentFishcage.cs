using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConcreteFishInTournamentFishcage : MonoBehaviour
{
	public CaughtFish Fish { get; private set; }

	internal void Init(CaughtFish fish)
	{
		this.Fish = fish;
		this.Name.text = fish.Fish.Name;
		this.Weight.text = string.Format("{0} {1}", MeasuringSystemManager.FishWeight(fish.Fish.Weight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
		this.Money.text = ((fish.Fish.GoldCost == null) ? string.Format("<size=17>\ue62b</size> {0}", fish.Fish.SilverCost) : string.Format("<size=17>\ue62c</size> {0}", fish.Fish.GoldCost));
		this.TournamentIcon.SetActive(this.Fish.Fish.TournamentScore != null);
		this.Release.onClick.AddListener(new UnityAction(this.ReleaseFish));
		this.Release.interactable = PhotonConnectionFactory.Instance.Profile.FishCage.Cage.Safety;
	}

	private void ReleaseFish()
	{
		if (this.Fish == null || !PhotonConnectionFactory.Instance.Profile.FishCage.Cage.Safety)
		{
			return;
		}
		UIHelper.Waiting(true, null);
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage += this.OnFishReleased;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed += this.OnFishReleaseFailed;
		PhotonConnectionFactory.Instance.ReleaseFishFromFishCage(this.Fish.Fish);
		if (this.Fish.Fish != null && this.Fish.Fish.NoRelease)
		{
			FishPenaltyHelper fishPenaltyHelper = MessageBoxList.Instance.gameObject.AddComponent<FishPenaltyHelper>();
			fishPenaltyHelper.CheckPenalty(true);
		}
	}

	public void OnFishReleaseFailed(Failure failure)
	{
		LogHelper.Error(failure.ErrorMessage, new object[0]);
		UIHelper.Waiting(false, null);
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage -= this.OnFishReleased;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed -= this.OnFishReleaseFailed;
	}

	public void OnFishReleased(Guid fishId)
	{
		UIHelper.Waiting(false, null);
		if (fishId != this.Fish.Fish.InstanceId.Value)
		{
			return;
		}
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCage -= this.OnFishReleased;
		PhotonConnectionFactory.Instance.OnReleaseFishFromFishCageFailed -= this.OnFishReleaseFailed;
	}

	public Text Name;

	public Text Weight;

	public Text Money;

	public Button Release;

	public GameObject TournamentIcon;
}
