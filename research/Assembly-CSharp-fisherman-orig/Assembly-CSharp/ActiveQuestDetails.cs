using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActiveQuestDetails : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnUntrackQuest = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> OnTrackQuest = delegate
	{
	};

	private void Awake()
	{
		this._fade = base.GetComponent<AlphaFade>();
		this.QuestImage.gameObject.SetActive(false);
		this._unTrackMission.interactable = false;
		this._unTrackMission.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		this.ClearRewardsItems();
		this._stepsHandlers.Clear();
	}

	public void Blink()
	{
		this._fade.OnHide.AddListener(new UnityAction(this.ShowAndInit));
		this._fade.HidePanel();
	}

	public void InitAndBlink(MissionOnClient quest, bool isBlink = true)
	{
		this._currMisssion = quest;
		if (isBlink)
		{
			this.Blink();
		}
	}

	public void SetEmptyList()
	{
		if (this._fade == null)
		{
			this.Awake();
		}
		this._currMisssion = null;
		this.QuestImage.gameObject.SetActive(false);
		this._progressGameObject.SetActive(false);
		this.SummaryPanel.SetActive(false);
		this._content1.SetActive(false);
		this._content2.SetActive(false);
		this._fade.OnHide.RemoveAllListeners();
		this._fade.OnShow.RemoveAllListeners();
		this._fade.ShowPanel();
	}

	public void Init(MissionOnClient mission)
	{
		this._stepsHandlers.Clear();
		this._content1.SetActive(true);
		this._content2.SetActive(true);
		this.SummaryPanel.SetActive(true);
		if (mission.ThumbnailBID != null)
		{
			this.QuestImageLdbl.Image = this.QuestImage;
			this.QuestImageLdbl.Load(string.Format("Textures/Inventory/{0}", mission.ThumbnailBID.Value));
			this.QuestImage.gameObject.SetActive(true);
		}
		else
		{
			this.QuestImage.gameObject.SetActive(false);
		}
		this.QuestName.text = mission.Name.ToUpper();
		this.QuestDescription.text = mission.Description;
		this.RefreshApplyButton();
		this.ClearSteps();
		List<MissionTaskOnClient> list = mission.Tasks.OrderBy((MissionTaskOnClient p) => p.IsCompleted).ToList<MissionTaskOnClient>();
		int num = mission.Tasks.Count - mission.Tasks.FindAll((MissionTaskOnClient t) => t.IsCompleted && t.IsHiddenWhenCompleted).Count;
		ushort num2 = 0;
		int num3 = 0;
		for (int i = 0; i < mission.Tasks.Count; i++)
		{
			MissionTaskOnClient missionTaskOnClient = list[i];
			if (!missionTaskOnClient.IsCompleted || !missionTaskOnClient.IsHiddenWhenCompleted)
			{
				GameObject gameObject = GUITools.AddChild(this.StepsParent.gameObject, this.StepPrefab);
				QuestStepItemHandler component = gameObject.GetComponent<QuestStepItemHandler>();
				component.Init(missionTaskOnClient.TaskId, missionTaskOnClient.Name, missionTaskOnClient.Progress, num3 == 0, missionTaskOnClient.IsCompleted, missionTaskOnClient.IsHidden);
				this._stepsHandlers.Add(component);
				num3++;
				if (missionTaskOnClient.IsCompleted)
				{
					num2 += 1;
				}
			}
		}
		this._progressGameObject.SetActive(true);
		this._textProgress.text = num2.ToString();
		this._textProgressCount.text = string.Format("/{0}", num);
		this._imageProgress.fillAmount = (float)num2 / (float)num;
		this.InitRewards(mission.Reward);
		float num4 = 1f;
		if (ClientMissionsManager.CurrentWidgetTaskId != null)
		{
			int taskId = ClientMissionsManager.CurrentWidgetTaskId.Value;
			int num5 = this._stepsHandlers.FindIndex((QuestStepItemHandler p) => p.TaskId == taskId);
			if (num5 != -1)
			{
				this._stepsHandlers[num5].ActiveGlow();
				num4 = Mathf.Max(1f - (0.3f + 0.7f / (float)this._stepsHandlers.Count * (float)num5), 0f);
			}
		}
		base.StartCoroutine(this.SetScrollPosition(num4));
	}

	public void RefreshApplyButton()
	{
		if (this._currMisssion == null || this._currMisssion.IsCompleted || this._currMisssion.IsArchived)
		{
			Selectable unTrackMission = this._unTrackMission;
			bool flag = false;
			this.ApplyButton.interactable = flag;
			unTrackMission.interactable = flag;
			this.ApplyButton.gameObject.SetActive(false);
			this._unTrackMission.gameObject.SetActive(false);
			return;
		}
		this.ApplyButton.interactable = !this._currMisssion.IsActiveMission;
		this.ApplyButton.gameObject.SetActive(this.ApplyButton.interactable);
		this._unTrackMission.interactable = !this.ApplyButton.interactable;
		this._unTrackMission.gameObject.SetActive(this._unTrackMission.interactable);
	}

	public void ClearSteps()
	{
		IEnumerator enumerator = this.StepsParent.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				Object.Destroy(transform.gameObject);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
	}

	public void SetActive()
	{
		if (this._currMisssion != null)
		{
			LogHelper.Log("Set mission active called " + this._currMisssion.MissionId);
			this.OnTrackQuest(this._currMisssion.MissionId);
		}
	}

	public void SetUnActive()
	{
		LogHelper.Log("Mission:SetUnActive");
		this.OnUntrackQuest();
	}

	private void ShowAndInit()
	{
		this._fade.OnHide.RemoveListener(new UnityAction(this.ShowAndInit));
		if (this._currMisssion != null)
		{
			this.Init(this._currMisssion);
		}
		this._fade.ShowPanel();
	}

	private IEnumerator SetScrollPosition(float pos)
	{
		yield return null;
		this._scrollContent.verticalNormalizedPosition = pos;
		yield break;
	}

	private void InitRewards(Reward r)
	{
		this.ClearRewardsItems();
		if (r == null)
		{
			this._rewardItemsGameObject.GetComponent<CanvasGroup>().alpha = 0f;
			return;
		}
		int num = 0;
		int num2 = 0;
		this.ParseAmount(r.Money1, r.Currency1, ref num2, ref num);
		this.ParseAmount(r.Money2, r.Currency2, ref num2, ref num);
		string text = r.Experience.ToString();
		this._rewardsHandler.Init(text, (num <= 0) ? null : num.ToString(), (num2 <= 0) ? null : num2.ToString());
		ProductReward[] products = r.GetProductRewards();
		ItemReward[] itemRewards = r.GetItemRewards();
		LicenseRef[] licensees = r.GetLicenseRewards();
		bool flag = products != null || itemRewards != null || licensees != null;
		this._rewardItemsGameObject.GetComponent<CanvasGroup>().alpha = (float)((!flag) ? 0 : 1);
		this._rewardItemsGameObject.SetActive(flag);
		if (products != null)
		{
			int k;
			for (k = 0; k < products.Length; k++)
			{
				StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == products[k].ProductId);
				if (storeProduct == null)
				{
					Debug.LogErrorFormat("ActiveQuestDetails:InitRewards - can't found item:{0} in ProductCache.Products", new object[] { products[k].ProductId });
				}
				else
				{
					this.AddRewardItem().FillData(storeProduct);
				}
			}
		}
		if (itemRewards != null)
		{
			int[] array = (from p in itemRewards
				where !this._itemRewards.Contains(p.ItemId)
				select p into x
				select x.ItemId).ToArray<int>();
			if (array.Length > 0)
			{
				this._itemRewards.AddRange(array);
				CacheLibrary.ItemsCache.OnGotItems += this.ItemsCache_OnGotItems;
				CacheLibrary.ItemsCache.GetItems(array, this._subscriberId);
			}
		}
		if (licensees != null)
		{
			int i;
			for (i = 0; i < licensees.Length; i++)
			{
				ShopLicense[] array2 = CacheLibrary.MapCache.AllLicenses.Where((ShopLicense p) => p.LicenseId == licensees[i].LicenseId).ToArray<ShopLicense>();
				for (int j = 0; j < array2.Length; j++)
				{
					this.AddRewardItem().FillData(array2[j]);
				}
			}
		}
		this.SetVisibilityRewardsButtons();
	}

	private void ItemsCache_OnGotItems(List<InventoryItem> items, int sunscriberId)
	{
		ActiveQuestDetails.<ItemsCache_OnGotItems>c__AnonStorey5 <ItemsCache_OnGotItems>c__AnonStorey = new ActiveQuestDetails.<ItemsCache_OnGotItems>c__AnonStorey5();
		<ItemsCache_OnGotItems>c__AnonStorey.items = items;
		if (this._subscriberId != sunscriberId)
		{
			return;
		}
		CacheLibrary.ItemsCache.OnGotItems -= this.ItemsCache_OnGotItems;
		int i;
		for (i = 0; i < <ItemsCache_OnGotItems>c__AnonStorey.items.Count; i++)
		{
			int num = this._itemRewards.Count((int p) => p == <ItemsCache_OnGotItems>c__AnonStorey.items[i].ItemId);
			this.AddRewardItem().FillData(<ItemsCache_OnGotItems>c__AnonStorey.items[i], num);
		}
		this._itemRewards.Clear();
		this.SetVisibilityRewardsButtons();
	}

	private RewardItem AddRewardItem()
	{
		GameObject gameObject = GUITools.AddChild(this._rewardsItemsContentPanel, this._rewardItemPrefab);
		RewardItem component = gameObject.GetComponent<RewardItem>();
		this._rewardItemsGo.Add(component);
		return component;
	}

	private void ClearRewardsItems()
	{
		for (int i = 0; i < this._rewardItemsGo.Count; i++)
		{
			Object.Destroy(this._rewardItemsGo[i].gameObject);
		}
		this._rewardItemsGo.Clear();
	}

	private void ParseAmount(double? m, string c, ref int silver, ref int gold)
	{
		if (m != null && m.Value > 0.0)
		{
			if (c == "SC")
			{
				silver = (int)m.Value;
			}
			else if (c == "GC")
			{
				gold = (int)m.Value;
			}
		}
	}

	private void SetVisibilityRewardsButtons()
	{
		bool flag = this._rewardsItemsContentPanel.transform.childCount > 3;
		for (int i = 0; i < this._rewardsButtons.Length; i++)
		{
			this._rewardsButtons[i].SetActive(flag);
		}
	}

	[SerializeField]
	private GameObject[] _rewardsButtons;

	[SerializeField]
	private ScrollRect _scrollContent;

	[SerializeField]
	private QuestRewardsItemHandler _rewardsHandler;

	[SerializeField]
	private GameObject _progressGameObject;

	[SerializeField]
	private Button _unTrackMission;

	[SerializeField]
	private Text _textProgress;

	[SerializeField]
	private Text _textProgressCount;

	[SerializeField]
	private Image _imageProgress;

	[SerializeField]
	private GameObject _rewardItemsGameObject;

	[SerializeField]
	private GameObject _rewardItemPrefab;

	[SerializeField]
	private GameObject _rewardsItemsContentPanel;

	public GameObject StepPrefab;

	public Transform StepsParent;

	public Text QuestName;

	public Text QuestDescription;

	public Image QuestImage;

	private ResourcesHelpers.AsyncLoadableImage QuestImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Button ApplyButton;

	public GameObject SummaryPanel;

	public GameObject _content1;

	public GameObject _content2;

	private AlphaFade _fade;

	private MissionOnClient _currMisssion;

	private List<RewardItem> _rewardItemsGo = new List<RewardItem>();

	private List<QuestStepItemHandler> _stepsHandlers = new List<QuestStepItemHandler>();

	private const float StepHandlerWidthInit = 0.3f;

	private const int RewardsCount4Scroll = 3;

	private int _subscriberId = new Random().Next(99999);

	private List<int> _itemRewards = new List<int>();
}
