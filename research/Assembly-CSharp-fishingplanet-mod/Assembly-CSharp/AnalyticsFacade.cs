using System;
using ObjectModel;

public static class AnalyticsFacade
{
	public static void WriteNewLevel(int levelNumber)
	{
	}

	public static void WriteSpentReal(string productName, decimal amount)
	{
	}

	public static void WriteSpentGold(string productName, int amount, int productCounts = 1)
	{
	}

	public static void WriteSpentGold(InventoryItem product, int amount, AnalyticsFacade.ShopLocation location)
	{
	}

	public static void WriteSpentSilver(string productName, int amount, int productCounts = 1)
	{
	}

	public static void WriteSpentSilver(InventoryItem product, int amount, AnalyticsFacade.ShopLocation location)
	{
	}

	public static void WriteSpentSilver(ShopLicense product, int amount, AnalyticsFacade.ShopLocation location)
	{
	}

	public static void WriteEarnedGold(string productName, int amount)
	{
	}

	public static void WriteEarnedGold(InventoryItem product, int amount)
	{
	}

	public static void WriteEarnedSilver(string productName, int amount)
	{
	}

	public static void WriteEarnedSilver(InventoryItem product, int amount)
	{
	}

	public static void WriteTravelToPond(int pondId, Profile user)
	{
	}

	public static void CatchFish(Fish fish, int pondID, string weatherName, DateTime inGameTime, bool isReleased)
	{
	}

	public enum ShopLocation
	{
		GlobalShop,
		LocalShop
	}
}
