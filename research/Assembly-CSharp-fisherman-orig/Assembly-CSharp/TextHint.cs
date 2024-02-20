using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextHint : ManagedHintObject
{
	public string OriginalText { get; set; }

	protected override void Show()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		rectTransform.localScale = Vector3.zero;
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(rectTransform, Vector3.one, 0.35f), 27);
		ShortcutExtensions.DOFade(this.group, 1f, 0.25f);
		this.UpdateKeyOutlines();
	}

	public void HideKeyOutlines()
	{
		foreach (KeyValuePair<int, Image> keyValuePair in this.spawnedImages)
		{
			keyValuePair.Value.gameObject.SetActive(false);
		}
	}

	public void UpdateKeyOutlines()
	{
		if (this.Info.Count > 0)
		{
			foreach (KeyValuePair<int, int> keyValuePair in this.Info)
			{
				if (keyValuePair.Key < this.Text.textInfo.wordCount)
				{
					TMP_WordInfo tmp_WordInfo = this.Text.textInfo.wordInfo[keyValuePair.Key];
					if (!this.spawnedImages.ContainsKey(keyValuePair.Key))
					{
						this.spawnedImages.Add(keyValuePair.Key, Object.Instantiate<Image>(this.buttonOutlinePrefab, this.Text.transform));
					}
					Vector3 bottomLeft = this.Text.textInfo.characterInfo[tmp_WordInfo.firstCharacterIndex].bottomLeft;
					Vector3 topRight = this.Text.textInfo.characterInfo[Mathf.Max(0, tmp_WordInfo.lastCharacterIndex)].topRight;
					float scale = this.Text.textInfo.characterInfo[tmp_WordInfo.firstCharacterIndex].scale;
					float num = Mathf.Max(40f, (topRight.x - bottomLeft.x) / scale);
					float num2 = Mathf.Max(40f, (topRight.y - bottomLeft.y) / scale);
					if (tmp_WordInfo.firstCharacterIndex == tmp_WordInfo.lastCharacterIndex)
					{
						num = Mathf.Max(num, num2);
						num2 = num;
					}
					Vector3 vector = (bottomLeft + topRight) * 0.5f;
					this.spawnedImages[keyValuePair.Key].gameObject.SetActive(true);
					this.spawnedImages[keyValuePair.Key].rectTransform.sizeDelta = new Vector2(num, num2);
					this.spawnedImages[keyValuePair.Key].rectTransform.anchoredPosition = vector;
				}
			}
		}
	}

	protected override void Hide()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(rectTransform, Vector3.zero, 0.3f), 17);
		ShortcutExtensions.DOFade(this.group, 0f, 0.2f);
	}

	public Image Background;

	public TextMeshProUGUI Text;

	public int Index;

	public Dictionary<int, int> Info = new Dictionary<int, int>();

	public Image buttonOutlinePrefab;

	private Dictionary<int, Image> spawnedImages = new Dictionary<int, Image>();
}
