using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RodPresetsInit : MonoBehaviour
{
	public void Awake()
	{
		this._gos = new List<GameObject>();
	}

	public void OnEnable()
	{
		this.RebuildEntries();
		UIStatsCollector.ChangeGameScreen(GameScreenType.Presets, GameScreenTabType.Undefined, null, null, null, null, null);
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.RebuildEntries;
		PhotonConnectionFactory.Instance.OnSharedRodSetup += this.OnShare;
		PhotonConnectionFactory.Instance.OnShareRodSetupFailure += this.OnShareFailed;
		PhotonConnectionFactory.Instance.OnReceiveNewRodSetup += this.OnReceiveNewRodSetup;
		PhotonConnectionFactory.Instance.OnProductBought += this.OnProductBought;
	}

	public void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.RebuildEntries;
		PhotonConnectionFactory.Instance.OnSharedRodSetup -= this.OnShare;
		PhotonConnectionFactory.Instance.OnShareRodSetupFailure -= this.OnShareFailed;
		PhotonConnectionFactory.Instance.OnReceiveNewRodSetup -= this.OnReceiveNewRodSetup;
		PhotonConnectionFactory.Instance.OnProductBought -= this.OnProductBought;
		RodPresetEntry.LastSelected = null;
	}

	private void OnProductBought(ProfileProduct product, int count)
	{
		this.RebuildEntries();
	}

	private void OnShare(RodSetup setup, string friendName)
	{
		GameFactory.Message.ShowMessage(ScriptLocalization.Get("SentRodSetupText"), base.transform.root.gameObject, 4f, false);
	}

	private void OnShareFailed(Failure failure)
	{
		GameFactory.Message.ShowMessage("Failed to send Rod Preset", base.transform.root.gameObject, 4f, false);
	}

	private void OnReceiveNewRodSetup(RodSetup setup, string sender)
	{
		base.Invoke("RebuildEntries", 0.1f);
	}

	public void RebuildEntries()
	{
		EventSystem.current.SetSelectedGameObject(null);
		this.activeRod = this.Preview.TackleContent.ActiveRod;
		this.Counter.text = string.Format("{0}/{1}", PhotonConnectionFactory.Instance.Profile.InventoryRodSetups.Count, PhotonConnectionFactory.Instance.Profile.Inventory.CurrentRodSetupCapacity);
		int i = 0;
		this.tg = base.GetComponent<ToggleGroup>();
		foreach (RodSetup rodSetup in PhotonConnectionFactory.Instance.Profile.InventoryRodSetups)
		{
			GameObject gameObject;
			if (i < this._gos.Count)
			{
				gameObject = this._gos[i];
			}
			else
			{
				gameObject = GUITools.AddChild(this.ScrollContent, this.RodSetupEntryPrefab);
				this._gos.Add(gameObject);
			}
			RodPresetEntry component = gameObject.GetComponent<RodPresetEntry>();
			component.Init(rodSetup);
			component.Preview = this.Preview;
			component.Border.group = this.tg;
			component.name = "preset_" + i;
			i++;
		}
		while (i < PhotonConnectionFactory.Instance.Profile.Inventory.CurrentRodSetupCapacity)
		{
			GameObject gameObject2;
			if (i < this._gos.Count)
			{
				gameObject2 = this._gos[i];
			}
			else
			{
				gameObject2 = GUITools.AddChild(this.ScrollContent, this.RodSetupEntryPrefab);
				this._gos.Add(gameObject2);
			}
			RodPresetEntry component2 = gameObject2.GetComponent<RodPresetEntry>();
			component2.SetEmpty();
			component2.Preview = this.Preview;
			component2.Border.group = this.tg;
			component2.name += i;
			i++;
		}
		if (PhotonConnectionFactory.Instance.Profile.Inventory.CurrentRodSetupCapacity < Inventory.MaxRodSetupCapacity)
		{
			GameObject gameObject3;
			if (i < this._gos.Count)
			{
				gameObject3 = this._gos[i];
			}
			else
			{
				gameObject3 = GUITools.AddChild(this.ScrollContent, this.RodSetupEntryPrefab);
				this._gos.Add(gameObject3);
			}
			RodPresetEntry component3 = gameObject3.GetComponent<RodPresetEntry>();
			component3.SetBuySlot();
			component3.Border.group = this.tg;
			component3.Preview = this.Preview;
			i++;
		}
		while (i < this._gos.Count)
		{
			Object.Destroy(this._gos[i]);
			this._gos.RemoveAt(i);
			i++;
		}
	}

	private void FixedUpdate()
	{
		if (this.Preview.TackleContent.ActiveRod != this.activeRod)
		{
			this.RebuildEntries();
		}
		if (this.tg != null && (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.transform.IsChildOf(base.transform)))
		{
			this.Preview.Reset(false);
			if (RodPresetEntry.LastSelected != null)
			{
				RodPresetEntry.LastSelected.OnDeselect();
			}
			this.tg.SetAllTogglesOff();
		}
	}

	public GameObject RodSetupEntryPrefab;

	public GameObject ScrollContent;

	public RodPresetPreview Preview;

	public Text Counter;

	private List<GameObject> _gos;

	private ToggleGroup tg;

	private InitRod activeRod;
}
