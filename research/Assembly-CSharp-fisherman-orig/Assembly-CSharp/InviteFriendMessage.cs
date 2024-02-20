using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class InviteFriendMessage : MonoBehaviour
{
	public void InitWithReward(ReferralReward reward)
	{
		if (reward == null)
		{
			return;
		}
		this._reward = reward;
		this.Description.text = string.Format(ScriptLocalization.Get("InviteFriendDescription"), reward.FinalRewardLevel).Replace("<br>", "\n");
		IEnumerable<int> enumerable = new List<int>();
		if (reward.FinalInvitedReward != null && reward.FinalInvitedReward.GetItemRewards() != null)
		{
			enumerable = from item in reward.FinalInvitedReward.GetItemRewards()
				select item.ItemId;
		}
		if (reward.FinalInvitingReward != null && reward.FinalInvitingReward.GetItemRewards() != null)
		{
			enumerable = enumerable.Concat(from item in reward.FinalInvitingReward.GetItemRewards()
				select item.ItemId);
		}
		if (reward.StartPlayInvitedReward != null && reward.StartPlayInvitedReward.GetItemRewards() != null)
		{
			enumerable = enumerable.Concat(from item in reward.StartPlayInvitedReward.GetItemRewards()
				select item.ItemId);
		}
		if (reward.StartPlayInvitingReward != null && reward.StartPlayInvitingReward.GetItemRewards() != null)
		{
			enumerable = enumerable.Concat(from item in reward.StartPlayInvitingReward.GetItemRewards()
				select item.ItemId);
		}
		if (enumerable.Count<int>() > 0)
		{
			PhotonConnectionFactory.Instance.GetItemsByIds(enumerable.ToArray<int>(), 500, true);
			PhotonConnectionFactory.Instance.OnGotItems += this.OnGotItems;
		}
		else
		{
			this.OnGotItems(null, 500);
		}
	}

	private void OnGotItems(List<InventoryItem> items, int sunscriberId)
	{
		if (sunscriberId == 500)
		{
			this._items = items;
			PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
			this.Caption1.text = ScriptLocalization.Get("OnceRegisteredCaption");
			this.Caption2.text = string.Format(ScriptLocalization.Get("FriendReachesLevelCaption"), this._reward.FinalRewardLevel);
			this.ShowReward(this._reward.StartPlayInvitingReward, "YouGetCaption");
			this.ShowReward(this._reward.StartPlayInvitedReward, "FriendGetsCaption");
			GUITools.AddChild(this.ContentRoot, this.Space).GetComponent<Text>().text = string.Empty;
			this.Caption2.transform.SetAsLastSibling();
			this.ShowReward(this._reward.FinalInvitingReward, "YouGetCaption");
			this.ShowReward(this._reward.FinalInvitedReward, "FriendGetsCaption");
		}
	}

	private void ShowReward(Reward reward, string key)
	{
		if (reward == null)
		{
			return;
		}
		GameObject gameObject = GUITools.AddChild(this.ContentRoot, this.GetReward);
		gameObject.transform.Find("Text").GetComponent<Text>().text = ScriptLocalization.Get(key);
		if (reward.Money1 != null && reward.Money1 > 0.0)
		{
			if (reward.Currency1 == "GC")
			{
				gameObject.transform.Find("Baitcoins").gameObject.SetActive(true);
				gameObject.transform.Find("AmmountBaitcoins").gameObject.SetActive(true);
				gameObject.transform.Find("AmmountBaitcoins").GetComponent<Text>().text = reward.Money1.Value.ToString();
			}
			if (reward.Currency1 == "SC")
			{
				gameObject.transform.Find("Money").gameObject.SetActive(true);
				gameObject.transform.Find("AmmountMoney").gameObject.SetActive(true);
				gameObject.transform.Find("AmmountMoney").GetComponent<Text>().text = reward.Money1.Value.ToString();
			}
		}
		if (reward.Money2 != null && reward.Money2 > 0.0)
		{
			if (reward.Currency2 == "GC")
			{
				gameObject.transform.Find("Baitcoins").gameObject.SetActive(true);
				gameObject.transform.Find("AmmountBaitcoins").gameObject.SetActive(true);
				gameObject.transform.Find("AmmountBaitcoins").GetComponent<Text>().text = reward.Money2.Value.ToString();
			}
			if (reward.Currency2 == "SC")
			{
				gameObject.transform.Find("Money").gameObject.SetActive(true);
				gameObject.transform.Find("AmmountMoney").gameObject.SetActive(true);
				gameObject.transform.Find("AmmountMoney").GetComponent<Text>().text = reward.Money2.Value.ToString();
			}
		}
		GUITools.AddChild(this.ContentRoot, this.Space).GetComponent<Text>().text = string.Empty;
		ProductReward[] products = reward.GetProductRewards();
		int num = 0;
		if (products != null)
		{
			int j;
			for (j = 0; j < products.Length; j++)
			{
				GUITools.AddChild(this.ContentRoot, this.Space).GetComponent<RectTransform>().sizeDelta = new Vector2(535f, 45f);
				StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == products[j].ProductId);
				GameObject gameObject2 = GUITools.AddChild(this.ContentRoot, this.GetItem);
				if (this.ldbls.Count <= num)
				{
					this.ldbls.Add(new ResourcesHelpers.AsyncLoadableImage());
				}
				this.ldbls[num].Image = gameObject2.transform.Find("Image").GetComponent<Image>();
				this.ldbls[num++].Load(string.Format("Textures/Inventory/{0}", storeProduct.ImageBID));
				gameObject2.transform.Find("Text").GetComponent<Text>().text = storeProduct.Name;
			}
		}
		ItemReward[] items = reward.GetItemRewards();
		if (items != null && this._items != null)
		{
			int i;
			for (i = 0; i < items.Length; i++)
			{
				InventoryItem inventoryItem = this._items.Find((InventoryItem tempItem) => tempItem.ItemId == items[i].ItemId);
				if (inventoryItem != null)
				{
					GUITools.AddChild(this.ContentRoot, this.Space).GetComponent<RectTransform>().sizeDelta = new Vector2(535f, 45f);
					GameObject gameObject3 = GUITools.AddChild(this.ContentRoot, this.GetItem);
					if (this.ldbls.Count <= num)
					{
						this.ldbls.Add(new ResourcesHelpers.AsyncLoadableImage());
					}
					this.ldbls[num].Image = gameObject3.transform.Find("Image").GetComponent<Image>();
					this.ldbls[num++].Load(string.Format("Textures/Inventory/{0}", inventoryItem.ThumbnailBID));
					gameObject3.transform.Find("Text").GetComponent<Text>().text = inventoryItem.Name;
				}
			}
		}
	}

	private void OnEnable()
	{
		this.AlphafadeComponent.FastHidePanel();
		this.AlphafadeComponent.ShowPanel();
		PhotonConnectionFactory.Instance.OnInviteIsUnique += this.Instance_OnInviteIsUnique;
		PhotonConnectionFactory.Instance.OnInviteDuplicated += this.Instance_OnInviteDuplicated;
		PhotonConnectionFactory.Instance.OnInviteCreated += this.Instance_OnInviteCreated;
		PhotonConnectionFactory.Instance.OnCreateInviteFailed += this.Instance_OnCreateInviteFailed;
	}

	private void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
		PhotonConnectionFactory.Instance.OnInviteIsUnique -= this.Instance_OnInviteIsUnique;
		PhotonConnectionFactory.Instance.OnInviteDuplicated -= this.Instance_OnInviteDuplicated;
		PhotonConnectionFactory.Instance.OnInviteCreated -= this.Instance_OnInviteCreated;
		PhotonConnectionFactory.Instance.OnCreateInviteFailed -= this.Instance_OnCreateInviteFailed;
	}

	public void Close()
	{
		this.AlphafadeComponent.HideFinished += this._alphaFade_HideFinished;
		this.AlphafadeComponent.HidePanel();
	}

	private void Instance_OnInviteDuplicated(Failure failure)
	{
		if (this.messageBox != null)
		{
			this._alphaFade_HideFinished(null, null);
		}
		this.messageBox = GUITools.AddChild(InfoMessageController.Instance.gameObject, MessageBoxList.Instance.messageBoxPrefab);
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.messageBox.GetComponent<MessageBox>().ConfirmButtonText = ScriptLocalization.Get("OkButton");
		this.messageBox.GetComponent<MessageBox>().Message = ScriptLocalization.Get("DuplicatedInvite");
		this.messageBox.GetComponent<MessageBox>().Caption = ScriptLocalization.Get("MessageCaption");
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CloseMessageBox;
	}

	private void Instance_OnInviteIsUnique()
	{
		PhotonConnectionFactory.Instance.CreateInvite(this.Email.text, this.FriendName.text);
	}

	private void Instance_OnCreateInviteFailed(Failure failure)
	{
		this.messageBox = GUITools.AddChild(InfoMessageController.Instance.gameObject, MessageBoxList.Instance.messageBoxPrefab);
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.messageBox.GetComponent<MessageBox>().ConfirmButtonText = ScriptLocalization.Get("OkButton");
		this.messageBox.GetComponent<MessageBox>().Message = ((failure.ErrorCode != 32566) ? ScriptLocalization.Get("InviteNotSent") : ScriptLocalization.Get("InviteLimitExceed"));
		this.messageBox.GetComponent<MessageBox>().Caption = ScriptLocalization.Get("MessageCaption");
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CloseMessageBox;
	}

	private void Instance_OnInviteCreated()
	{
		this.Close();
		this.messageBox = GUITools.AddChild(InfoMessageController.Instance.gameObject, this.InviteSent);
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.messageBox.GetComponent<MessageBox>().Message = string.Format(ScriptLocalization.Get("InviteSent"), this.FriendName.text);
		this.messageBox.GetComponent<EventAction>().ActionCalled += this.CloseMessageBox;
	}

	private void _alphaFade_HideFinished(object sender, EventArgsAlphaFade e)
	{
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
		Object.Destroy(base.gameObject);
	}

	public void EnterEmail()
	{
		if (AbusiveWords.HasAbusiveWords(this.FriendName.text) || string.IsNullOrEmpty(this.FriendName.text))
		{
			this.messageBox = GUITools.AddChild(InfoMessageController.Instance.gameObject, MessageBoxList.Instance.messageBoxPrefab);
			this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			this.messageBox.GetComponent<MessageBox>().ConfirmButtonText = ScriptLocalization.Get("OkButton");
			this.messageBox.GetComponent<MessageBox>().Message = ScriptLocalization.Get("IncorrectName");
			this.messageBox.GetComponent<MessageBox>().Caption = ScriptLocalization.Get("MessageCaption");
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CloseMessageBox;
			return;
		}
		if (!string.IsNullOrEmpty(this.Email.text) && InviteFriendMessage.rgxEmail.IsMatch(this.Email.text))
		{
			PhotonConnectionFactory.Instance.CheckInviteIsUnique(this.Email.text);
		}
		else
		{
			this.messageBox = GUITools.AddChild(InfoMessageController.Instance.gameObject, MessageBoxList.Instance.messageBoxPrefab);
			this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			this.messageBox.GetComponent<MessageBox>().ConfirmButtonText = ScriptLocalization.Get("OkButton");
			this.messageBox.GetComponent<MessageBox>().Message = ScriptLocalization.Get("EmailInvalidFormat");
			this.messageBox.GetComponent<MessageBox>().Caption = ScriptLocalization.Get("MessageCaption");
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CloseMessageBox;
		}
	}

	private void CloseMessageBox(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.GetComponent<MessageBox>().Close();
		}
	}

	public InputField Email;

	public InputField FriendName;

	public AlphaFade AlphafadeComponent;

	public GameObject InviteSent;

	public Text Caption1;

	public Text Caption2;

	public Text Description;

	public GameObject Space;

	public GameObject GetReward;

	public GameObject GetItem;

	public GameObject ContentRoot;

	private GameObject messageBox;

	private static string patternEmail = "\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z";

	protected static Regex rgxEmail = new Regex(InviteFriendMessage.patternEmail, RegexOptions.IgnoreCase);

	private ReferralReward _reward;

	private List<InventoryItem> _items;

	private List<ResourcesHelpers.AsyncLoadableImage> ldbls = new List<ResourcesHelpers.AsyncLoadableImage>();
}
