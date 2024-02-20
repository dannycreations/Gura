using System;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory.Mixing;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Mixing : InfoMessage
{
	private void Awake()
	{
		this._alphaFade = base.GetComponent<AlphaFade>();
		this._buttonMix.onClick.AddListener(new UnityAction(this.Mix));
		this._buttonOk.onClick.AddListener(new UnityAction(this.Close));
		this._buttonCancel.onClick.AddListener(delegate
		{
			if (this._mixingEmulator != null)
			{
				this._mixingEmulator.FullStop();
			}
			this.Close();
		});
	}

	private void Update()
	{
		if (this._mixingEmulator != null)
		{
			this._mixingEmulator.Update();
		}
	}

	private void OnDestroy()
	{
		if (this._mixingEmulator != null)
		{
			this._mixingEmulator.ChangeState -= this._mixingEmulator_ChangeState;
			this._mixingEmulator.Dispose();
			this._mixingEmulator = null;
		}
	}

	public bool Init(ChumIngredient ingredient, Func<InventoryItem, InventoryItem, bool> transferItemFunc)
	{
		double? weight = ingredient.Weight;
		double num = ((weight == null) ? 0.0 : weight.Value);
		if (num <= 0.0)
		{
			UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), "Can't mix chum - ingredient without weight configured", true, null, false);
			return false;
		}
		this._transferItemFunc = transferItemFunc;
		this._ingredient = ingredient;
		bool flag = ingredient.ItemSubType == ItemSubTypes.ChumGroundbaits || ingredient.ItemSubType == ItemSubTypes.ChumMethodMix;
		double num2 = ((!flag) ? 0.0 : Math.Round(num / 2.0, 2, MidpointRounding.AwayFromZero));
		double num3 = num + num2;
		string[] array = MeasuringSystemManager.FormatWeight(new double[] { num, num2, num3 });
		this._titleText.text = this._ingredient.Name;
		this._weightText.text = array[0];
		this._addText.text = array[1];
		this._icoLdbl.Image = this._ico;
		this._icoLdbl.Load(string.Format("Textures/Inventory/{0}", (ingredient.ThumbnailBID == null) ? "0" : ingredient.ThumbnailBID.ToString()));
		this._chum = new Chum();
		Chum chum = this._chum;
		ChumIngredient ingredient2 = this._ingredient;
		double? weight2 = this._ingredient.Weight;
		Inventory.AddChumIngredient(chum, ingredient2, (weight2 == null) ? 0.0 : weight2.Value);
		ChumBase heaviestChumBase = this._chum.HeaviestChumBase;
		this._titleTextResult.text = ((ingredient.SpecialItem != InventorySpecialItem.Snow) ? Inventory.GetChumNameMixed(heaviestChumBase.Name) : ScriptLocalization.Get("SnowballsCaption"));
		this._chum.Name = this._titleTextResult.text;
		this._weightTextResult.text = array[2];
		this._icoResultLdbl.Image = this._icoResult;
		this._icoResultLdbl.Load(string.Format("Textures/Inventory/{0}", (heaviestChumBase.DollThumbnailBID == null) ? "0" : heaviestChumBase.DollThumbnailBID.ToString()));
		this._mixingEmulator = new MixingEmulator(this._chum, this._progressPrc, this._progress, this._buttonOk, this._buttonCancel, this._noPremText, this._premText);
		this._mixingEmulator.ChangeState += this._mixingEmulator_ChangeState;
		return true;
	}

	private void Mix()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (this._ingredient == null)
		{
			return;
		}
		this._chum.UpdateIngredients(true);
		if (profile.Inventory.CanMixChum(this._chum))
		{
			this._mixingEmulator.Init(this._transferItemFunc);
		}
		else
		{
			string lastVerificationError = profile.Inventory.LastVerificationError;
			UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), lastVerificationError, true, null, false);
		}
	}

	private void Close()
	{
		this._transferItemFunc = null;
		this._alphaFade.HidePanel();
	}

	private void _mixingEmulator_ChangeState()
	{
		this._buttonOk.gameObject.SetActive(this._mixingEmulator.InState(MixingEmulator.States.Finish));
		this._buttonMixing.gameObject.SetActive(this._mixingEmulator.InState(MixingEmulator.States.Start) || this._mixingEmulator.InState(MixingEmulator.States.Process) || this._mixingEmulator.InState(MixingEmulator.States.Ready));
		this._buttonMix.gameObject.SetActive(!this._buttonOk.gameObject.activeSelf && !this._buttonMixing.gameObject.activeSelf);
	}

	[SerializeField]
	private Image _ico;

	private ResourcesHelpers.AsyncLoadableImage _icoLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	private Image _icoResult;

	private ResourcesHelpers.AsyncLoadableImage _icoResultLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	private Text _titleText;

	[SerializeField]
	private Text _weightText;

	[SerializeField]
	private Text _addText;

	[SerializeField]
	private Text _waterTitle;

	[SerializeField]
	private Text _titleTextResult;

	[SerializeField]
	private Text _weightTextResult;

	[SerializeField]
	private BorderedButton _buttonMix;

	[SerializeField]
	private BorderedButton _buttonMixing;

	[SerializeField]
	private BorderedButton _buttonOk;

	[SerializeField]
	private BorderedButton _buttonCancel;

	[SerializeField]
	private Text _progressPrc;

	[SerializeField]
	private Image _progress;

	[SerializeField]
	private TextMeshProUGUI _noPremText;

	[SerializeField]
	private TextMeshProUGUI _premText;

	private AlphaFade _alphaFade;

	private ChumIngredient _ingredient;

	private Chum _chum;

	private Func<InventoryItem, InventoryItem, bool> _transferItemFunc;

	private MixingEmulator _mixingEmulator;
}
