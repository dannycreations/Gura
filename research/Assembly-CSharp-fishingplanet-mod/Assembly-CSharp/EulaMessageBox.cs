using System;
using TMPro;

public class EulaMessageBox : MessageBoxBase
{
	protected override void Awake()
	{
		base.Awake();
		MessageFactory.MessageBoxQueue.Enqueue(this);
	}

	public TextMeshProUGUI TopText;

	public TextMeshProUGUI BottomText;

	public GamePadIconTextAdder LinkText;
}
