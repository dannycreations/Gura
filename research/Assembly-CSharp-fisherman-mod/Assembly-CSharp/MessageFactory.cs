using System;
using System.Collections.Generic;

public static class MessageFactory
{
	public static Queue<InfoMessage> InfoMessagesQueue = new Queue<InfoMessage>();

	public static ListQueue<MessageBoxBase> MessageBoxQueue = new ListQueue<MessageBoxBase>();
}
