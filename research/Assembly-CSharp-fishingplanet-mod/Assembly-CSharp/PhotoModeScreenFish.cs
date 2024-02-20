using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PhotoModeScreenFish : PhotoModeScreen
{
	protected override void Awake()
	{
		base.Awake();
		this._size = this._labelRoot.GetComponentInParent<CanvasScaler>().referenceResolution;
		this._actions = new CustomPlayerAction[] { ControlsController.ControlsActions.PHMSwitchFishDescriptionVisibility };
		this._axisActions = new CustomPlayerTwoAxisAction[]
		{
			ControlsController.ControlsActions.PHMLabelOffsetUI,
			ControlsController.ControlsActions.PHMMoveFishDescription
		};
		Fish caughtFish = CatchedFishInfoHandler.CaughtFish;
		string fishColor = UIHelper.GetFishColor(caughtFish, "ffffff");
		this._fishDescription.text = string.Format("<color=#{0}>{1}</color>", fishColor, caughtFish.Name);
		this._lengthValue.text = string.Format("{0} {1}", MeasuringSystemManager.FishLength(caughtFish.Length).ToString("N3"), MeasuringSystemManager.FishLengthSufix());
		this._weightValue.text = string.Format("{0} {1}", MeasuringSystemManager.FishWeight(caughtFish.Weight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
	}

	private void Start()
	{
		this._labelRoot.gameObject.SetActive(true);
	}

	private void Update()
	{
		if (this._alphaFade.IsShow && ControlsController.ControlsActions.PHMSwitchFishDescriptionVisibility.WasReleased)
		{
			this._labelRoot.gameObject.SetActive(!this._labelRoot.gameObject.activeInHierarchy);
		}
		if (Input.GetMouseButton(1))
		{
			float num = ControlsController.ControlsActions.PHMMoveFishDescription.X * this._mouseSensitivity;
			float num2 = ControlsController.ControlsActions.PHMMoveFishDescription.Y * this._mouseSensitivity;
			this._offset.x = Mathf.Clamp01(this._offset.x + num);
			this._offset.y = Mathf.Clamp01(this._offset.y - num2);
		}
		else
		{
			CustomPlayerTwoAxisAction phmlabelOffsetUI = ControlsController.ControlsActions.PHMLabelOffsetUI;
			this._offset.x = Mathf.Clamp01(this._offset.x + phmlabelOffsetUI.X * this._offsetSpeed * Time.deltaTime);
			this._offset.y = Mathf.Clamp01(this._offset.y - phmlabelOffsetUI.Y * this._offsetSpeed * Time.deltaTime);
		}
		float num3 = (1f - this._limits.x * 2f) * this._size.x - this._labelRoot.rect.width;
		float num4 = (1f - this._limits.y * 2f) * this._size.y - this._labelRoot.rect.height;
		this._labelRoot.anchoredPosition = new Vector2(this._size.x * this._limits.x + Mathf.Lerp(0f, num3, this._offset.x), -(this._size.y * this._limits.y + Mathf.Lerp(0f, num4, this._offset.y)));
	}

	public override string ToString()
	{
		return "PhotoModeScreenFish";
	}

	[SerializeField]
	private RectTransform _labelRoot;

	[SerializeField]
	private Vector2 _limits = new Vector2(0.05f, 0.05f);

	[SerializeField]
	private float _offsetSpeed;

	[SerializeField]
	private Text _fishDescription;

	[SerializeField]
	private Text _lengthValue;

	[SerializeField]
	private Text _weightValue;

	[SerializeField]
	private float _mouseSensitivity = 0.02f;

	[SerializeField]
	private Vector2 _offset = new Vector2(0.5f, 1f);

	private Vector2 _size;
}
