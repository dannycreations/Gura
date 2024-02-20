using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

[DebuggerDisplay("Count = {Count}")]
[Serializable]
public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerable, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>
{
	public SerializableDictionary()
		: this(0, null)
	{
	}

	public SerializableDictionary(int capacity)
		: this(capacity, null)
	{
	}

	public SerializableDictionary(IEqualityComparer<TKey> comparer)
		: this(0, comparer)
	{
	}

	public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		this.Initialize(capacity);
		this._Comparer = comparer ?? EqualityComparer<TKey>.Default;
	}

	public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
		: this(dictionary, null)
	{
	}

	public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		: this((dictionary == null) ? 0 : dictionary.Count, comparer)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
		{
			this.Add(keyValuePair.Key, keyValuePair.Value);
		}
	}

	public Dictionary<TKey, TValue> AsDictionary
	{
		get
		{
			return new Dictionary<TKey, TValue>(this);
		}
	}

	public int Count
	{
		get
		{
			return this._Count - this._FreeCount;
		}
	}

	public TValue this[TKey key, TValue defaultValue]
	{
		get
		{
			int num = this.FindIndex(key);
			if (num >= 0)
			{
				return this._Values[num];
			}
			return defaultValue;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			int num = this.FindIndex(key);
			if (num >= 0)
			{
				return this._Values[num];
			}
			throw new KeyNotFoundException(key.ToString());
		}
		set
		{
			this.Insert(key, value, false);
		}
	}

	public bool ContainsValue(TValue value)
	{
		if (value == null)
		{
			for (int i = 0; i < this._Count; i++)
			{
				if (this._HashCodes[i] >= 0 && this._Values[i] == null)
				{
					return true;
				}
			}
		}
		else
		{
			EqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
			for (int j = 0; j < this._Count; j++)
			{
				if (this._HashCodes[j] >= 0 && @default.Equals(this._Values[j], value))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ContainsKey(TKey key)
	{
		return this.FindIndex(key) >= 0;
	}

	public void Clear()
	{
		if (this._Count <= 0)
		{
			return;
		}
		for (int i = 0; i < this._Buckets.Length; i++)
		{
			this._Buckets[i] = -1;
		}
		Array.Clear(this._Keys, 0, this._Count);
		Array.Clear(this._Values, 0, this._Count);
		Array.Clear(this._HashCodes, 0, this._Count);
		Array.Clear(this._Next, 0, this._Count);
		this._FreeList = -1;
		this._Count = 0;
		this._FreeCount = 0;
		this._Version++;
	}

	public void Add(TKey key, TValue value)
	{
		this.Insert(key, value, true);
	}

	private void Resize(int newSize, bool forceNewHashCodes)
	{
		int[] array = new int[newSize];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = -1;
		}
		TKey[] array2 = new TKey[newSize];
		TValue[] array3 = new TValue[newSize];
		int[] array4 = new int[newSize];
		int[] array5 = new int[newSize];
		Array.Copy(this._Values, 0, array3, 0, this._Count);
		Array.Copy(this._Keys, 0, array2, 0, this._Count);
		Array.Copy(this._HashCodes, 0, array4, 0, this._Count);
		Array.Copy(this._Next, 0, array5, 0, this._Count);
		if (forceNewHashCodes)
		{
			for (int j = 0; j < this._Count; j++)
			{
				if (array4[j] != -1)
				{
					array4[j] = this._Comparer.GetHashCode(array2[j]) & int.MaxValue;
				}
			}
		}
		for (int k = 0; k < this._Count; k++)
		{
			int num = array4[k] % newSize;
			array5[k] = array[num];
			array[num] = k;
		}
		this._Buckets = array;
		this._Keys = array2;
		this._Values = array3;
		this._HashCodes = array4;
		this._Next = array5;
	}

	private void Resize()
	{
		this.Resize(SerializableDictionary<TKey, TValue>.PrimeHelper.ExpandPrime(this._Count), false);
	}

	public bool Remove(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = this._Comparer.GetHashCode(key) & int.MaxValue;
		int num2 = num % this._Buckets.Length;
		int num3 = -1;
		for (int i = this._Buckets[num2]; i >= 0; i = this._Next[i])
		{
			if (this._HashCodes[i] == num && this._Comparer.Equals(this._Keys[i], key))
			{
				if (num3 < 0)
				{
					this._Buckets[num2] = this._Next[i];
				}
				else
				{
					this._Next[num3] = this._Next[i];
				}
				this._HashCodes[i] = -1;
				this._Next[i] = this._FreeList;
				this._Keys[i] = default(TKey);
				this._Values[i] = default(TValue);
				this._FreeList = i;
				this._FreeCount++;
				this._Version++;
				return true;
			}
			num3 = i;
		}
		return false;
	}

	private void Insert(TKey key, TValue value, bool add)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (this._Buckets == null)
		{
			this.Initialize(0);
		}
		int num = this._Comparer.GetHashCode(key) & int.MaxValue;
		int num2 = num % this._Buckets.Length;
		int num3 = 0;
		int i = this._Buckets[num2];
		while (i >= 0)
		{
			if (this._HashCodes[i] == num && this._Comparer.Equals(this._Keys[i], key))
			{
				if (add)
				{
					throw new ArgumentException("Key already exists: " + key);
				}
				this._Values[i] = value;
				this._Version++;
				return;
			}
			else
			{
				num3++;
				i = this._Next[i];
			}
		}
		int num4;
		if (this._FreeCount > 0)
		{
			num4 = this._FreeList;
			this._FreeList = this._Next[num4];
			this._FreeCount--;
		}
		else
		{
			if (this._Count == this._Keys.Length)
			{
				this.Resize();
				num2 = num % this._Buckets.Length;
			}
			num4 = this._Count;
			this._Count++;
		}
		this._HashCodes[num4] = num;
		this._Next[num4] = this._Buckets[num2];
		this._Keys[num4] = key;
		this._Values[num4] = value;
		this._Buckets[num2] = num4;
		this._Version++;
	}

	private void Initialize(int capacity)
	{
		int prime = SerializableDictionary<TKey, TValue>.PrimeHelper.GetPrime(capacity);
		this._Buckets = new int[prime];
		for (int i = 0; i < this._Buckets.Length; i++)
		{
			this._Buckets[i] = -1;
		}
		this._Keys = new TKey[prime];
		this._Values = new TValue[prime];
		this._HashCodes = new int[prime];
		this._Next = new int[prime];
		this._FreeList = -1;
	}

	private int FindIndex(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (this._Buckets != null)
		{
			int num = this._Comparer.GetHashCode(key) & int.MaxValue;
			for (int i = this._Buckets[num % this._Buckets.Length]; i >= 0; i = this._Next[i])
			{
				if (this._HashCodes[i] == num && this._Comparer.Equals(this._Keys[i], key))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		int num = this.FindIndex(key);
		if (num >= 0)
		{
			value = this._Values[num];
			return true;
		}
		value = default(TValue);
		return false;
	}

	public ICollection<TKey> Keys
	{
		get
		{
			return this._Keys.Take(this.Count).ToArray<TKey>();
		}
	}

	public ICollection<TValue> Values
	{
		get
		{
			return this._Values.Take(this.Count).ToArray<TValue>();
		}
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		this.Add(item.Key, item.Value);
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		int num = this.FindIndex(item.Key);
		return num >= 0 && EqualityComparer<TValue>.Default.Equals(this._Values[num], item.Value);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index > array.Length)
		{
			throw new ArgumentOutOfRangeException(string.Format("index = {0} array.Length = {1}", index, array.Length));
		}
		if (array.Length - index < this.Count)
		{
			throw new ArgumentException(string.Format("The number of elements in the dictionary ({0}) is greater than the available space from index to the end of the destination array {1}.", this.Count, array.Length));
		}
		for (int i = 0; i < this._Count; i++)
		{
			if (this._HashCodes[i] >= 0)
			{
				array[index++] = new KeyValuePair<TKey, TValue>(this._Keys[i], this._Values[i]);
			}
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return false;
		}
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return this.Remove(item.Key);
	}

	public SerializableDictionary<TKey, TValue>.Enumerator GetEnumerator()
	{
		return new SerializableDictionary<TKey, TValue>.Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	[SerializeField]
	[HideInInspector]
	private int[] _Buckets;

	[SerializeField]
	[HideInInspector]
	private int[] _HashCodes;

	[SerializeField]
	[HideInInspector]
	private int[] _Next;

	[SerializeField]
	[HideInInspector]
	private int _Count;

	[SerializeField]
	[HideInInspector]
	private int _Version;

	[SerializeField]
	[HideInInspector]
	private int _FreeList;

	[SerializeField]
	[HideInInspector]
	private int _FreeCount;

	[SerializeField]
	private TKey[] _Keys;

	[SerializeField]
	private TValue[] _Values;

	private readonly IEqualityComparer<TKey> _Comparer;

	private static class PrimeHelper
	{
		public static bool IsPrime(int candidate)
		{
			if ((candidate & 1) != 0)
			{
				int num = (int)Math.Sqrt((double)candidate);
				for (int i = 3; i <= num; i += 2)
				{
					if (candidate % i == 0)
					{
						return false;
					}
				}
				return true;
			}
			return candidate == 2;
		}

		public static int GetPrime(int min)
		{
			if (min < 0)
			{
				throw new ArgumentException("min < 0");
			}
			for (int i = 0; i < SerializableDictionary<TKey, TValue>.PrimeHelper.Primes.Length; i++)
			{
				int num = SerializableDictionary<TKey, TValue>.PrimeHelper.Primes[i];
				if (num >= min)
				{
					return num;
				}
			}
			for (int j = min | 1; j < 2147483647; j += 2)
			{
				if (SerializableDictionary<TKey, TValue>.PrimeHelper.IsPrime(j) && (j - 1) % 101 != 0)
				{
					return j;
				}
			}
			return min;
		}

		public static int ExpandPrime(int oldSize)
		{
			int num = 2 * oldSize;
			if (num > 2146435069 && 2146435069 > oldSize)
			{
				return 2146435069;
			}
			return SerializableDictionary<TKey, TValue>.PrimeHelper.GetPrime(num);
		}

		public static readonly int[] Primes = new int[]
		{
			3, 7, 11, 17, 23, 29, 37, 47, 59, 71,
			89, 107, 131, 163, 197, 239, 293, 353, 431, 521,
			631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
			4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
			25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
			156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
			968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
			5999471, 7199369
		};
	}

	public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
	{
		internal Enumerator(SerializableDictionary<TKey, TValue> dictionary)
		{
			this._Dictionary = dictionary;
			this._Version = dictionary._Version;
			this._Current = default(KeyValuePair<TKey, TValue>);
			this._Index = 0;
		}

		public KeyValuePair<TKey, TValue> Current
		{
			get
			{
				return this._Current;
			}
		}

		public bool MoveNext()
		{
			if (this._Version != this._Dictionary._Version)
			{
				throw new InvalidOperationException(string.Format("Enumerator version {0} != Dictionary version {1}", this._Version, this._Dictionary._Version));
			}
			while (this._Index < this._Dictionary._Count)
			{
				if (this._Dictionary._HashCodes[this._Index] >= 0)
				{
					this._Current = new KeyValuePair<TKey, TValue>(this._Dictionary._Keys[this._Index], this._Dictionary._Values[this._Index]);
					this._Index++;
					return true;
				}
				this._Index++;
			}
			this._Index = this._Dictionary._Count + 1;
			this._Current = default(KeyValuePair<TKey, TValue>);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (this._Version != this._Dictionary._Version)
			{
				throw new InvalidOperationException(string.Format("Enumerator version {0} != Dictionary version {1}", this._Version, this._Dictionary._Version));
			}
			this._Index = 0;
			this._Current = default(KeyValuePair<TKey, TValue>);
		}

		object IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}

		public void Dispose()
		{
		}

		private readonly SerializableDictionary<TKey, TValue> _Dictionary;

		private int _Version;

		private int _Index;

		private KeyValuePair<TKey, TValue> _Current;
	}
}
