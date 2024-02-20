using System;
using UnityEngine;

public static class PaymentHelper
{
	public static string GetBuyMoneyUrl(int languageId, string token, int productId)
	{
		return string.Format("{0}/{1}/Payment/BuyMoney?productId={3}&token={2}", new object[]
		{
			PaymentHelper.PaymentBaseUrl,
			PaymentHelper.LanguageId2LanguageName(languageId),
			WWW.EscapeURL(token),
			productId
		});
	}

	public static string GetBuySubscriptionUrl(int languageId, string token, int productId)
	{
		return string.Format("{0}/{1}/Payment/BuySubscription?productId={3}&token={2}", new object[]
		{
			PaymentHelper.PaymentBaseUrl,
			PaymentHelper.LanguageId2LanguageName(languageId),
			WWW.EscapeURL(token),
			productId
		});
	}

	private static string LanguageId2LanguageName(int languageId)
	{
		int num = Array.IndexOf<int>(PaymentHelper.ValidLanguageId, languageId);
		return PaymentHelper.ValidCultures[num];
	}

	public static string PaymentBaseUrl = "https://account.fishingplanet.com";

	private const string PaymentMoneyQuery = "{0}/{1}/Payment/BuyMoney?productId={3}&token={2}";

	private const string PaymentSubscriptionQuery = "{0}/{1}/Payment/BuySubscription?productId={3}&token={2}";

	private static readonly string[] ValidCultures = new string[] { "en", "ru", "en", "de", "fr", "pl", "uk", "it", "es" };

	private static readonly int[] ValidLanguageId = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
}
