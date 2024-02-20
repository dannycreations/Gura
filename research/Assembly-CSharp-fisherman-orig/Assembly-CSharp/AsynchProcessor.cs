using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class AsynchProcessor
{
	public static void ProcessByteArrayWorkItem(InputByteArrayWorkItem inputWorkItem)
	{
		new Thread(delegate
		{
			QueueWorkItem queueWorkItem = null;
			try
			{
				queueWorkItem = new QueueWorkItem
				{
					Data = inputWorkItem.ProcessAction(inputWorkItem.Data),
					ResultAction = inputWorkItem.ResultAction
				};
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Item cannot be processed asynchroniously: [{0}]. \n Stack trace : [{1}]", new object[] { ex.Message, ex.StackTrace });
			}
			if (queueWorkItem != null)
			{
				object obj = AsynchProcessor.queue_lock;
				lock (obj)
				{
					AsynchProcessor.queueWorkItem.Enqueue(queueWorkItem);
				}
			}
		}).Start();
	}

	public static void ProcessDictionaryWorkItem(InputDictionaryWorkItem inputWorkItem)
	{
		new Thread(delegate
		{
			QueueWorkItem queueWorkItem = null;
			try
			{
				queueWorkItem = new QueueWorkItem
				{
					Data = inputWorkItem.ProcessAction(inputWorkItem.Data),
					ResultAction = inputWorkItem.ResultAction
				};
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Item cannot be processed asynchroniously: [{0}]. \n Stack trace : [{1}]", new object[] { ex.Message, ex.StackTrace });
				if (inputWorkItem.ErrorAction != null)
				{
					inputWorkItem.ErrorAction();
				}
			}
			if (queueWorkItem != null)
			{
				object obj = AsynchProcessor.queue_lock;
				lock (obj)
				{
					AsynchProcessor.queueWorkItem.Enqueue(queueWorkItem);
				}
			}
		}).Start();
	}

	public static void ProcessByteWorkItem(InputByteWorkItem inputWorkItem)
	{
		new Thread(delegate
		{
			QueueWorkItem queueWorkItem = null;
			try
			{
				queueWorkItem = new QueueWorkItem
				{
					Data = inputWorkItem.ProcessAction(inputWorkItem.Data),
					ResultAction = inputWorkItem.ResultAction
				};
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Item cannot be processed asynchroniously: [{0}]. \n Stack trace : [{1}]", new object[] { ex.Message, ex.StackTrace });
			}
			if (queueWorkItem != null)
			{
				object obj = AsynchProcessor.queue_lock;
				lock (obj)
				{
					AsynchProcessor.queueWorkItem.Enqueue(queueWorkItem);
				}
			}
		}).Start();
	}

	public static void ReleaseQueue()
	{
		for (;;)
		{
			QueueWorkItem queueWorkItem = null;
			object obj = AsynchProcessor.queue_lock;
			lock (obj)
			{
				if (AsynchProcessor.queueWorkItem.Count > 0)
				{
					queueWorkItem = AsynchProcessor.queueWorkItem.Dequeue();
				}
			}
			if (queueWorkItem == null)
			{
				break;
			}
			queueWorkItem.ResultAction(queueWorkItem.Data);
		}
	}

	private static readonly Queue<QueueWorkItem> queueWorkItem = new Queue<QueueWorkItem>();

	private static readonly object queue_lock = new object();
}
