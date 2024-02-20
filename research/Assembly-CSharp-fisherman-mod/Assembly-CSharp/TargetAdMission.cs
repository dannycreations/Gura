using System;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetAdMission : MonoBehaviour
{
	public void Init(MissionOnClient m, int? missionId, int? taskId)
	{
		this.ImageLoader.Load(m.ThumbnailBID, this._missionIco, "Textures/Inventory/{0}");
		this._title.text = ScriptLocalization.Get("RecommendedForCaption").ToUpper();
		this._misisonValue.text = m.Name;
		MissionTaskOnClient missionTaskOnClient = ((taskId == null) ? null : m.Tasks.FirstOrDefault((MissionTaskOnClient p) => p.TaskId == taskId.GetValueOrDefault() && taskId != null));
		bool flag = missionTaskOnClient != null;
		this._taskValue.text = ((!flag) ? string.Empty : missionTaskOnClient.Name);
		if (!flag)
		{
			this._taskIco.text = string.Empty;
			this._taskBg.gameObject.SetActive(false);
		}
	}

	public void SetActive(bool isActive)
	{
		ShortcutExtensions.DOKill(this._сanvasGroup, false);
		ShortcutExtensions.DOFade(this._сanvasGroup, (!isActive) ? 0f : 1f, (!isActive) ? 0f : 0.5f);
	}

	[SerializeField]
	private TextMeshProUGUI _title;

	[SerializeField]
	private TextMeshProUGUI _misisonValue;

	[SerializeField]
	private TextMeshProUGUI _taskValue;

	[SerializeField]
	private TextMeshProUGUI _taskIco;

	[SerializeField]
	private Image _taskBg;

	[SerializeField]
	private Image _missionIco;

	[SerializeField]
	private CanvasGroup _сanvasGroup;

	private ResourcesHelpers.AsyncLoadableImage ImageLoader = new ResourcesHelpers.AsyncLoadableImage();

	private const float AnimTime = 0.5f;
}
