using System;
using System.Collections;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageIconManagerChum : MonoBehaviour
{
	public string DamageHours
	{
		get
		{
			return this._damageHours.text;
		}
	}

	public void Init(Chum ii)
	{
		this._ii = ii;
		if (base.gameObject.activeSelf)
		{
			this.Refresh();
		}
	}

	private void OnEnable()
	{
		this.Refresh();
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
	}

	private void Refresh()
	{
		if (this._ii != null && base.gameObject.activeInHierarchy)
		{
			float num = this._ii.BaseUsageTimePercentageRemain / 100f;
			float num2 = this._ii.BaseUsageTimeRemain;
			if (num2 < 0f)
			{
				num = 0f;
				num2 = 0f;
			}
			if (!this._damageImage.transform.parent.gameObject.activeSelf)
			{
				this._damageImage.transform.parent.gameObject.SetActive(true);
			}
			this._damageImage.fillAmount = num;
			this._damageImage.color = Color.Lerp(this._normalColor, this._damagedColor, 1f - num);
			this._damageHours.text = ((num2 <= 1f) ? num2.ToString("0.0").TrimEnd(new char[] { '0' }).TrimEnd(new char[] { ',' })
				.TrimEnd(new char[] { '.' }) : num2.ToString("0").TrimEnd(new char[] { '0' }).TrimEnd(new char[] { ',' })
				.TrimEnd(new char[] { '.' }));
			base.StartCoroutine(this.RefreshByTime());
		}
		else
		{
			this._damageImage.transform.parent.gameObject.SetActive(false);
		}
	}

	private IEnumerator RefreshByTime()
	{
		yield return new WaitForSeconds(10f);
		this.Refresh();
		yield break;
	}

	[SerializeField]
	private Image _damageImage;

	[SerializeField]
	private TextMeshProUGUI _damageHours;

	[SerializeField]
	private Color _normalColor = Color.yellow;

	[SerializeField]
	private Color _damagedColor = Color.red;

	private const float RefreshTime = 10f;

	private Chum _ii;
}
