using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPlayersInit : MonoBehaviour
{
	internal void Start()
	{
		this._scrollbar = this.ContentPanel.transform.parent.Find("Scrollbar").gameObject.GetComponent<Scrollbar>();
		this.TglGlobal.onValueChanged.AddListener(delegate(bool t)
		{
			if (t)
			{
				this.RefreshMain();
			}
		});
		this.TglLocal.onValueChanged.AddListener(delegate(bool t)
		{
			if (t)
			{
				this.RefreshMain();
			}
		});
		this.TglCompetition.onValueChanged.AddListener(delegate(bool t)
		{
			if (t)
			{
				this.RefreshCompetition();
			}
		});
		if (StaticUserData.CurrentPond == null)
		{
			this.TglLocal.gameObject.SetActive(false);
		}
	}

	internal void Refresh()
	{
		if (this.TglCompetition.isOn)
		{
			this.RefreshCompetition();
		}
		else
		{
			this.RefreshMain();
		}
	}

	public void RefreshMain()
	{
		if (!this.CommonLeaderboardPanel.IsShow)
		{
			this.TournamentLeaderboardPanel.GetComponent<AlphaFade>().HidePanel();
			this.CommonLeaderboardPanel.ShowPanel();
		}
		this.LoadingText.SetActive(true);
		this.ContentPanel.GetComponent<CanvasGroup>().alpha = 0f;
		if (this.TglLocal.isOn)
		{
		}
	}

	public void RefreshCompetition()
	{
		if (!this.TournamentLeaderboardPanel.GetComponent<AlphaFade>().IsShow)
		{
			this.TournamentLeaderboardPanel.GetComponent<AlphaFade>().ShowPanel();
			this.CommonLeaderboardPanel.HidePanel();
		}
	}

	private void OnGotLeaderboards(TopLeadersResult result)
	{
		PhotonConnectionFactory.Instance.OnGotLeaderboards -= this.OnGotLeaderboards;
		PhotonConnectionFactory.Instance.OnGettingLeaderboardsFailed -= this.OnGettingLeaderboardsFailed;
		if (result.Kind == 1)
		{
			this.Clear();
			this._unfilledResult = new Queue<TopPlayers>(result.Players.ToList<TopPlayers>());
		}
	}

	private void OnGettingLeaderboardsFailed(Failure failure)
	{
	}

	private void FillData(TopLeadersResult result)
	{
		if (result.Players != null)
		{
		}
		bool flag = false;
		int num = 0;
		if (result.Players != null)
		{
			int num2 = 0;
			foreach (TopPlayers topPlayers in result.Players)
			{
				GameObject gameObject = this.ContentPanel.transform.GetChild(num2).gameObject;
				gameObject.SetActive(true);
				if (num % 2 != 0)
				{
					gameObject.GetComponent<Image>().enabled = true;
				}
				gameObject.GetComponent<ConcreteTopPlayer>().InitItem(topPlayers, num + 1, flag);
				num++;
				num2++;
			}
		}
		this.LoadingText.SetActive(false);
	}

	private void FillPlayer(TopPlayers player, bool weeklyExp, int counter)
	{
		GameObject gameObject = this.ContentPanel.transform.GetChild(counter).gameObject;
		gameObject.SetActive(true);
		gameObject.GetComponent<ConcreteTopPlayer>().InitItem(player, counter + 1, weeklyExp);
	}

	private void DisableLayoutGroup()
	{
		this.ContentPanel.GetComponent<ContentSizeFitter>().enabled = false;
		this.ContentPanel.GetComponent<VerticalLayoutGroup>().enabled = false;
		this._scrollbar.value = 1f;
	}

	private void SetSizeForContent(float height)
	{
		RectTransform component = this.ContentPanel.GetComponent<RectTransform>();
		component.anchorMax = new Vector2(0.5f, 1f);
		component.anchorMin = new Vector2(0.5f, 1f);
		component.pivot = new Vector2(0.5f, 0.5f);
		component.sizeDelta = new Vector2(component.sizeDelta.x, Mathf.Max(height, this.ContentPanel.transform.parent.GetComponent<RectTransform>().rect.height));
		component.anchoredPosition = new Vector3(0f, 0f - height / 2f, 0f);
		GameObject gameObject = this.ContentPanel.transform.parent.Find("Scrollbar").gameObject;
		if (component.sizeDelta.y > this.ContentPanel.transform.parent.GetComponent<RectTransform>().rect.height)
		{
			gameObject.gameObject.SetActive(true);
			gameObject.GetComponent<Scrollbar>().value = 1f;
		}
		else
		{
			gameObject.gameObject.SetActive(false);
		}
	}

	public void Clear()
	{
		if (this.ContentPanel != null)
		{
			for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
			{
				this.ContentPanel.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
	}

	private void Sorting_ChangedValue(object sender, EventArgs e)
	{
		this.RefreshMain();
	}

	private void Update()
	{
		if (this._unfilledResult.Count > 0)
		{
			bool flag = false;
			for (int i = 0; i < 10; i++)
			{
				if (this._unfilledResult.Count > 0)
				{
					this.FillPlayer(this._unfilledResult.Dequeue(), flag, this._counter);
					this._counter++;
				}
			}
			if (this._unfilledResult.Count == 0)
			{
				this.LoadingText.SetActive(false);
				this.ContentPanel.GetComponent<CanvasGroup>().alpha = 1f;
				this._counter = 0;
				base.Invoke("DisableLayoutGroup", 0.25f);
			}
		}
	}

	public GameObject PlayerItemPrefab;

	public GameObject ContentPanel;

	public Toggle TglGlobal;

	public Toggle TglLocal;

	public Toggle TglCompetition;

	public AlphaFade CommonLeaderboardPanel;

	public LeaderboardCompetitionHandler TournamentLeaderboardPanel;

	public GameObject LoadingText;

	public Text CaptionExp;

	private Queue<TopPlayers> _unfilledResult = new Queue<TopPlayers>();

	private int _counter;

	private Scrollbar _scrollbar;
}
