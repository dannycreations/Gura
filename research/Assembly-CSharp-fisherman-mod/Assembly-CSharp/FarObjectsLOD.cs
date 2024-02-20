using System;
using System.Linq;
using UnityEngine;

public class FarObjectsLOD : MonoBehaviour, ILazyManualUpdate
{
	private void Awake()
	{
		base.gameObject.SetActive(false);
	}

	public void Init(Transform target)
	{
		base.gameObject.SetActive(true);
		this._lods = new GameObject[base.transform.childCount];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			this._lods[i] = base.transform.GetChild(i).gameObject;
			this._lods[i].SetActive(false);
		}
		this._target = target;
		this._settings = this._settings.OrderBy((FarObjectsLOD.Record r) => r.QualityLevel).ToArray<FarObjectsLOD.Record>();
		if (this._settings.Length > 0)
		{
			int distances = this._lods.Length;
			bool flag = this._settings.All((FarObjectsLOD.Record r) => r.Distances.Length == distances);
			if (flag)
			{
				foreach (FarObjectsLOD.Record record in this._settings)
				{
					Array.Sort<int>(record.Distances);
				}
			}
			else
			{
				base.gameObject.SetActive(false);
				if (!flag)
				{
					LogHelper.Error("Some record of {0} has distances != {1}", new object[] { base.name, distances });
				}
			}
		}
	}

	public void ManualUpdate()
	{
		int qualityLevel = (int)SettingsManager.RenderQuality;
		if (this._target != null)
		{
			FarObjectsLOD.Record record = this._settings.FirstOrDefault((FarObjectsLOD.Record _) => _.QualityLevel >= qualityLevel);
			if (record == null)
			{
				record = this._settings[this._settings.Length - 1];
			}
			float magnitude = (this._target.position - base.transform.position).magnitude;
			int num;
			if (record.Distances.Length == 1)
			{
				num = ((magnitude >= (float)record.Distances[0]) ? (-1) : 0);
			}
			else
			{
				num = record.Distances.Length - 1;
				for (int i = 0; i < record.Distances.Length; i++)
				{
					if (magnitude < (float)record.Distances[i])
					{
						num = i;
						break;
					}
				}
			}
			if ((int)this._lastObjectIndex != num)
			{
				if ((int)this._lastObjectIndex - num == 1 && ((num == -1 && magnitude > (float)record.Distances[0] * (1f + this._threshold)) || (num != -1 && (float)record.Distances[num] * (1f - this._threshold) < magnitude)))
				{
					return;
				}
				if ((int)this._lastObjectIndex != -1)
				{
					this._lods[(int)this._lastObjectIndex].SetActive(false);
				}
				this._lastObjectIndex = (sbyte)num;
				if ((int)this._lastObjectIndex != -1)
				{
					this._lods[(int)this._lastObjectIndex].SetActive(true);
				}
			}
		}
	}

	private void OnDestroy()
	{
		this._lods = null;
	}

	[SerializeField]
	private FarObjectsLOD.Record[] _settings;

	[SerializeField]
	private float _threshold = 0.05f;

	[SerializeField]
	private bool _useDebugQuality;

	[SerializeField]
	private int _debugQualityLevel;

	private Transform _target;

	private sbyte _lastObjectIndex = -1;

	private GameObject[] _lods;

	[Serializable]
	public class Record
	{
		public int QualityLevel;

		public int[] Distances;
	}
}
