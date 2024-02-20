using System;

namespace DeltaDNA
{
	public class Transaction<T> : GameEvent<T> where T : Transaction<T>
	{
		public Transaction(string name, string type, Product productsReceived, Product productsSpent)
			: base("transaction")
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Name cannot be null or empty");
			}
			if (string.IsNullOrEmpty(type))
			{
				throw new ArgumentException("Type cannot be null or empty");
			}
			if (productsReceived == null)
			{
				throw new ArgumentException("Products received cannot be null");
			}
			if (productsSpent == null)
			{
				throw new ArgumentException("Products spent cannot be null");
			}
			base.AddParam("transactionName", name);
			base.AddParam("transactionType", type);
			base.AddParam("productsReceived", productsReceived);
			base.AddParam("productsSpent", productsSpent);
		}

		public T SetTransactionId(string transactionId)
		{
			if (string.IsNullOrEmpty(transactionId))
			{
				throw new ArgumentException("transactionId cannot be null or empty");
			}
			base.AddParam("transactionID", transactionId);
			return (T)((object)this);
		}

		public T SetReceipt(string receipt)
		{
			if (string.IsNullOrEmpty(receipt))
			{
				throw new ArgumentException("receipt cannot be null or empty");
			}
			base.AddParam("transactionReceipt", receipt);
			return (T)((object)this);
		}

		public T SetServer(string server)
		{
			if (string.IsNullOrEmpty(server))
			{
				throw new ArgumentException("server cannot be null or empty");
			}
			base.AddParam("transactionServer", server);
			return (T)((object)this);
		}

		public T SetTransactorId(string transactorId)
		{
			if (string.IsNullOrEmpty(transactorId))
			{
				throw new ArgumentException("transactorId cannot be null or empty");
			}
			base.AddParam("transactorID", transactorId);
			return (T)((object)this);
		}

		public T SetProductId(string productId)
		{
			if (string.IsNullOrEmpty(productId))
			{
				throw new ArgumentException("productId cannot be null or empty");
			}
			base.AddParam("productID", productId);
			return (T)((object)this);
		}
	}
}
