using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.GlobalMap;
using UnityEngine;

public class PondsRotations : MonoBehaviour
{
	public void Set(Ponds pondId, Vector3 rotation, float cameraScale)
	{
		this.Set(pondId, new PondsRotations.PondRotationData
		{
			CameraScale = cameraScale,
			Rot = Quaternion.Euler(rotation)
		});
	}

	public void Set(Ponds pondId, Quaternion rotation, float cameraScale)
	{
		this.Set(pondId, new PondsRotations.PondRotationData
		{
			CameraScale = cameraScale,
			Rot = rotation
		});
	}

	public void Clear(Ponds pondId)
	{
		List<PondsRotations.PondRotation> list = this._pondRotations.ToList<PondsRotations.PondRotation>();
		int num = list.FindIndex((PondsRotations.PondRotation p) => p.PondId == pondId);
		if (num != -1)
		{
			list.RemoveAt(num);
			this._pondRotations = list.ToArray();
		}
	}

	public Quaternion FindRot(Ponds pondId, float cameraScale)
	{
		Quaternion quaternion = Quaternion.identity;
		List<PondsRotations.PondRotation> list = this._pondRotations.ToList<PondsRotations.PondRotation>();
		PondsRotations.PondRotation pondRotation = list.FirstOrDefault((PondsRotations.PondRotation p) => p.PondId == pondId);
		if (pondRotation != null && pondRotation.RotationData != null)
		{
			List<PondsRotations.PondRotationData> list2 = pondRotation.RotationData.ToList<PondsRotations.PondRotationData>();
			float num = float.MaxValue;
			float num2 = Mathf.Abs(cameraScale);
			for (int i = 0; i < list2.Count; i++)
			{
				float num3 = Mathf.Abs(Mathf.Abs(list2[i].CameraScale) - num2);
				if (num3 < num)
				{
					num = num3;
					quaternion = list2[i].Rot;
				}
			}
			if (quaternion == Quaternion.identity)
			{
				quaternion = list2[0].Rot;
			}
		}
		return quaternion;
	}

	private void Set(Ponds pondId, PondsRotations.PondRotationData prd)
	{
		List<PondsRotations.PondRotation> list = this._pondRotations.ToList<PondsRotations.PondRotation>();
		int num = list.FindIndex((PondsRotations.PondRotation p) => p.PondId == pondId);
		if (num == -1)
		{
			list.Add(new PondsRotations.PondRotation
			{
				PondId = pondId
			});
			num = list.Count - 1;
		}
		if (list[num].RotationData == null)
		{
			list[num].RotationData = new PondsRotations.PondRotationData[] { prd };
		}
		else
		{
			List<PondsRotations.PondRotationData> list2 = list[num].RotationData.ToList<PondsRotations.PondRotationData>();
			list2.Add(prd);
			list[num].RotationData = list2.ToArray();
		}
		this._pondRotations = list.ToArray();
	}

	[Tooltip("Earth rotation angle for each pond")]
	[SerializeField]
	private PondsRotations.PondRotation[] _pondRotations;

	[Serializable]
	public class PondRotation
	{
		public Ponds PondId;

		public PondsRotations.PondRotationData[] RotationData;
	}

	[Serializable]
	public class PondRotationData
	{
		public Quaternion Rot = Quaternion.identity;

		public float CameraScale;
	}
}
