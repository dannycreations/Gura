using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LureInfoHandler : MonoBehaviour
{
	private void Awake()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.Refresh;
		PhotonConnectionFactory.Instance.OnInventoryMoved += this.Refresh;
		PhotonConnectionFactory.Instance.OnItemLost += this.OnChumLost;
		PhotonConnectionFactory.Instance.OnMovedTimeForward += this.OnMovedTimeForward;
	}

	private void Start()
	{
		if (GameFactory.Player != null)
		{
			this.playerSubscribed = true;
			PlayerController player = GameFactory.Player;
			player.SwitchRodInitialized = (Action<AssembledRod>)Delegate.Combine(player.SwitchRodInitialized, new Action<AssembledRod>(this.Refresh));
		}
	}

	private void OnDestroy()
	{
		if (this.playerSubscribed && GameFactory.Player != null)
		{
			PlayerController player = GameFactory.Player;
			player.SwitchRodInitialized = (Action<AssembledRod>)Delegate.Remove(player.SwitchRodInitialized, new Action<AssembledRod>(this.Refresh));
		}
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.Refresh;
		PhotonConnectionFactory.Instance.OnInventoryMoved -= this.Refresh;
		PhotonConnectionFactory.Instance.OnItemLost -= this.OnChumLost;
		PhotonConnectionFactory.Instance.OnMovedTimeForward -= this.OnMovedTimeForward;
	}

	private void OnChumLost(InventoryItem chum)
	{
		this._lostChum = chum as Chum;
		this.Refresh();
	}

	private void OnMovedTimeForward(TimeSpan time)
	{
		base.StartCoroutine(this.RefreshInOneFrame());
	}

	private IEnumerator RefreshInOneFrame()
	{
		yield return new WaitForEndOfFrame();
		this.Refresh();
		yield break;
	}

	private void Update()
	{
		if (this._curTime >= 0f)
		{
			this._curTime += Time.deltaTime;
			this._chumLoadingFillerImage.fillAmount = this._curTime / this._loadingDuration;
			if (this._curTime >= this._loadingDuration)
			{
				this._curTime = -1f;
			}
		}
	}

	public void ChumLoading(bool isLoading, float duration = 0f)
	{
		this.Refresh();
		this._loadingDuration = duration;
		ShortcutExtensions.DOKill(this._chumLoadingCg, false);
		ShortcutExtensions.DOKill(this._chumInfoCg, false);
		ShortcutExtensions.DOKill(this._chumLoadingRot, false);
		this._chumLoadingIcon.text = "\ue71a";
		float num = ((!isLoading) ? 0f : 1f);
		if (isLoading)
		{
			Image chumLoadingFillerImage = this._chumLoadingFillerImage;
			float num2 = 0f;
			this._chumLoadingCg.alpha = num2;
			chumLoadingFillerImage.fillAmount = num2;
			this._chumLoadingRot.rotation = Quaternion.identity;
		}
		ShortcutExtensions.DOFade(this._chumInfoCg, 1f - num, 0.2f);
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOFade(this._chumLoadingCg, num, 0.2f), delegate
		{
			if (isLoading)
			{
				this._curTime = 0f;
				TweenSettingsExtensions.SetLoops<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DORotate(this._chumLoadingRot, Vector3.forward * -180f, 1f, 3), 28), -1, 2);
			}
			else if (GameFactory.Player.IsHandThrowMode)
			{
				this._chumLoadingCg.alpha = 1f;
				this._chumLoadingIcon.text = "\ue69f";
				this._chumLoadingFillerImage.fillAmount = 0f;
				this._chumLoadingRot.rotation = Quaternion.identity;
			}
		});
	}

	private void InitWithBaits(AssembledRod rod)
	{
		MonoBehaviour.print("init with baits");
		if (rod == null || rod.Rod == null || rod.Bait == null)
		{
			Debug.LogErrorFormat("LureInfoHandler.InitWithBaits called: rod: {0}, rod.Rod: {1}, rod.Bait: {2}", new object[]
			{
				(rod != null) ? "not null" : "null",
				(rod == null || rod.Rod != null) ? ((rod != null) ? rod.Rod.Name : "null") : "null",
				(rod == null || rod.Bait != null) ? ((rod != null) ? rod.Bait.Name : "null") : "null"
			});
			return;
		}
		InventoryItem inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Equipment && x.ItemId == rod.Bait.ItemId);
		int num = ((inventoryItem == null) ? 0 : inventoryItem.Count);
		InventoryItem inventoryItem2 = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.ParentItem && x.ItemId == rod.Bait.ItemId);
		this.LureCount.text = ((inventoryItem2 != null) ? (num + 1).ToString() : ((rod.Rod.ItemSubType != ItemSubTypes.SpodRod) ? num.ToString() : string.Empty)).ToString();
		if (rod.Rod.ItemSubType == ItemSubTypes.SpodRod && rod.ChumAll.Length > 1)
		{
			Chum chum = ((rod.ChumAll[0] == null) ? null : PhotonConnectionFactory.Instance.Profile.Inventory.FindChumMix(rod.ChumAll[0]));
			float num2 = ((chum == null) ? ((float)rod.ChumAll[0].Weight.Value) : ((float)rod.ChumAll[0].Weight.Value + (float)chum.Weight.Value));
			this.Info.text = string.Format("{0} \n {1} / {2} {3}", new object[]
			{
				rod.ChumAll[0].Name,
				MeasuringSystemManager.ToStringGrams(MeasuringSystemManager.Kilograms2Grams((float)rod.ChumAll[0].Weight.Value)),
				MeasuringSystemManager.ToStringGrams(MeasuringSystemManager.Kilograms2Grams(num2)),
				MeasuringSystemManager.GramsOzWeightSufix()
			});
		}
		else
		{
			this.Info.text = string.Format("{0} \n {1}", rod.Bait.Name, rod.Hook.Name);
		}
		this.LureImg.Image = this.Lure;
		if (rod.Rod.ItemSubType == ItemSubTypes.SpodRod)
		{
			this.LureImg.Load(string.Format("Textures/Inventory/{0}", (rod.ChumAll[0] == null || rod.ChumAll[0].ThumbnailBID == null) ? rod.Feeder.ThumbnailBID.ToString() : rod.ChumAll[0].ThumbnailBID.ToString()));
		}
		else
		{
			this.LureImg.Load((rod.Bait.ThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", rod.Bait.ThumbnailBID));
		}
	}

	private void InitWithLure(AssembledRod rod)
	{
		if (rod == null || rod.Hook == null)
		{
			return;
		}
		InventoryItem inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Equipment && x.ItemId == rod.Hook.ItemId);
		InventoryItem inventoryItem2 = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.ParentItem && x.ItemId == rod.Hook.ItemId);
		int num = ((inventoryItem == null) ? 0 : inventoryItem.Count);
		this.LureCount.text = ((inventoryItem2 != null) ? (num + 1) : num).ToString();
		this.Info.text = string.Format("{0}", rod.Hook.Name);
		this.LureImg.Image = this.Lure;
		this.LureImg.Load((rod.Hook.ThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", rod.Hook.ThumbnailBID));
		this.ResetInfo(32, string.Empty, 4100, false);
	}

	private void InitBottom(AssembledRod rod)
	{
		MonoBehaviour.print("init bottom");
		this.SetBottom();
		this.InitWithBaits(rod);
		Chum chum = rod.Chum;
		Chum chum2 = ((chum == null) ? null : PhotonConnectionFactory.Instance.Profile.Inventory.FindChumMix(chum));
		this.ChumImage.gameObject.SetActive(chum != null);
		if (chum != null && !chum.IsExpired)
		{
			if (this._lostChum != null)
			{
				Guid? instanceId = this._lostChum.InstanceId;
				bool flag = instanceId != null;
				Guid? instanceId2 = chum.InstanceId;
				if (flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault()))
				{
					goto IL_C6;
				}
			}
			if (rod.ChumAll.Length > 1)
			{
				chum = rod.ChumAll[1];
			}
			this.Info.alignment = 260;
			this.Info.fontSize = 24f;
			float num = ((chum2 == null) ? ((float)chum.Weight.Value) : ((float)chum.Weight.Value + (float)chum2.Weight.Value));
			this.InfoChum.text = string.Format("{0} \n {1} / {2} {3}", new object[]
			{
				chum.Name,
				MeasuringSystemManager.ToStringGrams(MeasuringSystemManager.Kilograms2Grams((float)chum.Weight.Value)),
				MeasuringSystemManager.ToStringGrams(MeasuringSystemManager.Kilograms2Grams(num)),
				MeasuringSystemManager.GramsOzWeightSufix()
			});
			this.FillerImage.fillAmount = chum.BaseUsageTimePercentageRemain / 100f;
			this.FillerImage.color = Color.Lerp(Color.yellow, Color.red, 1f - this.FillerImage.fillAmount);
			this.chumLdbl.Image = this.ChumImage;
			this.chumLdbl.Load((rod.ChumAll.Length <= 1 && rod.Rod.ItemSubType == ItemSubTypes.SpodRod) ? string.Empty : string.Format("Textures/Inventory/{0}", chum.ThumbnailBID));
			if (rod.ChumAll.Length == 1 && rod.Rod.ItemSubType == ItemSubTypes.SpodRod)
			{
				this._chumLoadingCg.alpha = 1f;
				this._chumLoadingIcon.text = "\ue69f";
				this._chumLoadingFillerImage.fillAmount = 0f;
				this._chumLoadingRot.rotation = Quaternion.identity;
				return;
			}
			return;
		}
		IL_C6:
		this.ResetInfo(24, "...", 260, true);
	}

	private void InitBaited(AssembledRod rod)
	{
		this.InitWithBaits(rod);
		this.ResetInfo(32, string.Empty, 4100, false);
	}

	public void Refresh()
	{
		this._chumLoadingCg.alpha = 0f;
		this._chumLoadingIcon.text = "\ue71a";
		this._chumLoadingFillerImage.fillAmount = 0f;
		this._chumLoadingRot.rotation = Quaternion.identity;
		if (GameFactory.Player != null && GameFactory.Player.IsHandThrowMode)
		{
			this.Refresh(FeederHelper.FindPreparedChumOnDoll() ?? FeederHelper.FindPreparedChumInHand());
			return;
		}
		if (GameFactory.Player == null || GameFactory.Player.Rod == null || GameFactory.Player.Rod.AssembledRod == null)
		{
			return;
		}
		this.Refresh(GameFactory.Player.Rod.AssembledRod);
	}

	public void Refresh(Chum chumInHands)
	{
		this.SetBottom();
		this._chumLoadingCg.alpha = 1f;
		this._chumLoadingIcon.text = "\ue69f";
		this._chumLoadingFillerImage.fillAmount = 0f;
		this._chumLoadingRot.rotation = Quaternion.identity;
		this.LureCount.text = "\ue795";
		this.Info.text = ScriptLocalization.Get("HandCastCaption");
		this.LureImg.Image = this.Lure;
		this.LureImg.Load((chumInHands == null) ? string.Empty : string.Format("Textures/Inventory/{0}", chumInHands.ThumbnailBID));
		Chum chum = ((chumInHands == null) ? null : PhotonConnectionFactory.Instance.Profile.Inventory.FindChumMix(chumInHands));
		this.ChumImage.gameObject.SetActive(chum != null);
		if (chumInHands == null || chumInHands.IsExpired)
		{
			this.ResetInfo(24, "...", 260, true);
		}
		else
		{
			this.Info.alignment = 260;
			this.Info.fontSize = 24f;
			float num = ((chum == null) ? ((float)chumInHands.Weight.Value) : ((float)chumInHands.Weight.Value + (float)chum.Weight.Value));
			this.InfoChum.text = string.Format("{0} \n {1} / {2} {3}", new object[]
			{
				chumInHands.Name,
				MeasuringSystemManager.ToStringGrams(MeasuringSystemManager.Kilograms2Grams((float)chumInHands.Weight.Value)),
				MeasuringSystemManager.ToStringGrams(MeasuringSystemManager.Kilograms2Grams(num)),
				MeasuringSystemManager.GramsOzWeightSufix()
			});
			this.FillerImage.fillAmount = chumInHands.BaseUsageTimePercentageRemain / 100f;
			this.FillerImage.color = Color.Lerp(Color.yellow, Color.red, 1f - this.FillerImage.fillAmount);
			this.ChumImage.overrideSprite = null;
		}
	}

	private void InitRigs(AssembledRod rod)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		this.SetBottom();
		if (profile == null || rod.Hook == null || rod.Bait == null)
		{
			return;
		}
		bool flag = profile.Inventory.Any((InventoryItem x) => x.Storage == StoragePlaces.ParentItem && x.ItemId == rod.Hook.ItemId);
		InventoryItem inventoryItem = profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Equipment && x.ItemId == rod.Hook.ItemId);
		int num = ((inventoryItem == null) ? 0 : inventoryItem.Count);
		this.LureCount.text = ((!flag) ? string.Empty : (num + 1).ToString());
		this.Info.text = string.Format("{0} \n {1}", rod.Hook.Name, rod.Bait.Name);
		this.LureImg.Load(rod.Hook.ThumbnailBID, this.Lure, "Textures/Inventory/{0}");
		this.chumLdbl.Load(rod.Bait.ThumbnailBID, this.ChumImage, "Textures/Inventory/{0}");
		this.ChumImage.gameObject.SetActive(true);
		this.ResetInfo(24, string.Empty, 260, false);
	}

	public void Refresh(AssembledRod rod)
	{
		if (rod == null)
		{
			return;
		}
		this.SetUsual();
		switch (rod.RodTemplate)
		{
		case RodTemplate.UnEquiped:
			this.LureCount.text = string.Empty;
			this.Info.text = string.Empty;
			this.InfoChum.text = string.Empty;
			this.FillerImage.fillAmount = 0f;
			break;
		case RodTemplate.Float:
		case RodTemplate.Jig:
			this.InitBaited(rod);
			break;
		case RodTemplate.Lure:
			this.InitWithLure(rod);
			break;
		case RodTemplate.Bottom:
		case RodTemplate.MethodCarp:
		case RodTemplate.PVACarp:
		case RodTemplate.Spod:
			this.InitBottom(rod);
			break;
		case RodTemplate.ClassicCarp:
			if (rod.Sinker != null && rod.Sinker.ItemSubType == ItemSubTypes.Sinker)
			{
				this.InitBaited(rod);
			}
			else
			{
				this.InitBottom(rod);
			}
			break;
		case RodTemplate.FlippingRig:
		case RodTemplate.SpinnerTails:
		case RodTemplate.SpinnerbaitTails:
			this.InitRigs(rod);
			break;
		case RodTemplate.CarolinaRig:
		case RodTemplate.TexasRig:
		case RodTemplate.ThreewayRig:
		case RodTemplate.OffsetJig:
			this.InitWithBaits(rod);
			break;
		}
	}

	private void ResetInfo(int fontSize = 24, string infoChum = "", TextAlignmentOptions alignment = 260, bool clearChumImage = false)
	{
		this.InfoChum.text = infoChum;
		this.Info.alignment = alignment;
		this.Info.fontSize = (float)fontSize;
		this.FillerImage.fillAmount = 0f;
		if (clearChumImage)
		{
			this.ChumImage.overrideSprite = null;
		}
	}

	private void SetUsual()
	{
		if (this.BgImage.overrideSprite != this.BgUsual)
		{
			this.BgImage.overrideSprite = this.BgUsual;
		}
		if (this.ChumImage.gameObject.activeInHierarchy)
		{
			this.ChumImage.gameObject.SetActive(false);
		}
	}

	private void SetBottom()
	{
		if (this.BgImage.overrideSprite != this.BgBottom)
		{
			this.BgImage.overrideSprite = this.BgBottom;
		}
	}

	[SerializeField]
	private Image _chumLoadingFillerImage;

	[SerializeField]
	private CanvasGroup _chumLoadingCg;

	[SerializeField]
	private Transform _chumLoadingRot;

	[SerializeField]
	private TextMeshProUGUI _chumLoadingIcon;

	[SerializeField]
	private CanvasGroup _chumInfoCg;

	private const string _chumIcon = "\ue69f";

	private const string _sinkerIcon = "\ue73b";

	private const string _loadingIcon = "\ue71a";

	public Image Lure;

	private ResourcesHelpers.AsyncLoadableImage LureImg = new ResourcesHelpers.AsyncLoadableImage();

	public TextMeshProUGUI LureCount;

	public TextMeshProUGUI Info;

	public TextMeshProUGUI InfoChum;

	public Image BgImage;

	public Image FillerImage;

	public Image ChumImage;

	private ResourcesHelpers.AsyncLoadableImage chumLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	private Sprite BgUsual;

	[SerializeField]
	private Sprite BgBottom;

	private bool playerSubscribed;

	private const float ChumLoadingAlphaTime = 0.2f;

	private float _curTime = -1f;

	private Chum _lostChum;

	private float _loadingDuration;
}
