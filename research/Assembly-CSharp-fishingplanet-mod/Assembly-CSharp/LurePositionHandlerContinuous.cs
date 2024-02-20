using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class LurePositionHandlerContinuous : MonoBehaviour
{
	private void Start()
	{
		this.iconRect = this.LureIcon.GetComponent<RectTransform>();
		this.iconImage = this.LureIcon.GetComponent<Image>();
		this.position = 1f;
		this.iconImage.color = new Color(this.iconImage.color.r, this.iconImage.color.g, this.iconImage.color.b, 1f);
	}

	public void Refresh(float position, float rotation)
	{
		this.position = Mathf.Lerp(position, this.position, Mathf.Exp(-Time.deltaTime * this.PositionFilter));
		this.rotation = Mathf.Lerp(rotation, this.rotation, Mathf.Exp(-Time.deltaTime * this.RotationFilter));
	}

	public void ShowDragStyleText(string dragStyle, float precision)
	{
		this.Retrieve1.SetActive(false);
		this.Retrieve2.SetActive(false);
		this.Retrieve3.SetActive(false);
		string text;
		switch (dragStyle)
		{
		case "Straight Slow":
			text = ScriptLocalization.Get("DragStyleStrightSlow");
			goto IL_13B;
		case "Straight":
			text = ScriptLocalization.Get("DragStyleStright");
			goto IL_13B;
		case "Lift&Drop":
			text = ScriptLocalization.Get("DragStyleLiftDrop");
			goto IL_13B;
		case "Stop&Go":
			text = ScriptLocalization.Get("DragStyleStopGo");
			goto IL_13B;
		case "Twitching":
			text = ScriptLocalization.Get("DragStyleTwitching");
			goto IL_13B;
		case "Walking":
			text = dragStyle;
			goto IL_13B;
		case "Popping":
			text = dragStyle;
			goto IL_13B;
		}
		text = string.Empty;
		IL_13B:
		this.DragStyleText.text = text;
		if (!string.IsNullOrEmpty(dragStyle))
		{
			if (precision >= 0f && precision <= 0.33f)
			{
				this.Retrieve1.SetActive(true);
			}
			if (precision > 0.33f && precision <= 0.66f)
			{
				this.Retrieve2.SetActive(true);
			}
			if (precision > 0.66f && precision <= 1f)
			{
				this.Retrieve3.SetActive(true);
			}
		}
	}

	internal void Update()
	{
		this.LureIcon.SetActive(this.position > -0.01f);
		this.iconRect.transform.localPosition = new Vector3(this.iconRect.transform.localPosition.x, this.IconBottomPosition + this.position * (this.IconTopPosition - this.IconBottomPosition), this.iconRect.transform.localPosition.z);
		Vector3 eulerAngles = this.iconRect.transform.rotation.eulerAngles;
		this.iconRect.transform.rotation = Quaternion.Euler(new Vector3(eulerAngles.x, eulerAngles.y, this.rotation * 57.29578f));
	}

	public GameObject LureIcon;

	public Text DragStyleText;

	public GameObject Retrieve1;

	public GameObject Retrieve2;

	public GameObject Retrieve3;

	public float IconBottomPosition;

	public float IconTopPosition;

	public float PositionFilter = 1f;

	public float RotationFilter = 1f;

	private RectTransform iconRect;

	private Image iconImage;

	private float position;

	private float rotation;
}
