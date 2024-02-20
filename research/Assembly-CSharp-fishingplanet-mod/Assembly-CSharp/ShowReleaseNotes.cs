using System;
using I2.Loc;
using UnityEngine;

public class ShowReleaseNotes : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (!PlayerPrefs.HasKey("RN0.2.14"))
		{
			PlayerPrefs.SetInt("RN0.2.14", 0);
			string text = string.Empty;
			string currentLanguage = LocalizationManager.CurrentLanguage;
			if (currentLanguage != null)
			{
				if (currentLanguage == "English")
				{
					text = this._english;
					goto IL_E5;
				}
				if (currentLanguage == "Russian")
				{
					text = this._russian;
					goto IL_E5;
				}
				if (currentLanguage == "German (Germany)")
				{
					text = this._english;
					goto IL_E5;
				}
				if (currentLanguage == "French")
				{
					text = this._english;
					goto IL_E5;
				}
				if (currentLanguage == "Polish")
				{
					text = this._english;
					goto IL_E5;
				}
				if (currentLanguage == "Ukrainian")
				{
					text = this._russian;
					goto IL_E5;
				}
			}
			text = this._english;
			IL_E5:
			this.ShowingReleaseNotes(text);
		}
	}

	public GameObject ShowingReleaseNotes(string message)
	{
		this.messageBox = GUITools.AddChild(base.gameObject, base.GetComponent<MessageBoxList>().messageReleaseNotesPrefab);
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.messageBox.GetComponent<MessageBox>().Message = message;
		this.messageBox.GetComponent<MessageBox>().SetActiveCancelButton = true;
		this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.ShowReleaseNotes_CancelActionCalled;
		return this.messageBox;
	}

	private void ShowReleaseNotes_CancelActionCalled(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.GetComponent<MessageBox>().Close();
		}
	}

	private GameObject messageBox;

	private string _english = "<b>Update 0.2.14 brings exciting changes you're bound to enjoy. Check them out:</b>\n\n<b>NEW WATERWAY ADDED</b>\n   Lake Saint Croix\n\n<b>NEW TACKLE ADDED:</b>\n\n<b>Reels Added:</b>\n   Exterminator Reel\n   EspiraML R Reel\n   CallistoXS R Reel\n\n<b>Rods Added:</b>\n   Rivertex GalaxyCast (Rivertex) - casting rod added\n   Rivertex Thora (Rivertex) - spinning rod added\n   Rivertex Fenix (Rivertex) - match rod added\n   Garry Scott Troy (Garry Scott) – telescope rod added\n\n<b>New Baits added:</b>\n   Two new baits added - Mayflies and Shiners (both are analogues of golden baits like Minnows, Shrimps, Spawnsacks)\n\n<b>New Lures added:</b>\n   4 bigger spinners\n   4 bigger medium spoons\n   8 shads added\n   16 worms added\n   11 Jig-Heads added\n   Crankbaits added\n\n<b>New tackle and equipment added:</b>\n   New Vest, Tacklebox, Rodcase, Stringer and Fish cage added, all for silver  (analogues of big gold-priced ones)\n\n<b>New Fish Species Added:</b>\n   Muskellunge, \n   Brook Trout, \n   Rock Bass, \n   Blue Catfish,\n   Lake Sturgeon\n   Brown Bullhead \n\n<b>New Functionality added:</b>\n   Ability to visit profile page of another player added in Leaderboards section - just click players nickname\n   Indication of purchased items added to shop, additionally indication shows where these items are\n   Notification/warning about license possession added when travelling to Waterway\n   Descriptions of Licenses structured and enhanced\n   Drag and speed settings are now saved on specific reel when reel is removed\n\n<b>Bugs fixed:</b>\n   Fish escape bug fixed and logic enhanced\n   Scrolling in inventory and shop fixed\n   Brook Trout animations fixed\n   Licensed duration in DLC fixed\n   Bass fighter-pack DLC improved with bass-jigs with smaller hook-sized to make it of value on earlier levels\n   Profanity filter fixed and improved\n   Fish crazy spinning when in hands fixed\n   Game messages are now shown in correct order\n\n<b>Gameplay:</b>\n   Lure and bait attractivity tuning for certain fish species (will be specified later)   Minor tackle and equipment relevelling";

	private string _ukrainian = "Release notes";

	private string _french = "Release notes";

	private string _german = "Release notes";

	private string _polish = "Release notes";

	private string _russian = "<b>Обновление 0.2.14 добавит еще больше азарта и удовольствия от игры. Детали обновления:</b>\n\n<b>ДОБАВЛЕН ВОДОЕМ</b>\n   Мичиган, Озеро Сент Круа \n\n<b>ДОБАВЛЕНЫ НОВЫЕ СНАСТИ:</b> \n<b>Катушки:</b>\n   Катушка Exterminator (Rivertex)\n   Катушка EspiraML R\n   Катушка CallistoXS R\n\n<b>Удочки</b> \n   Удилище GalaxyCast (Rivertex) - кастинговое\n   Удилище Thora (Rivertex) - спиннинговое\n   Удилище Fenix (Rivertex) - матчевое\n   Удилище Troy (Garry Scott) – телескопическое\n\n<b>Добавлены новые наживки:</b>\n   Добавлнеы две новые наживки для хищников Подёнки и Шайнеры (аналоги золотых блесен)\n\n<b>Добавлены новые приманки:</b>\n   Четыре больших спиннера\n   Четыре большие колебалки\n   Восемь силиконок\n   16 Силиконовых червей\n   11 Джиг-головок\n   Добавлены Воблеры\n\n<b>Добавлены предметы и снаряжение:</b>\n   Добавлены новые жилетка, ящики для, чехол, кукан и садок (аналогичные золотым)\n\n<b>ДОБАВЛЕНЫ НОВЫЕ РЫБЫ:</b>\n   Щука маскинонг\n   Рок Басс\n   Красноглазый Каменный Окунь\n   Американский Голец\n   Озерный Осетр\n   Голубой сом\n   Коричневый Сомик\n\n<b>Добавлен функционал:</b>\n   Просмотр профиля другого игрока в Топах игроков при нажатии на имя игрока\n   В магазине отображаются уже купленные вещи и где они находоятся (в рюкзаке или дома)\n   При поездке на водоем появилось напоминение об отсутствии необходимой лицензии\n   Реструктурированы и изменены описания лицензий\n   Настройки фрикциона и скорости подмотки теперь сохраняются даже если катушке деэкипируется\n\n<b>Исправленные баги:</b>\n   Исправлена логика сходов рыбы и ликвидирован таймер\n   Исправлена работа скроллера\n   Исправлены проблемы с анимацей рыбы Американский Голец\n   Исправлена логика начисления длительности лицензий при покупке всех DLC\n   В DLC басс файтер пак добавлены басс-джиги с меньшим размером крючка для актуализации его для ранних водоемов\n   Исправлены ошибки и неточности в работе фильтра матерной речи\n   Исправлен баг с вращением рыбы, когда она висит в руках\n   Исправлена очередность выведения ряда сообщений\n\n<b>Геймплей:</b>\n   Изменена привлекательность части приманок для определенных видов рыб\n   Незначительные изменения в распределении снастей и снаряжения по уровням\n";
}
