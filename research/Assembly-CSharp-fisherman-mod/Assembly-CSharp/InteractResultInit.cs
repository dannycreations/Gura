using System;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractResultInit : MonoBehaviour
{
	internal void InitItemGained(InventoryItem itemsGained, int? amount, string currency, bool excess = false)
	{
		if (itemsGained != null)
		{
			this.Image.gameObject.SetActive(true);
			this.Icon.gameObject.SetActive(false);
			this.ImgLdbl.Image = this.Image;
			this.ImgLdbl.Load(string.Format("Textures/Inventory/{0}", itemsGained.ThumbnailBID));
			string text = itemsGained.Name;
			if (excess)
			{
				text = string.Format(ScriptLocalization.Get("GiftPlacedToEscessTab"), "\n" + itemsGained.Name);
			}
			this.Desc.text = text;
		}
		else if (amount != null && amount.Value > 0)
		{
			this.Icon.gameObject.SetActive(true);
			this.Image.gameObject.SetActive(false);
			this.Icon.text = MeasuringSystemManager.GetCurrencyIcon(currency);
			this.Desc.text = amount.Value.ToString() + " " + ((!(currency == "SC")) ? ScriptLocalization.Get("BaitcoinsDelivered") : ScriptLocalization.Get("MoneyCreditsCaption")) + "!";
		}
		else
		{
			this.InitEmpty();
		}
		this.Caption.text = ScriptLocalization.Get("InteractRewardCaption");
	}

	public void InitEmpty()
	{
		this.Caption.text = ScriptLocalization.Get("DeliveredCaption");
		this.Image.gameObject.SetActive(false);
		this.Icon.gameObject.SetActive(false);
		this.Desc.text = ScriptLocalization.Get("NothingDelivered");
	}

	public void InitProductDelivered(ProfileProduct product, bool excess = false)
	{
		this.Caption.text = ScriptLocalization.Get("DeliveredCaption");
		this.Icon.gameObject.SetActive(false);
		string text = product.Name;
		if (excess)
		{
			text = string.Format(ScriptLocalization.Get("GiftPlacedToEscessTab"), "\n" + product.Name);
		}
		this.Desc.text = text;
		if (product.ImageBID != null)
		{
			this.Image.gameObject.SetActive(true);
			this.ImgLdbl.Image = this.Image;
			this.ImgLdbl.Load(string.Format("Textures/Inventory/{0}", product.ImageBID));
		}
	}

	internal void InitGift(Player player, InventoryItem gift)
	{
		this.Image.gameObject.SetActive(true);
		this.Caption.text = string.Format(ScriptLocalization.Get("RecievedGiftText"), player.UserName);
		this.ImgLdbl.Image = this.Image;
		this.ImgLdbl.Load(string.Format("Textures/Inventory/{0}", gift.ThumbnailBID));
		this.Desc.text = string.Format("{0} x {1}\n<size=20><color=#808080FF>{2}</color></size>", gift.Name, gift.Count, ScriptLocalization.Get(NameResolvingHelpers.GetStorageName(gift)));
	}

	public void InitCompetitionStarted(TournamentStartInfo startInfo)
	{
		Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.PondId == startInfo.PondId);
		this.Caption.text = ScriptLocalization.Get("CompetitionStartedCaption").ToUpper();
		this.Image.gameObject.SetActive(true);
		this.ImgLdbl.Image = this.Image;
		this.ImgLdbl.Load(string.Format("Textures/Inventory/{0}", startInfo.ImageBID));
		string text = ScriptLocalization.Get("TrophiesPondStatCaption");
		string text2 = ScriptLocalization.Get("EndsInTournamentTimeText");
		this.Desc.fontSize = 32f;
		this.Desc.text = startInfo.Name;
		this.DescAdditional.text = string.Format("<color=#7C7C7CFF>{0}</color> {1}\n <color=#7C7C7CFF>{2}</color> {3}", new object[]
		{
			text,
			pond.Name,
			text2,
			MeasuringSystemManager.DateTimeString(startInfo.EndDate.ToLocalTime())
		});
	}

	public void InitCompetitionStarted(UserCompetitionStartMessage startInfo)
	{
		Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.PondId == startInfo.PondId);
		this.Caption.text = ScriptLocalization.Get("CompetitionStartedCaption").ToUpper();
		this.Image.gameObject.SetActive(true);
		this.ImgLdbl.Image = this.Image;
		this.ImgLdbl.Load(string.Format("Textures/Inventory/{0}", startInfo.ImageBID));
		string text = ScriptLocalization.Get("TrophiesPondStatCaption");
		string text2 = ScriptLocalization.Get("EndsInTournamentTimeText");
		this.Desc.fontSize = 32f;
		UserCompetitionHelper.GetDefaultName(this.Desc, new UserCompetitionPublic
		{
			HostName = startInfo.HostName,
			Format = startInfo.Format,
			Type = startInfo.Type,
			TemplateName = startInfo.TemplateName,
			IsSponsored = startInfo.IsSponsored,
			NameCustom = startInfo.NameCustom,
			TournamentId = startInfo.TournamentId
		});
		this.DescAdditional.text = string.Format("<color=#7C7C7CFF>{0}</color> {1}\n <color=#7C7C7CFF>{2}</color> {3}", new object[]
		{
			text,
			pond.Name,
			text2,
			MeasuringSystemManager.DateTimeString(startInfo.EndDate.ToLocalTime())
		});
	}

	public void InitItemWearedOnDeequip(InventoryItem item)
	{
		this.Caption.text = ScriptLocalization.Get("MessageCaption").ToUpper();
		this.Caption.color = Color.red;
		this.Image.gameObject.SetActive(true);
		this.ImgLdbl.Load(item.ThumbnailBID, this.Image, "Textures/Inventory/{0}");
		this.Desc.text = string.Format("<color=#FF0000FF>{0}</color>\n{1}", ScriptLocalization.Get("WearedItemLostOnDeequip"), UgcConsts.GetYellowTan(item.Name));
	}

	private const string GiftFormatString = "{0} x {1}\n<size=20><color=#808080FF>{2}</color></size>";

	private const string CompetitionInfoFormatStr = "<color=#7C7C7CFF>{0}</color> {1}\n <color=#7C7C7CFF>{2}</color> {3}";

	private ResourcesHelpers.AsyncLoadableImage ImgLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	private Image Image;

	[SerializeField]
	private TextMeshProUGUI Caption;

	[SerializeField]
	private TextMeshProUGUI Desc;

	[SerializeField]
	private TextMeshProUGUI DescAdditional;

	[SerializeField]
	private TextMeshProUGUI Icon;
}
