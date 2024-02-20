using System;
using System.Collections;
using ObjectModel;
using UnityEngine;

public class ManagedHintObject3DBillboardText : ManagedHintObject
{
	protected override void Update()
	{
		base.Update();
		if (this.observer.Message.ArrowType3D == HintArrowType3D.Pointer && this.observer.Message.ScenePosition != null && this._bbText.cameraToLookAt == null && Camera.main != null)
		{
			ManagedHintObject3DBillboardText.HintType hintType = ManagedHintObject3DBillboardText.HintType.Water;
			RaycastHit raycastHit;
			if (Physics.Raycast(base.transform.position, Vector3.down, ref raycastHit, 50f, GlobalConsts.GroundObstacleMask) && raycastHit.point.y > 0f)
			{
				hintType = ManagedHintObject3DBillboardText.HintType.Terrain;
			}
			this._bbText.SetText(string.Empty);
			this._bbText.cameraToLookAt = Camera.main;
			base.StartCoroutine(this.ShowText(this.GetIcoText(hintType)));
		}
	}

	private IEnumerator ShowText(string text)
	{
		yield return new WaitForEndOfFrame();
		this._bbText.SetText(text);
		yield break;
	}

	protected override void Show()
	{
		base.gameObject.SetActive(true);
	}

	protected override void Hide()
	{
		base.gameObject.SetActive(false);
	}

	private string GetIcoText(ManagedHintObject3DBillboardText.HintType ht)
	{
		return (ht != ManagedHintObject3DBillboardText.HintType.Water) ? "\ue802" : "\ue634";
	}

	[SerializeField]
	private BillboardText _bbText;

	private const float MaxCastDist = 50f;

	private enum HintType : byte
	{
		Terrain,
		Water
	}
}
