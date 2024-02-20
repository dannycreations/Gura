using System;
using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using ObjectModel.Common;
using UnityEngine;

public static class UIHelper
{
	public static bool IsMouse
	{
		get
		{
			return SettingsManager.InputType == InputModuleManager.InputType.Mouse;
		}
	}

	public static bool IsWaiting
	{
		get
		{
			return UIHelper._messageBox != null;
		}
	}

	public static Canvas GetTopmostCanvas(Component component)
	{
		Canvas[] componentsInParent = component.GetComponentsInParent<Canvas>();
		if (componentsInParent != null && componentsInParent.Length > 0)
		{
			return componentsInParent[componentsInParent.Length - 1];
		}
		return null;
	}

	public static UIHelper.FishTypes GetFishType(FishBrief f)
	{
		if (f.CodeName.EndsWith("Y"))
		{
			return UIHelper.FishTypes.Young;
		}
		if (f.CodeName.EndsWith("T"))
		{
			return UIHelper.FishTypes.Trophy;
		}
		if (f.CodeName.EndsWith("U"))
		{
			return UIHelper.FishTypes.Unique;
		}
		return UIHelper.FishTypes.Common;
	}

	public static string GetTypeNameByFishType(UIHelper.FishTypes type)
	{
		switch (type)
		{
		case UIHelper.FishTypes.Young:
			return ScriptLocalization.Get("YoungType");
		case UIHelper.FishTypes.Common:
			return ScriptLocalization.Get("CommonType");
		case UIHelper.FishTypes.Trophy:
			return ScriptLocalization.Get("TrophyType");
		case UIHelper.FishTypes.Unique:
			return ScriptLocalization.Get("UniqueType");
		default:
			return ScriptLocalization.Get("CommonType");
		}
	}

	public static string GetFishCodeName(FishBrief f, UIHelper.FishTypes ft)
	{
		if (ft == UIHelper.FishTypes.Common)
		{
			return f.CodeName;
		}
		return f.CodeName.Remove(f.CodeName.Length - 1);
	}

	public static string GetFishColor(FishBrief f)
	{
		if (f.CodeName.EndsWith("Y"))
		{
			return UIHelper.FishColor;
		}
		if (f.CodeName.EndsWith("T"))
		{
			return UIHelper.FishTrophyColor;
		}
		if (f.CodeName.EndsWith("U"))
		{
			return UIHelper.FishUniqueColor;
		}
		return UIHelper.FishColor;
	}

	public static string GetFishColor(Fish f, string fishColor = null)
	{
		if (f == null)
		{
			return UIHelper.FishColor;
		}
		if (f.IsEvent)
		{
			return UIHelper.FishEventColor;
		}
		if (f.IsTrophy != null && f.IsTrophy.Value)
		{
			return UIHelper.FishTrophyColor;
		}
		if (f.IsUnique != null && f.IsUnique.Value)
		{
			return UIHelper.FishUniqueColor;
		}
		return string.IsNullOrEmpty(fishColor) ? UIHelper.FishColor : fishColor;
	}

	public static void Waiting(bool flag, string text = null)
	{
		if (flag)
		{
			if (UIHelper._messageBox != null)
			{
				Debug.LogErrorFormat("Waiting 'true' but _messageBox != null", new object[0]);
				return;
			}
			ControlsController.ControlsActions.UICancel.Deactivate();
			if (string.IsNullOrEmpty(text))
			{
				text = LocalizationManager.GetTermTranslation("RunningStatusText");
			}
			MenuHelpers menuHelpers = new MenuHelpers();
			GameObject gameObject = InfoMessageController.Instance.gameObject;
			UIHelper._messageBox = GUITools.AddChild(gameObject, menuHelpers.MessageBoxList.Waiting).GetComponent<MessageBox>();
			RectTransform component = UIHelper._messageBox.GetComponent<RectTransform>();
			component.anchoredPosition = Vector3.zero;
			component.sizeDelta = Vector2.zero;
			UIHelper._messageBox.Message = text;
			UIHelper._messageBox.OpenFast();
		}
		else if (UIHelper._messageBox != null)
		{
			UIHelper._messageBox.HideFast();
			UIHelper._messageBox = null;
			ControlsController.ControlsActions.UICancel.Activate();
		}
	}

