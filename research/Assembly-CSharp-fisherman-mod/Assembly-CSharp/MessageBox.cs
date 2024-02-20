using System;
using System.Globalization;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MessageBoxBase
{
	protected override void Awake()
	{
		base.Awake();
		this.Caption = string.Empty;
		if (this.confirmButtonText != null)
		{
			this.confirmButtonText.text = ScriptLocalization.Get("OkButton").ToUpper(CultureInfo.InvariantCulture);
		}
		if (this.cancelButtonText != null)
		{
			this.cancelButtonText.text = ScriptLocalization.Get("CancelButton").ToUpper(CultureInfo.InvariantCulture);
		}
		this._alphaFade.FastHidePanel();
		base.gameObject.SetActive(false);
		MessageFactory.MessageBoxQueue.Enqueue(this);
	}

	public void Init(string msgText, string confirmText, string cancelText, bool isShow)
	{
		this.Message = msgText;
		this.ConfirmButtonText = confirmText;
		this.CancelButtonText = cancelText;
		if (isShow)
		{
			base.Open();
		}
	}

	public string Caption
	{
		set
		{
			if (this.caption != null)
			{
				this.caption.text = value;
			}
		}
	}

	public string Message
	{
		set
		{
			this.text.text = value;
		}
	}

	public string ConfirmButtonText
	{
		set
		{
			this.confirmButtonText.text = value.ToUpper();
		}
	}

	public string CancelButtonText
	{
		set
		{
			this.cancelButtonText.text = value.ToUpper();
		}
	}

	public string ThirdButtonText
	{
		set
		{
			this.thirdButtonText.text = value.ToUpper();
		}
	}

	public void DisableConfirmButton()
	{
		this.confirmButtonText.transform.parent.gameObject.GetComponent<Button>().interactable = false;
	}

	public Text caption;

	public Text text;

	public Text confirmButtonText;

	public Text cancelButtonText;

	public Text thirdButtonText;

	public bool SetActiveCancelButton;

	public GameObject confirmButton;

	public GameObject declineButton;
}
