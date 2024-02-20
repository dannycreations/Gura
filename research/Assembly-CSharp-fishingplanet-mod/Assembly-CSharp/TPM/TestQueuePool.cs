using System;
using UnityEngine;

namespace TPM
{
	public class TestQueuePool : MonoBehaviour
	{
		private void Update()
		{
			int num = 0;
			if (Input.GetKeyUp(49))
			{
				num = 1;
			}
			else if (Input.GetKeyUp(50))
			{
				num = 2;
			}
			else if (Input.GetKeyUp(51))
			{
				num = 3;
			}
			else if (Input.GetKeyUp(52))
			{
				num = 4;
			}
			else if (Input.GetKeyUp(257))
			{
				num = -1;
			}
			else if (Input.GetKeyUp(258))
			{
				num = -2;
			}
			else if (Input.GetKeyUp(259))
			{
				num = -3;
			}
			else if (Input.GetKeyUp(260))
			{
				num = -4;
			}
			if (num == 0)
			{
				return;
			}
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					this._puter.Replace();
					this._pool.Enqueue(this._puter);
				}
			}
			else
			{
				for (int j = 0; j < -num; j++)
				{
					QueuePoolDataExample queuePoolDataExample = this._pool.Dequeue();
					if (!object.ReferenceEquals(queuePoolDataExample, null))
					{
					}
				}
			}
		}

		private QueuePool<QueuePoolDataExample> _pool = new QueuePool<QueuePoolDataExample>(5);

		private QueuePoolDataExample _puter = new QueuePoolDataExample();
	}
}