	public static void ShowProgress(bool flag, float duration, Action onFinish)
	{
		Debug.LogErrorFormat("UIHelper:ShowProgress {0}", new object[] { flag });
		if (flag)
		{
			if (UIHelper._progressMessage != null)
			{
				return;
			}
			GameObject gameObject = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.ProgressMessage);
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.anchoredPosition = Vector3.zero;
			component.sizeDelta = Vector2.zero;
			UIHelper._progressMessage = gameObject.GetComponent<ProgressMessageInit>();
			UIHelper._progressMessage.OnFinish += onFinish;
			UIHelper._progressMessage.Init(duration);
		}
		else if (UIHelper._progressMessage != null)
		{
			Debug.LogErrorFormat("UIHelper:Close", new object[0]);
			UIHelper._progressMessage.Close();
			UIHelper._progressMessage = null;
		}
	}

	public static bool IsShowHelpFirstTime
	{
		get
		{
			return !PlayerPrefs.HasKey("ShowHelpFirstTime");
		}
	}

	public static void ShowHelpFirstTime()
	{
		KeysHandlerAction.HelpHandler(null);
		PlayerPrefs.SetInt("ShowHelpFirstTime", 1);
	}

	public static void ParseAmount(Amount amount, ref int silver, ref int gold)
	{
		if (amount != null && amount.Value > 0)
		{
			if (amount.Currency == "SC")
			{
				silver = amount.Value;
			}
			else if (amount.Currency == "GC")
			{
				gold = amount.Value;
			}
		}
	}

	public static bool GetGoldAndSilverTexts(Reward reward, out string silverStr, out string goldStr)
	{
		if (reward == null)
		{
			reward = new Reward();
		}
		Amount amount = new Amount
		{
			Currency = reward.Currency1,
			Value = ((reward.Money1 == null) ? 0 : ((int)reward.Money1.Value))
		};
		Amount amount2 = new Amount
		{
			Currency = reward.Currency2,
			Value = ((reward.Money2 == null) ? 0 : ((int)reward.Money2.Value))
		};
		int num = 0;
		int num2 = 0;
		UIHelper.ParseAmount(amount, ref num2, ref num);
		UIHelper.ParseAmount(amount2, ref num2, ref num);
		silverStr = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon("SC"), num2);
		goldStr = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon("GC"), num);
		return num > 0;
	}

	public static bool IsPointInRt(Vector2 point, RectTransform rt)
	{
		Rect rect = rt.rect;
		float num = rt.anchoredPosition.x - rect.width / 2f;
		float num2 = rt.anchoredPosition.x + rect.width / 2f;
		float num3 = rt.anchoredPosition.y + rect.height / 2f;
		float num4 = rt.anchoredPosition.y - rect.height / 2f;
		return point.x >= num && point.x <= num2 && point.y >= num4 && point.y <= num3;
	}

	public static Sprite GetSpriteFromBytes(byte[] data, int w, int h, Vector2 pivot, float pixelsPerUnit = 100f)
	{
		Texture2D texture2D = new Texture2D(w, h);
		ImageConversion.LoadImage(texture2D, data);
		return Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), pivot, pixelsPerUnit);
	}

	public static void ShowMessage(string capt, string text, bool onPriority = true, Action actionCalled = null, bool forceShow = false)
	{
		MenuHelpers.Instance.ShowMessage(capt, text, onPriority, actionCalled, forceShow);
	}

	public static MessageBox ShowYesNo(string message, Action actionCalled = null, string capt = null, string confirmButtonTextLocId = "YesCaption", Action cancelCalled = null, string cancelButtonLocId = "NoCaption", GameObject prefab = null, Action afterHideAcceptAction = null, Action afterHideCancelAction = null)
	{
		return MenuHelpers.Instance.ShowYesNo((!string.IsNullOrEmpty(capt)) ? capt : ScriptLocalization.Get("MessageCaption"), message, actionCalled, confirmButtonTextLocId, cancelCalled, cancelButtonLocId, afterHideAcceptAction, afterHideCancelAction, prefab);
	}

	public static void ShowCanceledMsg(string title, string message, TournamentCanceledInit.MessageTypes type, Action okFunc = null, bool forceShow = false)
	{
		if (MessageBoxList.Instance.ServerMessagesHandler != null)
		{
			MessageBoxList.Instance.ServerMessagesHandler.ShowCanceledMsg(title, message, type, DateTime.MinValue, okFunc, forceShow);
		}
	}

	public static void ShowCanceledMsg(string title, string message, TournamentCanceledInit.MessageTypes type, DateTime endDate, Action okFunc = null, bool forceShow = false)
	{
		if (MessageBoxList.Instance.ServerMessagesHandler != null)
		{
			MessageBoxList.Instance.ServerMessagesHandler.ShowCanceledMsg(title, message, type, endDate, okFunc, forceShow);
		}
	}

	public static string FishTrophyColor = "008000";

	public static string FishUniqueColor = "ffa500";

	public static string FishEventColor = "9771F6";

	public static string FishColor = "808080";

	private const string ShowHelpFirstTimeTag = "ShowHelpFirstTime";

	private static MessageBox _messageBox;

	private static ProgressMessageInit _progressMessage;

	public enum FishTypes
	{
		Young,
		Common,
		Trophy,
		Unique
	}
}
