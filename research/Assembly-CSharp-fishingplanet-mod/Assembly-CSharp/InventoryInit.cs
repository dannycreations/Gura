using System;
using InventorySRIA;
using UnityEngine;

public class InventoryInit : ActivityStateControlled
{
	private void Awake()
	{
		InventoryInit.Instance = this;
	}

	protected override void Start()
	{
		base.Start();
		GameObject gameObject = Object.Instantiate<GameObject>(this._mixingPrefab, Vector3.zero, Quaternion.identity, this._mixingContent.transform);
		gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		gameObject.GetComponent<DropMeChum>().DragNDropTypeInst = base.GetComponent<DragNDropType>();
	}

	protected override void SetHelp()
	{
		if (this.SRIA != null)
		{
			this.SRIA.SubscribeAndRefresh();
		}
	}

	protected override void HideHelp()
	{
		if (this.SRIA != null)
		{
			this.SRIA.Unsubscribe();
		}
	}

	[SerializeField]
	private GameObject _mixingPrefab;

	[SerializeField]
	private GameObject _mixingContent;

	public GameObject CountableSelectionPrefab;

	public GameObject BackgroundOperationPrefab;

	public RectTransform TutorialBlackoutParent;

	public DragMeDoll[] TutorialDollDisableItems;

	public GameObject TutorialBaitGlowPanel;

	public GameObject TutorialBaitContent;

	public GameObject TutorialTerminalContent;

	public GameObject TutorialLineContent;

	public global::InventorySRIA.InventorySRIA SRIA;

	public static InventoryInit Instance;
}
