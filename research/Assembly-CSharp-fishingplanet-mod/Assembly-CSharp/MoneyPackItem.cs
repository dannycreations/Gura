using System;

public class MoneyPackItem : PremiumItemBase
{
	protected override BuyProductManager.UrlTypes UrlType()
	{
		return BuyProductManager.UrlTypes.BuyMoney;
	}

	protected override void SetCurrency()
	{
		string text = ((base.Product.Gold == null) ? "SC" : "GC");
		this.CurrencyIco.text = MeasuringSystemManager.GetCurrencyIcon(text);
		this.CurrencyName.text = MeasuringSystemManager.GetCurrencyName(text);
	}

	protected override void SetPrice()
	{
		this.Silvers.text = ((base.Product.Gold == null) ? base.Product.Silver.ToString() : base.Product.Gold.Value.ToString("D"));
	}
}
