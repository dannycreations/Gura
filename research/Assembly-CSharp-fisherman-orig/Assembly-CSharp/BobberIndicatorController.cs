using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class BobberIndicatorController : MonoBehaviour, IBobberIndicatorController
{
	public float Sensitivity { get; set; }

	public bool HintActive { get; set; }

	internal void Start()
	{
		if (GameFactory.BobberIndicator != null)
		{
			throw new SceneConfigException("BobberIndicator could be only one");
		}
		if (this.Bobber == null)
		{
			throw new PrefabConfigException("Bobber Indicator prefab has error. Bobber image ref is null");
		}
		this.bobberTransform = this.Bobber.GetComponent<RectTransform>();
		this.bobberImage = this.Bobber.GetComponent<Image>();
		if (this.bobberTransform == null)
		{
			throw new PrefabConfigException("Bobber Indicator prefab has error. Bobber image ref has no RectTransform");
		}
		if (this.bobberImage == null)
		{
			throw new PrefabConfigException("Bobber Indicator prefab has error. Bobber image ref has no Image");
		}
		this._bobberColor = this.bobberImage.color;
		GameFactory.BobberIndicator = this;
		if (GameFactory.Player.Tackle != null)
		{
			GameFactory.BobberIndicator.Sensitivity = GameFactory.Player.Tackle.Sensitivity;
		}
		if (this.Panel != null)
		{
			this.Panel.SetActive(false);
		}
		this.Info.text = string.Empty;
		this._infoDefaultColor = this.Info.color;
	}

	internal void Update()
	{
		Vector3 eulerAngles = GameFactory.Player.Tackle.transform.rotation.eulerAngles;
		Quaternion quaternion = Quaternion.Euler(eulerAngles.x, 0f, eulerAngles.z);
		float num = Mathf.Clamp(Quaternion.Angle(Quaternion.identity, quaternion), -90f, 90f);
		num = 90f * Mathf.Sign(num) * Mathf.Pow(Mathf.Abs(num / 90f), 1f / Mathf.Max(this.Sensitivity, 0.01f));
		this.bobberTransform.localRotation = Quaternion.Euler(0f, 0f, num);
		float num2 = 0f;
		if (GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.State == typeof(FishSwimAway))
		{
			Vector2 vector = this.UpdateHorizontalVelocity();
			num2 = ((vector.y >= 0f) ? 1f : (-1f)) * vector.magnitude * 60f;
			num2 = Mathf.Clamp(num2, -100f, 100f);
		}
		if (PhotonConnectionFactory.Instance.Profile.Level < 4 && GameFactory.Player.Tackle.Fish != null)
		{
			if (GameFactory.Player.Tackle.Fish.State == typeof(FishBite))
			{
				this.Info.text = ScriptLocalization.Get("WaitText");
				this.EnableStrikeWarning(this.Info.gameObject, false);
			}
			else if (GameFactory.Player.Tackle.Fish.State == typeof(FishSwimAway))
			{
				this.EnableStrikeWarning(this.Info.gameObject, true);
				this.Info.text = ScriptLocalization.Get("StrikeText");
			}
			else
			{
				this.Info.text = string.Empty;
				this.EnableStrikeWarning(this.Info.gameObject, false);
			}
		}
		else
		{
			this.Info.text = string.Empty;
			this.EnableStrikeWarning(this.Info.gameObject, false);
		}
		float y = (GameFactory.Player.Tackle as FloatBehaviour).WaterMark.position.y;
		float num3 = 0f;
		if (y > 0f)
		{
			num3 = Mathf.Atan(y * 20f) * 25f * this.Sensitivity;
		}
		else if (y < 0f)
		{
			num3 = y * 200f * this.Sensitivity;
		}
		this.bobberTransform.localPosition = new Vector3(num2, num3, 0f);
		if (!this.HintActive)
		{
			float num4 = Mathf.Sqrt(num3 * num3 + num2 * num2);
			float num5 = Mathf.Clamp01(0.96f - Mathf.Clamp01(Mathf.Abs(num4) / 100f));
			this.bobberImage.color = new Color(this._bobberColor.r, this._bobberColor.g, this._bobberColor.b, num5);
		}
	}

	public void SetBobberColor(Color c)
	{
		if (this.bobberImage != null)
		{
			this.bobberImage.color = c;
		}
	}

	private Vector2 UpdateHorizontalVelocity()
	{
		Vector3 position = GameFactory.Player.Tackle.transform.position;
		Vector2 vector;
		vector..ctor(position.x, position.z);
		Vector2 vector2 = Vector2.zero;
		Vector2? vector3 = this.priorPosition;
		if (vector3 != null)
		{
			vector2 = (vector - this.priorPosition.Value) / Time.deltaTime;
		}
		this.priorPosition = new Vector2?(vector);
		return vector2;
	}

	public void Show()
	{
		this.Panel.SetActive(true);
	}

	public void Hide()
	{
		this.Panel.SetActive(false);
		this.priorPosition = null;
	}

	private void EnableStrikeWarning(GameObject enableStrikeOn, bool enable)
	{
		TextColorPulsation textColorPulsation = enableStrikeOn.GetComponent<TextColorPulsation>() ?? enableStrikeOn.AddComponent<TextColorPulsation>();
		if (!enable)
		{
			this.Info.color = this._infoDefaultColor;
		}
		textColorPulsation.enabled = enable;
		textColorPulsation.StartColor = Color.red;
		textColorPulsation.MinAlpha = 0f;
		textColorPulsation.PulseTime = 1f;
	}

	public GameObject Panel;

	public GameObject Level;

	public GameObject Bobber;

	public Text Info;

	private RectTransform bobberTransform;

	private Image bobberImage;

	private Color _infoDefaultColor;

	private Color _bobberColor;

	private Vector2? priorPosition;
}
