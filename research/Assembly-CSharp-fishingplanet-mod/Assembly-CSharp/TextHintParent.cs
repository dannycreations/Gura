using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class TextHintParent : MonoBehaviour
{
	private void Awake()
	{
		PhotonConnectionFactory.Instance.GameScreenChanged += this.GameScreenChanged;
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		ControlsController.OnBindingsChanged += this.ControlsController_OnBindingsChanged;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.GameScreenChanged -= this.GameScreenChanged;
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		ControlsController.OnBindingsChanged -= this.ControlsController_OnBindingsChanged;
	}

	private bool isInventoryOthersParent
	{
		get
		{
			return CatchedFishInfoHandler.IsDisplayed() && CatchedFishInfoHandler.Instance.IsKeepnet;
		}
	}

	private bool isCaughtPanel
	{
		get
		{
			return CatchedFishInfoHandler.IsDisplayed() && !CatchedFishInfoHandler.Instance.IsKeepnet && (GameFactory.Message == null || !GameFactory.Message.IsDisplaying);
		}
	}

	private void ControlsController_OnBindingsChanged()
	{
		this.UpdateTextHint();
	}

	private void OnInputTypeChanged(InputModuleManager.InputType inputType)
	{
		this.UpdateTextHint();
	}

	private void GameScreenChanged(GameScreenType type, GameScreenTabType tab)
	{
		this._currentParent = this.GetParent(type);
		this._currentPivotY = ((!(this._currentParent == this.InventoryOthersParent)) ? 1f : 0f);
		foreach (TextHint textHint in this.textHints)
		{
			Vector2 pivot = (textHint.transform as RectTransform).pivot;
			pivot.y = this._currentPivotY;
			(textHint.transform as RectTransform).pivot = pivot;
			(textHint.transform as RectTransform).SetParent(this._currentParent);
			(textHint.transform as RectTransform).anchoredPosition = Vector2.zero;
			(textHint.transform as RectTransform).localScale = Vector3.one;
		}
	}

	public void AddTextHint(TextHint hint)
	{
		this._currentParent = this.GetParent(HintSystem.Instance.ScreenType);
		this._currentPivotY = ((!(this._currentParent == this.InventoryOthersParent)) ? 1f : 0f);
		this.textHints.Add(hint);
		Vector2 pivot = (hint.transform as RectTransform).pivot;
		pivot.y = this._currentPivotY;
		(hint.transform as RectTransform).pivot = pivot;
		(hint.transform as RectTransform).SetParent(this._currentParent);
		(hint.transform as RectTransform).anchoredPosition = Vector2.zero;
		this.SortTextHints();
	}

	public void RmTextHint(TextHint hint)
	{
		this.textHints.Remove(hint);
		if (base.gameObject.activeInHierarchy)
		{
			base.StartCoroutine(this.WaitAndSort());
		}
	}

	public Transform GetParent()
	{
		return this.GetParent(HintSystem.Instance.ScreenType);
	}

	public RectTransform GetParent(GameScreenType type)
	{
		switch (type)
		{
		case GameScreenType.GlobalMap:
			return this.GlobalMapParent;
		default:
			if (type != GameScreenType.Missions)
			{
				return this.InventoryOthersParent;
			}
			if (HintSystem.Instance != null && HintSystem.Instance.activeHints != null)
			{
				if (HintSystem.Instance.activeHints.Any((ManagedHint x) => x.Message.MissionId == 54))
				{
					return this.InventoryOthersParent;
				}
			}
			return this.ZeroScaleParent;
		case GameScreenType.LocalMap:
		case GameScreenType.Game:
		case GameScreenType.Map:
		case GameScreenType.Time:
			if (this.isCaughtPanel)
			{
				return this.GameCaughtPanelParent;
			}
			if (this.isInventoryOthersParent)
			{
				return this.InventoryOthersParent;
			}
			return this.GameLocalMapParent;
		}
	}

	private IEnumerator WaitAndSort()
	{
		yield return new WaitForEndOfFrame();
		this.SortTextHints();
		yield break;
	}

	private void Update()
	{
		if (this.textHints.Count > 0)
		{
			if (!this.textHints.Any((TextHint x) => x.gameObject.activeInHierarchy))
			{
				this.SortTextHints();
			}
		}
		if ((CatchedFishInfoHandler.IsDisplayed() && !CatchedFishInfoHandler.Instance.IsKeepnet && this._currentParent != this.GameCaughtPanelParent) || (this._currentParent == this.GameCaughtPanelParent && (!CatchedFishInfoHandler.IsDisplayed() || (CatchedFishInfoHandler.IsDisplayed() && CatchedFishInfoHandler.Instance.IsKeepnet))))
		{
			this.Refresh();
		}
	}

	public void Refresh()
	{
		this.GameScreenChanged(HintSystem.Instance.ScreenType, GameScreenTabType.All);
	}

	private void SortTextHints()
	{
		this.textHints.Sort((TextHint a, TextHint b) => a.Index.CompareTo(b.Index));
		bool flag = false;
		for (int i = 0; i < this.textHints.Count; i++)
		{
			bool flag2 = this.textHints[i].Displayed && !flag;
			this.textHints[i].gameObject.SetActive(flag2);
			if (this.textHints[i].gameObject.activeInHierarchy && this.textHints[i].Info.Count > 0)
			{
				this.textHints[i].UpdateKeyOutlines();
			}
			if (flag2)
			{
				flag = true;
			}
		}
		this.sortAssigned = false;
	}

	private void UpdateTextHint()
	{
		List<TextHint> list = this.textHints.FindAll((TextHint p) => !string.IsNullOrEmpty(p.OriginalText));
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			TextHint textHint = list[i];
			textHint.HideKeyOutlines();
			string text = textHint.OriginalText;
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			string text2;
			while (TextHintParent.SetIcoByActionName(text, out text2, dictionary, false))
			{
				text = text2;
			}
			textHint.Text.text = text2;
			textHint.Info = dictionary;
			if (dictionary.Count > 0)
			{
				flag = true;
			}
		}
		if (flag)
		{
			base.StartCoroutine(this.WaitAndSort());
		}
	}

	public static bool TryReplaceControlsInText(string inText, out string text, Dictionary<int, int> lengths, bool tooltip = false)
	{
		text = inText;
		bool flag = false;
		string text2;
		while (TextHintParent.SetIcoByActionName(text, out text2, lengths, tooltip))
		{
			text = text2;
			flag = true;
		}
		return flag;
	}

	public static bool SetIcoByActionName(string text, out string textResult, Dictionary<int, int> lengths, bool tooltip = false)
	{
		textResult = text;
		Debug.LogWarning(textResult);
		int num = 0;
		int num2 = 0;
		for (int i = num; i < text.Length; i++)
		{
			if (TextHintParent.WordBreaks.Contains(text[i]))
			{
				int num3 = 0;
				for (int j = i + 1; j < text.Length; j++)
				{
					char c = text[j];
					if (TextHintParent.WordBreaks.Contains(c))
					{
						break;
					}
					if (c == '<')
					{
						num3++;
					}
					if (c == '>')
					{
						num3--;
					}
					UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
					if (num3 == 0 && (char.IsDigit(c) || char.IsLetter(c) || TextHintParent.SymbolsInUse.Contains(c) || unicodeCategory == UnicodeCategory.OtherLetter))
					{
						num2++;
						break;
					}
				}
			}
			if (text[i] == '$')
			{
				if (i != 0 && num2 == 0)
				{
					num2 = 1;
				}
				for (int k = i + 1; k < text.Length; k++)
				{
					if (text[k] == '$')
					{
						string text2 = text.Substring(i, 1 + k - i);
						string text3 = text2.Remove(0, 1);
						text3 = text3.Remove(text3.Length - 1, 1);
						bool flag;
						string icoByActionName = HotkeyIcons.GetIcoByActionName(text3, out flag);
						string text4 = text3;
						int num4 = ((!flag) ? 12 : 2);
						int num5 = ((!flag) ? 0 : 1);
						string text5 = ((!flag) ? "-0.15" : "0.0");
						string text6 = ((!tooltip) ? "#FFEE44" : "#000000");
						string text7 = ((!(icoByActionName == "\ue718")) ? string.Empty : "OPTIONS");
						if (!string.IsNullOrEmpty(icoByActionName))
						{
							text4 = string.Format(" <color={0}><voffset={1}em><size=+{2}><space={3}em>{4}<space={5}em></size></voffset>{6}</color> ", new object[] { text6, text5, num4, num5, icoByActionName, num5, text7 });
						}
						textResult = textResult.Replace(text2, text4);
						Debug.LogWarning(string.Format("WordCount: {0}, word = {1}, ico ={2}", num2, icoByActionName, text4));
						if (flag)
						{
							if (!lengths.ContainsKey(num2))
							{
								lengths.Add(num2, icoByActionName.Length);
							}
							else
							{
								lengths[num2] = icoByActionName.Length;
							}
						}
						return true;
					}
				}
			}
		}
		return false;
	}

	public RectTransform GlobalMapParent;

	public RectTransform GameLocalMapParent;

	public RectTransform GameCaughtPanelParent;

	public RectTransform InventoryOthersParent;

	public RectTransform ZeroScaleParent;

	private RectTransform _currentParent;

	private float _currentPivotY;

	private List<TextHint> textHints = new List<TextHint>();

	private bool sortAssigned;

	private static readonly List<char> SymbolsInUse = new List<char>
	{
		'#', '-', ':', ';', '.', '/', '!', '?', '%', '*',
		'(', ')', '"', '！'
	};

	private static readonly List<char> WordBreaks = new List<char> { ' ', '，', '：', '！', '。' };
}
