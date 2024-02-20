using System;
using System.Diagnostics;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HudMissionTask : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> OnHide = delegate
	{
	};

	public int TaskId { get; private set; }

	public int OrderId { get; private set; }

	public float Y
	{
		get
		{
			return this._rt.anchoredPosition.y;
		}
	}

	public float H
	{
		get
		{
			return this._rt.rect.height;
		}
	}

	public bool IsDone { get; private set; }

	private void OnDestroy()
	{
		this._done.FastHidePanel();
		this._current.FastHidePanel();
	}

	public void Init(int taskId, int orderId, string taskName, string count, string progress, float prevY, float prevH, int index)
	{
		this._prevY = prevY;
		this._prevH = prevH;
		if ((float)taskName.Length > this._textLenForResizeHeight)
		{
			if ((float)taskName.Length - this._textLenForResizeHeight <= 7f)
			{
				int num = taskName.LastIndexOf(' ') + 1;
				taskName = string.Format("{0} \n{1}", taskName.Substring(0, num), taskName.Substring(num));
			}
			this.UpHeightSize(this._taskHeight);
		}
		this._done.FastHidePanel();
		this.TaskId = taskId;
		this.OrderId = orderId;
		TMP_Text taskNameDone = this._taskNameDone;
		string text = taskName;
		this._taskName.text = text;
		taskNameDone.text = text;
		this.UpdateProgress(progress, count);
		base.gameObject.SetActive(true);
		float y = HudMissionTask.GetY(this._prevY, this._prevH, index);
		this._rt.anchoredPosition = new Vector2(this._rt.anchoredPosition.x, y);
		this._current.FastShowPanel();
	}

	public static float GetY(float prevY, float prevH, int index)
	{
		float num = prevY - prevH - 12f;
		if (index == 0)
		{
			num += 20f;
		}
		return num;
	}

	public void UpdateProgress(string progress, string count)
	{
		bool flag = count.Trim(new char[] { '0', '.', ',', ' ' }) == string.Empty;
		if ((flag && this._icoImage.gameObject.activeSelf) || (!flag && !this._icoImage.gameObject.activeSelf))
		{
			this._taskProgress.gameObject.SetActive(!flag);
			this._taskProgressDone.gameObject.SetActive(!flag);
			this._icoImage.gameObject.SetActive(!flag);
			this._icoImageNotProgress.gameObject.SetActive(flag);
		}
		if (!flag)
		{
			this._taskProgress.text = string.Format("<b><color=#FFDD77FF>{0}</color></b> <size=16>/{1}</size>", progress, count);
			this._taskProgressDone.text = string.Format("<b>{0}</b> /{1}", progress, count);
		}
	}

	public void Done()
	{
		this.IsDone = true;
		this._current.HideFinished += this._current_HideFinished;
		this._current.HidePanel();
	}

	public void MoveUp(float directionY)
	{
		ShortcutExtensions.DOAnchorPos(this._rt, new Vector2(this._rt.anchoredPosition.x, directionY), 0.1f, false);
	}

	public void MoveLeft()
	{
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOAnchorPos(this._rt, new Vector2(-455f, this._rt.anchoredPosition.y), 0.4f, false), delegate
		{
			this.OnHide(this.TaskId);
		});
	}

	private void UpHeightSize(float heightOffset)
	{
		RectTransform rectTransform = this._taskNameDone.GetComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height + heightOffset);
		rectTransform = this._taskName.GetComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height + heightOffset);
		this._rt.sizeDelta = new Vector2(this._rt.rect.width, this._rt.rect.height + heightOffset);
	}

	private void _current_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this._current.HideFinished -= this._current_HideFinished;
		this._done.ShowPanel();
	}

	[SerializeField]
	private RectTransform _rt;

	[SerializeField]
	private float _textLenForResizeHeight = 36f;

	[SerializeField]
	private float _taskHeight = 27.04f;

	[SerializeField]
	private TextMeshProUGUI _taskName;

	[SerializeField]
	private TextMeshProUGUI _taskProgress;

	[SerializeField]
	private TextMeshProUGUI _icoImage;

	[SerializeField]
	private TextMeshProUGUI _icoImageNotProgress;

	[SerializeField]
	private TextMeshProUGUI _taskNameDone;

	[SerializeField]
	private TextMeshProUGUI _taskProgressDone;

	[SerializeField]
	private AlphaFade _current;

	[SerializeField]
	private AlphaFade _done;

	public const float Height = 21f;

	private const float DistBetweenTasks = 12f;

	private const float FirstTaskOffset = 20f;

	private const float LeftMoveSpeed = 0.4f;

	private const float UpMoveSpeed = 0.1f;

	private const float LeftMovePosX = -455f;

	private float _prevY;

	private float _prevH;
}
