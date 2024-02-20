using System;
using System.Collections;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class QuestStepItemHandler : MonoBehaviour
{
	public int TaskId { get; private set; }

	private void OnDestroy()
	{
		if (this._imGlow != null)
		{
			Object.Destroy(this._imGlow);
		}
		base.StopCoroutine(this.StopGlow());
	}

	public void Init(int taskId, string description, string progress, bool isFirst, bool isDone, bool isHidden)
	{
		Color color;
		ColorUtility.TryParseHtmlString((!isDone) ? "#F7F7F7FF" : "#F7F7F764", ref color);
		this._imageRuller.SetActive(!isFirst);
		this.TaskId = taskId;
		Text text = this._stepDescription;
		float num = 41f;
		this._progress.SetActive(!isDone);
		if (this._progress.activeSelf)
		{
			string text2;
			string text3;
			ClientMissionsManager.ParseTaskProgress(progress, out text2, out text3);
			this._stepProgress.text = (string.IsNullOrEmpty(text2) ? string.Empty : text2);
			this._stepCount.text = (string.IsNullOrEmpty(text3) ? string.Empty : string.Format("/{0}", text3));
			if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text3))
			{
				text = this._stepDescriptionShort;
				num = 30f;
			}
		}
		text.text = description;
		text.color = color;
		text.gameObject.SetActive(true);
		this.DoneLabel.SetActive(isDone);
		this.CurrentLabel.SetActive(!isDone && isHidden);
		this.ActiveLabel.SetActive(!isDone && !isHidden);
		this.ResizeTextDescription(text, num);
	}

	public void ActiveGlow()
	{
	}

	private IEnumerator StopGlow()
	{
		yield return new WaitForSeconds(8f);
		if (this._imGlow != null)
		{
			Object.Destroy(this._imGlow);
		}
		yield break;
	}

	private void ResizeTextDescription(Text t, float symbCountResize)
	{
		RectTransform component = base.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.rect.width, t.preferredHeight);
	}

	[SerializeField]
	private GameObject _imageGlowPrefab;

	[SerializeField]
	private GameObject _imageRuller;

	[SerializeField]
	private Text _stepProgress;

	[SerializeField]
	private Text _stepCount;

	[SerializeField]
	private Text _stepDescription;

	[SerializeField]
	private Text _stepDescriptionShort;

	[SerializeField]
	private GameObject _progress;

	public GameObject DoneLabel;

	public GameObject CurrentLabel;

	public GameObject ActiveLabel;

	private GameObject _imGlow;

	private const float PulsationTime = 8f;

	private const float TextSymbCountResizeShort = 30f;

	private const float TextSymbCountResize = 41f;

	private const float TextResizeK = 28f;
}
