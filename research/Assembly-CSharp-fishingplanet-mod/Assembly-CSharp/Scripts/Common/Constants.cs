using System;

namespace Scripts.Common
{
	public static class Constants
	{
		public const string Password = "Password";

		public const string Email = "Email";

		public const string AppName = "Master";

		public const string SendSystemInfo = "SentSystemInfo2";

		public const int MaxPassLength = 6;

		public enum AB_TESTS
		{
			SKIP_CHARACTER_CUSTOMIZATION_ON_START = 1,
			INITIAL_MONEY_TEST,
			NEW_PREMIUM_SHOP_IMPLEMETATION,
			PREM_SHOP_BUY_OPTIONS
		}
	}
}
