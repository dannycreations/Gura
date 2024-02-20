using System;
using Assets.Scripts.UI._2D.Inventory.Mixing;

public class ChumComponentWater : ChumComponentEmpty
{
	protected override void Awake()
	{
	}

	public void Init(Types type, UiTypes uiType, float v)
	{
		this._type = type;
		this._uiType = uiType;
		this.UpdateValue(v);
	}

	public void UpdateValue(float v)
	{
		string text = string.Format("{0} {1}", MeasuringSystemManager.Kilograms2Grams(v), MeasuringSystemManager.GramsOzWeightSufix());
		this.Init(text);
	}
}
