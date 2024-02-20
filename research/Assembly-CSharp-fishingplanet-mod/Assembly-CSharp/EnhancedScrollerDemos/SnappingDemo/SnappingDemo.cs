using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.SnappingDemo
{
	public class SnappingDemo : MonoBehaviour
	{
		private int Credits
		{
			get
			{
				return this._credits;
			}
			set
			{
				this._credits = ((value >= 0) ? value : 0);
				this.creditsText.text = string.Format("{0:n0}", this._credits);
				this.pullLeverButton.gameObject.SetActive(this._credits > 0);
			}
		}

		private SnappingDemo.GameStateEnum GameState
		{
			get
			{
				return this._gameState;
			}
			set
			{
				this._gameState = value;
				SnappingDemo.GameStateEnum gameState = this._gameState;
				if (gameState != SnappingDemo.GameStateEnum.Playing)
				{
					if (gameState == SnappingDemo.GameStateEnum.GameOver)
					{
						this.playingPanel.SetActive(false);
						this.gameOverPanel.SetActive(true);
					}
				}
				else
				{
					foreach (SlotController slotController in this._slotControllers)
					{
						slotController.scroller.snapping = true;
					}
					this.Credits = this.startingCredits;
					this.playingPanel.SetActive(true);
					this.gameOverPanel.SetActive(false);
				}
			}
		}

		private void Awake()
		{
			this.GameState = SnappingDemo.GameStateEnum.Initializing;
			this._slotControllers = base.gameObject.GetComponentsInChildren<SlotController>();
			this._snappedDataIndices = new int[this._slotControllers.Length];
			foreach (SlotController slotController in this._slotControllers)
			{
				slotController.scroller.scrollerSnapped = new ScrollerSnappedDelegate(this.ScrollerSnapped);
			}
		}

		private void Start()
		{
			foreach (SlotController slotController in this._slotControllers)
			{
				slotController.Reload(this.slotSprites);
			}
		}

		private void LateUpdate()
		{
			if (this.GameState == SnappingDemo.GameStateEnum.Initializing)
			{
				this.GameState = SnappingDemo.GameStateEnum.Playing;
			}
		}

		public void PullLeverButton_OnClick()
		{
			this._snapCount = 0;
			this.Credits--;
			this.pullLeverButton.interactable = false;
			foreach (SlotController slotController in this._slotControllers)
			{
				slotController.AddVelocity(((Random.Range(0f, 1f) <= 0.5f) ? (-1f) : 1f) * Random.Range(this.minVelocity, this.maxVelocity));
			}
		}

		public void ResetButton_OnClick()
		{
			this.GameState = SnappingDemo.GameStateEnum.Playing;
		}

		private void ScrollerSnapped(EnhancedScroller scroller, int cellIndex, int dataIndex, EnhancedScrollerCellView cellView)
		{
			if (this.GameState != SnappingDemo.GameStateEnum.Playing)
			{
				return;
			}
			this._snapCount++;
			this._snappedDataIndices[this._snapCount - 1] = dataIndex;
			if (this._snapCount == this._slotControllers.Length)
			{
				this.TallyScore();
				this.pullLeverButton.interactable = true;
			}
			if (this.Credits == 0)
			{
				this.GameState = SnappingDemo.GameStateEnum.GameOver;
			}
		}

		private void TallyScore()
		{
			this._snapCount = 0;
			int num = 0;
			int num2 = this._snappedDataIndices[0];
			int num3 = this._snappedDataIndices[1];
			int num4 = this._snappedDataIndices[2];
			if (num2 == this.blankIndex || num3 == this.blankIndex || num4 == this.blankIndex)
			{
				num = 0;
			}
			else if (num2 == num3 && num2 == num4)
			{
				if (num2 == this.sevenIndex)
				{
					num = 1000;
				}
				else if (num2 == this.bigWinIndex)
				{
					num = 150;
				}
				else if (num2 == this.tripleBarIndex)
				{
					num = 70;
				}
				else if (num2 == this.cherryIndex)
				{
					num = 40;
				}
				else
				{
					num = 20;
				}
			}
			else if (num2 == this.cherryIndex || num3 == this.cherryIndex || num4 == this.cherryIndex)
			{
				num = 3;
			}
			if (num > 0)
			{
				this.Credits += num;
				this.playWin.Play(num);
			}
		}

		private SlotController[] _slotControllers;

		private int[] _snappedDataIndices;

		private int _credits;

		private int _snapCount;

		private SnappingDemo.GameStateEnum _gameState;

		public float minVelocity;

		public float maxVelocity;

		public int cherryIndex;

		public int sevenIndex;

		public int tripleBarIndex;

		public int bigWinIndex;

		public int blankIndex;

		public Sprite[] slotSprites;

		public Button pullLeverButton;

		public Text creditsText;

		public int startingCredits;

		public GameObject playingPanel;

		public GameObject gameOverPanel;

		public PlayWin playWin;

		private enum GameStateEnum
		{
			Initializing,
			Playing,
			GameOver
		}
	}
}
