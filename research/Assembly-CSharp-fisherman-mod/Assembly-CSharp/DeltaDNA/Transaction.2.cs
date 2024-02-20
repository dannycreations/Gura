using System;

namespace DeltaDNA
{
	public class Transaction : Transaction<Transaction>
	{
		public Transaction(string name, string type, Product productsReceived, Product productsSpent)
			: base(name, type, productsReceived, productsSpent)
		{
		}
	}
}
