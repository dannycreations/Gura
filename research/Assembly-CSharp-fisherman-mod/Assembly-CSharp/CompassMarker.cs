using System;
using System.Collections;
using ObjectModel;
using TMPro;
using UnityEngine;

public class CompassMarker : CompassMarkerBase
{
	public HintArrowType3D HintType
	{
		get
		{
			return this.Type;
		}
	}

	private void OnDestroy()
	{
		base.StopAllCoroutines();
	}

	public void Init(HintArrowType3D type, Vector3 pos)
	{
		this.Type = type;
		if (this.Type == HintArrowType3D.Undefined)
		{
			this.Type = HintArrowType3D.Fish;
			RaycastHit raycastHit;
			if (Physics.Raycast(pos, Vector3.down, ref raycastHit, 50f, GlobalConsts.GroundObstacleMask) && raycastHit.point.y > 0f)
			{
				this.Type = HintArrowType3D.Pointer;
			}
		}
		this._icoText.text = ((!HintSettings.IcoDictionary.ContainsKey(this.Type)) ? string.Empty : HintSettings.IcoDictionary[this.Type]);
		if (base.gameObject.activeInHierarchy)
		{
			base.StartCoroutine(this.HideShine());
		}
	}

	protected IEnumerator HideShine()
	{
		yield return new WaitForSeconds(6f);
		this._shine.SetActive(false);
		yield break;
	}

	[SerializeField]
	private GameObject _shine;

	[SerializeField]
	private TextMeshProUGUI _icoText;

	protected HintArrowType3D Type;

	protected const float MaxCastDist = 50f;

	protected const float ShineTime = 6f;
}
