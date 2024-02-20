using System;
using UnityEngine;
using UnityEngine.UI;

public class DamageHandler : MonoBehaviour
{
	internal void Start()
	{
		this._tempRodColor = this.Rod.GetComponent<Image>().color;
		this._tempReelColor = this.Reel.GetComponent<Image>().color;
		this._reelImage = this.Reel.transform.Find("Image").gameObject;
		this._rodImage = this.Rod.transform.Find("Image").gameObject;
		this.Rod.SetActive(false);
		this.Reel.SetActive(false);
		this.Rod.SetActive(false);
		this.Reel.SetActive(false);
		this._reelBlinkIsOn = false;
		this._rodBlinkIsOn = false;
	}

	private void RodOn()
	{
		this.Rod.SetActive(true);
	}

	public void RodOff()
	{
		if (this._rodIsOn)
		{
			this._rodImage.GetComponent<Image>().color = Color.white;
			this.Rod.SetActive(Math.Abs(this.Rod.GetComponent<Image>().color.a) > 0.1f);
			this._rodIsOn = false;
		}
		else if (!this._rodBlinkIsOn)
		{
			this.Rod.SetActive(false);
		}
	}

	private void ReelOn()
	{
		this.Reel.SetActive(true);
	}

	public void ReelOff()
	{
		if (this._reelIsOn)
		{
			this._reelImage.GetComponent<Image>().color = Color.white;
			this.Reel.SetActive(Math.Abs(this.Reel.GetComponent<Image>().color.a) > 0.1f);
			this._reelIsOn = false;
		}
		else if (!this._reelBlinkIsOn)
		{
			this.Reel.SetActive(false);
		}
	}

	public void RodDamagedOn()
	{
		if (this._rodImage == null)
		{
			return;
		}
		this.Rod.SetActive(true);
		this._rodImage.SetActive(true);
		this._rodImage.GetComponent<Image>().color = this.DamageColor;
		this._rodIsOn = true;
	}

	public void RodHeavilyDamagedOn()
	{
		if (this._rodImage == null)
		{
			return;
		}
		this.RodDamagedOn();
		this._rodImage.GetComponent<Image>().color = this.VeryDamageColor;
	}

	public void ReelDamagedOn()
	{
		if (this._reelImage == null)
		{
			return;
		}
		this.Reel.SetActive(true);
		this._reelImage.SetActive(true);
		this._reelImage.GetComponent<Image>().color = this.DamageColor;
		this._reelIsOn = true;
	}

	public void ReelHeavilyDamagedOn()
	{
		if (this._reelImage == null)
		{
			return;
		}
		this.ReelDamagedOn();
		this._reelImage.GetComponent<Image>().color = this.VeryDamageColor;
	}

	public void RodBlinkOff()
	{
		this._rodBlinkIsOn = false;
		this.Rod.SetActive(this._rodIsOn);
		this.Rod.GetComponent<Image>().color = this._tempRodColor;
	}

	public void ReelBlinkOff()
	{
		this._reelBlinkIsOn = false;
		this.Reel.SetActive(this._reelIsOn);
		this.Rod.GetComponent<Image>().color = this._tempReelColor;
	}

	public void RodDamageBlink()
	{
		if (!this.Rod.activeSelf)
		{
			this._tempRodColor = this.Rod.GetComponent<Image>().color;
			this.Rod.GetComponent<Image>().color = this.DamageOverloadColor;
			this.Rod.SetActive(true);
			this._rodBlinkIsOn = true;
			base.Invoke("RodBlinkOff", this.BlinkDuration);
		}
	}

	public void RodBlink()
	{
		if (!this.Rod.activeSelf)
		{
			this._tempRodColor = this.Rod.GetComponent<Image>().color;
			this.Rod.GetComponent<Image>().color = this.OverloadColor;
			this.Rod.SetActive(true);
			this._rodBlinkIsOn = true;
			base.Invoke("RodBlinkOff", this.BlinkDuration);
		}
	}

	public void ReelDamageBlink()
	{
		if (!this.Reel.activeSelf)
		{
			this._tempReelColor = this.Reel.GetComponent<Image>().color;
			this.Reel.GetComponent<Image>().color = this.DamageOverloadColor;
			this.Reel.SetActive(true);
			this._reelBlinkIsOn = true;
			base.Invoke("ReelBlinkOff", this.BlinkDuration);
		}
	}

	public void ReelBlink()
	{
		if (!this.Reel.activeSelf)
		{
			this._tempReelColor = this.Reel.GetComponent<Image>().color;
			this.Reel.GetComponent<Image>().color = this.OverloadColor;
			this.Reel.SetActive(true);
			this._reelBlinkIsOn = true;
			base.Invoke("ReelBlinkOff", this.BlinkDuration);
		}
	}

	public void OnEnable()
	{
		this.ReelBlinkOff();
		this.RodBlinkOff();
	}

	public GameObject Rod;

	public GameObject Reel;

	public Color DamageColor;

	public Color VeryDamageColor;

	public Color OverloadColor;

	public Color DamageOverloadColor;

	public float BlinkDuration = 0.5f;

	private bool _reelIsOn;

	private bool _rodIsOn;

	private bool _reelBlinkIsOn;

	private bool _rodBlinkIsOn;

	private Color _tempRodColor = Color.white;

	private Color _tempReelColor = Color.white;

	private GameObject _reelImage;

	private GameObject _rodImage;
}
