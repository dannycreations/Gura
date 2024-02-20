using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComingSoon : MonoBehaviour
{
	public void Add()
	{
		InfoMessageController.Instance.StartCoroutine(this.AddInOneFrame());
	}

	private IEnumerator AddInOneFrame()
	{
		yield return new WaitForEndOfFrame();
		if (this.Text is Text)
		{
			Text text = this.Text as Text;
			text.text += this.ComingSoonFormat;
		}
		else if (this.Text is TextMeshProUGUI)
		{
			TextMeshProUGUI textMeshProUGUI = this.Text as TextMeshProUGUI;
			textMeshProUGUI.text += this.ComingSoonFormat;
		}
		yield break;
	}

	public string ComingSoonFormat = " [COMING SOON]";

	[SerializeField]
	private Graphic Text;
}
