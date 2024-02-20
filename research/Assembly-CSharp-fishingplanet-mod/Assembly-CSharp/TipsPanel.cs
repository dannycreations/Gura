using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I2.Loc;
using UnityEngine;

public class TipsPanel : ActivityStateControlled
{
	protected override void HideHelp()
	{
		ScreenManager.Instance.SetTransfer(false);
	}

	protected override void SetHelp()
	{
		ScreenManager.Instance.SetTransfer(true);
		this.RefreshTip();
	}

	private void RefreshTip()
	{
		if (this._lastTip != null)
		{
			Object.Destroy(this._lastTip);
		}
		if (!SettingsManager.ShowTips)
		{
			return;
		}
		string text = LocalizationManager.CurrentLanguage.Replace(" ", string.Empty);
		TextAsset textAsset = Resources.Load("Tips/" + text) as TextAsset;
		if (textAsset == null)
		{
			return;
		}
		try
		{
			List<string> list = textAsset.text.Split(new char[] { '\n' }).ToList<string>();
			int count = list.Count;
			Random random = new Random();
			int num = random.Next(1, count);
			string text2 = list[num];
			if (!string.IsNullOrEmpty(text2))
			{
				this._lastTip = GUITools.AddChild(base.gameObject, this.tipPrefab);
				this._lastTip.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
				this._lastTip.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
				this._lastTip.GetComponent<TipInit>().Init(text2);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
		}
	}

	private string GetLine(string fileName, int line)
	{
		string text;
		using (StreamReader streamReader = new StreamReader(fileName))
		{
			for (int i = 1; i < line; i++)
			{
				streamReader.ReadLine();
			}
			text = streamReader.ReadLine();
		}
		return text;
	}

	public GameObject tipPrefab;

	private GameObject _lastTip;
}
