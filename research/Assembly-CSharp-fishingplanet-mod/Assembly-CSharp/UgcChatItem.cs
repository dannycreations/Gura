using System;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UgcChatItem : MonoBehaviour
{
	private void Update()
	{
		if (this._rtMessages.rect.height > this._le.preferredHeight)
		{
			this.SetHeight();
		}
	}

	public bool Init(ChatMessage msg, string text)
	{
		this._message.text = text;
		this._uName.text = msg.Sender.UserName;
		if (msg.Timestamp != null)
		{
			this._time.text = MeasuringSystemManager.LongTimeString(msg.Timestamp.Value.ToLocalTime());
		}
		if (msg.Sender.UserId == PhotonConnectionFactory.Instance.Profile.UserId.ToString())
		{
			this._hLayout.childAlignment = 8;
			this._bg.color = this.OwnerBg;
		}
		this.SetHeight();
		return true;
	}

	public void Remove()
	{
		Object.Destroy(base.gameObject);
	}

	public void SetAsReaded()
	{
		this._message.color = new Color(this._message.color.r, this._message.color.g, this._message.color.b, 0.3f);
		this._uName.color = new Color(this._uName.color.r, this._uName.color.g, this._uName.color.b, 0.3f);
		this._time.color = new Color(this._time.color.r, this._time.color.g, this._time.color.b, 0.3f);
		this._bg.color = new Color(this._bg.color.r, this._bg.color.g, this._bg.color.b, 0.6f);
	}

	private void SetHeight()
	{
		this._le.preferredHeight = Mathf.Max(this._rtMessages.rect.height, 111.17f);
		this._rt.sizeDelta = new Vector2(this._rt.rect.width, this._le.preferredHeight);
		this._bgRt.sizeDelta = new Vector2(this._bgRt.rect.width, this._le.preferredHeight);
	}

	[SerializeField]
	private TextMeshProUGUI _uName;

	[SerializeField]
	private TextMeshProUGUI _time;

	[SerializeField]
	private TextMeshProUGUI _message;

	[SerializeField]
	private LayoutElement _le;

	[SerializeField]
	private RectTransform _rt;

	[SerializeField]
	private RectTransform _rtMessages;

	[SerializeField]
	private HorizontalLayoutGroup _hLayout;

	[SerializeField]
	private Image _bg;

	[SerializeField]
	private RectTransform _bgRt;

	private readonly Color OwnerBg = new Color(0.14509805f, 0.19215687f, 0.24313726f);

	private const float ReadedAlpha = 0.3f;

	private const float ReadedBgAlpha = 0.6f;

	private const float MinHeight = 111.17f;
}
