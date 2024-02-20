using System;
using System.Collections.Generic;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSlideReferences : MonoBehaviour
{
	public void InitSlide(TutorialSlide slide, bool initText = true)
	{
		this.SetImageSprite(this._primaryImage, slide.PrimaryImage.path, slide.PrimaryImage.localized, slide.PrimaryImage.platformDependent);
		this.SetImageSprite(this._secondaryImage, slide.SecondaryImage.path, slide.SecondaryImage.localized, slide.SecondaryImage.platformDependent);
		if (initText)
		{
			this.SetText(this._primaryText, slide.PrimaryText.text, slide.PrimaryText.inputType, slide.PrimaryText.KeyInputType, slide.PrimaryText.MouseInputType);
			this.SetText(this._secondaryText, slide.SecondaryText.text, slide.SecondaryText.inputType, slide.SecondaryText.KeyInputType, slide.SecondaryText.MouseInputType);
		}
		this.SetPositionAndSize(this._primaryImage.GetComponent<RectTransform>(), slide.PrimaryImage.position, slide.PrimaryImage.size);
		this.SetPositionAndSize(this._secondaryImage.GetComponent<RectTransform>(), slide.SecondaryImage.position, slide.SecondaryImage.size);
		this.SetPositionAndSize(this._primaryText.GetComponent<RectTransform>(), slide.PrimaryText.position, slide.PrimaryText.size);
		this.SetPositionAndSize(this._secondaryText.GetComponent<RectTransform>(), slide.SecondaryText.position, slide.SecondaryText.size);
	}

	private void SetImageSprite(Image image, string pathToSprite, bool localized, bool platformDependent)
	{
		if (!string.IsNullOrEmpty(pathToSprite))
		{
			string text = pathToSprite;
			if (localized)
			{
				int num = pathToSprite.IndexOf('_') + 1;
				pathToSprite = pathToSprite.Insert((num >= 0) ? num : 0, TutorialSlideReferences.LocalizationAdditions[ChangeLanguage.GetCurrentLanguage.Id]);
			}
			if (platformDependent)
			{
				pathToSprite += "_pc";
			}
			if (!TutorialSlidesController.TutorialSlidesBundle.Contains(pathToSprite))
			{
				pathToSprite = text;
				if (localized)
				{
					int num2 = pathToSprite.IndexOf('_') + 1;
					pathToSprite = pathToSprite.Insert((num2 >= 0) ? num2 : 0, TutorialSlideReferences.LocalizationAdditionsFallback[ChangeLanguage.GetCurrentLanguage.Id]);
				}
				if (platformDependent)
				{
					pathToSprite += "_pc";
				}
			}
			base.StartCoroutine(AssetBundleManager.LoadFromExisting(TutorialSlidesController.TutorialSlidesBundle, pathToSprite, delegate(Object ret)
			{
				image.sprite = ret as Sprite;
			}));
		}
	}

	private void SetText(Text text, string key, InputControlType[] buttons, Key[] keys, Mouse[] mouse)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < keys.Length; i++)
		{
			list.Add(keys[i].ToString());
		}
		for (int j = 0; j < mouse.Length; j++)
		{
			list.Add(ScriptLocalization.Get(HotkeyIcons.MouseMappings[mouse[j]]));
		}
		if (!string.IsNullOrEmpty(key))
		{
			text.text = string.Format(ScriptLocalization.Get(key).Replace("<br>", "\n"), list.ToArray());
		}
	}

	private void SetPositionAndSize(RectTransform rt, Vector2 position, Vector2 size)
	{
		if (position != Vector2.zero)
		{
			rt.anchoredPosition = position;
		}
		if (size != Vector2.zero)
		{
			rt.sizeDelta = size;
		}
	}

	[SerializeField]
	private Image _primaryImage;

	[SerializeField]
	private Image _secondaryImage;

	[SerializeField]
	private Text _primaryText;

	[SerializeField]
	private Text _secondaryText;

	public static Dictionary<int, string> LocalizationAdditions = new Dictionary<int, string>
	{
		{ 3, "1_EnM_" },
		{ 1, "2_EnE_" },
		{ 2, "3_Ru_" },
		{ 4, "4_Ge_" },
		{ 5, "5_Fr_" },
		{ 6, "6_Pl_" },
		{ 7, "7_Ua_" },
		{ 9, "8_Es_" },
		{ 10, "9_Port_" },
		{ 11, "10_Ne_" },
		{ 12, "11_Chi_" },
		{ 13, "11_Chi_" },
		{ 14, "13_Ita_" }
	};

	public static Dictionary<int, string> LocalizationAdditionsFallback = new Dictionary<int, string>
	{
		{ 3, "1_EnM_" },
		{ 1, "2_EnE_" },
		{ 2, "3_Ru_" },
		{ 4, "4_Ge_" },
		{ 5, "5_Fr_" },
		{ 6, "6_Pl_" },
		{ 7, "7_Ua_" },
		{ 8, "1_EnM_" },
		{ 9, "8_Es_" },
		{ 10, "9_Port_" },
		{ 11, "10_Ne_" },
		{ 12, "1_EnM_" },
		{ 13, "1_EnM_" },
		{ 14, "1_EnM_" }
	};
}
