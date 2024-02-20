using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public static class AbusiveWords
{
	public static string ReplaceAbusiveWords(string message)
	{
		for (int i = 0; i < AbusiveWords.abusiveWordsWithPunctuation.Length; i++)
		{
			string text = AbusiveWords.abusiveWordsWithPunctuation[i];
			message = Regex.Replace(message, text, "***", RegexOptions.IgnoreCase);
		}
		IList<string> list = AbusiveWords.SplitIntoWords(message);
		bool flag = false;
		for (int j = 0; j < list.Count; j++)
		{
			string text2 = list[j];
			for (int k = 0; k < AbusiveWords.abusiveWholeWords.Length; k++)
			{
				string text3 = AbusiveWords.abusiveWholeWords[k];
				if (string.Equals(text2, text3, StringComparison.OrdinalIgnoreCase))
				{
					list[j] = "***";
					flag = true;
					break;
				}
			}
			for (int l = 0; l < AbusiveWords.abusiveWords.Length; l++)
			{
				string text4 = AbusiveWords.abusiveWords[l];
				if (AbusiveWords.CheckWord(text2, text4))
				{
					list[j] = "***";
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int m = 0; m < list.Count; m++)
			{
				stringBuilder.Append(list[m]);
			}
			return stringBuilder.ToString();
		}
		return message;
	}

	public static bool HasAbusiveWords(string message)
	{
		for (int i = 0; i < AbusiveWords.abusiveWordsWithPunctuation.Length; i++)
		{
			string text = AbusiveWords.abusiveWordsWithPunctuation[i];
			if (Regex.Match(message, text, RegexOptions.IgnoreCase).Success)
			{
				return true;
			}
		}
		IList<string> list = AbusiveWords.SplitIntoWords(message);
		for (int j = 0; j < list.Count; j++)
		{
			string text2 = list[j];
			for (int k = 0; k < AbusiveWords.abusiveWholeWords.Length; k++)
			{
				string text3 = AbusiveWords.abusiveWholeWords[k];
				if (string.Equals(text2, text3, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			for (int l = 0; l < AbusiveWords.abusiveWords.Length; l++)
			{
				string text4 = AbusiveWords.abusiveWords[l];
				if (AbusiveWords.CheckWord(text2, text4))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool CheckWord(string word, string abusiveWord)
	{
		return word.StartsWith(abusiveWord, StringComparison.OrdinalIgnoreCase);
	}

	private static IList<string> SplitIntoWords(string str)
	{
		List<string> list = new List<string>();
		bool flag = false;
		int num = 0;
		for (int i = 0; i < str.Length; i++)
		{
			if (char.IsLetterOrDigit(str, i))
			{
				if (!flag)
				{
					flag = true;
					if (i > num)
					{
						list.Add(str.Substring(num, i - num));
						num = i;
					}
				}
			}
			else if (flag)
			{
				flag = false;
				if (i > num)
				{
					list.Add(str.Substring(num, i - num));
					num = i;
				}
			}
		}
		list.Add(str.Substring(num, str.Length - num));
		return list;
	}

	private const string Replacement = "***";

	private static string[] abusiveWordsWithPunctuation = new string[]
	{
		"\\ba_s_s\\b", "\\bb!tch\\b", "\\bbi+ch\\b", "\\bf_u_c_k\\b", "\\bjack-off\\b", "\\bjerk-off\\b", "\\bl3i+ch\\b", "\\bmo-fo\\b", "\\bs.o.b.\\b", "\\bsh!+\\b",
		"\\bsh!t\\b", "\\bshi+\\b", "\\bson-of-a-bitch\\b", "\\bs_h_i_t\\b", "\\bcock-sucker\\b", "\\bgod-dam\\b", "\\bgod-damned\\b", "\\bmaster-bate\\b", "\\b8=D\\b", "\\bbin laden\\b",
		"\\bChing chong\\b", "\\bChingada Madre\\b", "\\bcoon hunt\\b", "\\bcoon kill\\b", "\\bcotton pic\\b", "\\bcotton pik\\b", "\\bDeez Nuts\\b", "\\bDio Bestia\\b", "\\bDurka durka\\b", "\\bFiglio di Puttana\\b",
		"\\bgod damn\\b", "\\bJack Off\\b", "\\bJungle Bunny\\b", "\\bk k k\\b", "\\bklu klux\\b", "\\bKlu Klux Klan\\b", "\\bkoon hunt\\b", "\\bkoon kill\\b", "\\bku klux klan\\b", "\\bMadonna Puttana\\b",
		"\\bMoon Cricket\\b", "\\bPorca Madonna\\b", "\\bPorch monkey\\b", "\\bRed Tube\\b", "\\bsofa king\\b", "\\bSucchia Cazzi\\b", "\\bsuck my\\b", "\\btea bag\\b", "\\bTesta di Cazzo\\b", "\\bwhite power\\b",
		"\\bxbl pet\\b"
	};

	private static string[] abusiveWholeWords = new string[]
	{
		"anal", "arse", "ass", "asses", "balls", "butt", "bum", "cock", "crap", "cum",
		"coon", "fag", "god", "hell", "homo", "jap", "muff", "nob", "spac", "tit",
		"wtf", "hitler", "cocu", "con", "cul", "fion", "pd", "pede", "pine", "pute",
		"tare", "zob", "бля", "ебя", "cipa", "cyc", "dupa", "huj", "loda", "syf"
	};

	private static string[] abusiveWords = new string[]
	{
		"4r5e", "5h1t", "5hit", "a55", "anus", "ar5e", "arrse", "assfucker", "assfukka", "asshole",
		"asswhole", "b00bs", "b17ch", "b1tch", "ballbag", "ballsack", "bastard", "beastial", "beastiality", "bellend",
		"bestial", "bestiality", "biatch", "bitch", "bloody", "blowjob", "blowjobs", "boiolas", "bollock", "bollok",
		"boner", "boob", "boobs", "booobs", "boooobs", "booooobs", "booooooobs", "breasts", "buceta", "bugger",
		"bunnyfucker", "butthole", "buttmuch", "buttplug", "c0ck", "c0cksucker", "carpetmuncher", "cawk", "chink", "cipa",
		"cl1t", "clit", "clitoris", "clits", "cnut", "cockface", "cockhead", "cockmunch", "cockmuncher", "cocks",
		"cocksuck", "cocksuka", "cocksukka", "cok", "cokmuncher", "coksucka", "cox", "cummer", "cumming", "cums",
		"cumshot", "cunilingus", "cunillingus", "cunnilingus", "cunt", "cyalis", "cyberfuc", "d1ck", "damn", "dick",
		"dildo", "dink", "dinks", "dirsa", "dlck", "doggin", "donkeyribber", "doosh", "duche", "dyke",
		"ejaculate", "ejakulate", "fuck", "f4nny", "fagging", "faggitt", "faggot", "faggs", "fagot", "fagots",
		"fags", "fanny", "fanyy", "fatass", "fcuk", "fcuker", "fcuking", "feck", "fecker", "felching",
		"fellate", "fellatio", "fingerfuck", "fistfuck", "flange", "fook", "fooker", "fuck", "fudgepacker", "fudgepacker",
		"fuk", "fux", "gangbang", "gangbanged", "gangbangs", "gaylord", "gaysex", "goatse", "goddamn", "heshe",
		"hoar", "hoare", "hoer", "hore", "horniest", "horny", "hotsex", "hitler", "jackoff", "jism",
		"jiz", "jizm", "jizz", "kawk", "kkk", "knob", "knobead", "knobed", "knobend", "knobhead",
		"knobjocky", "knobjokey", "kock", "kondum", "kondums", "kum", "kummer", "kumming", "kums", "kunilingus",
		"l3itch", "labia", "lmfao", "lust", "lusting", "m0f0", "m0fo", "m45terbate", "ma5terb8", "ma5terbate",
		"masochist", "masterb8", "masterbat*", "masterbat3", "masterbate", "masterbation", "masterbations", "masturbate", "mof0", "mofo",
		"mothafuck", "motherfuck", "mutha", "muthafecker", "muthafuckker", "muther", "mutherfucker", "n1gga", "n1gger", "nazi",
		"nigg3r", "nigg4h", "nigga", "niggah", "niggas", "niggaz", "nigger", "niggers", "nobjokey", "nobhead",
		"nobjocky", "nobjokey", "numbnuts", "nutsack", "orgasim", "orgasm", "p0rn", "pawn", "pecker", "penis",
		"phuck", "phuk", "phuq", "pigfucker", "pimpis", "piss", "poop", "porn", "prick", "pron",
		"pube", "pusse", "pussi", "pussies", "pussy", "rectum", "retard", "rimjaw", "rimming", "sadist",
		"schlong", "screwing", "scroat", "scrote", "scrotum", "semen", "sex", "sh1t", "shag", "shagger",
		"shaggin", "shagging", "shemale", "shit", "skank", "slut", "sluts", "smegma", "smut", "snatch",
		"spunk", "t1tt1e5", "t1tties", "teets", "teez", "testical", "testicle", "titfuck", "tits", "titt",
		"titwank", "tosser", "turd", "tw4t", "twat", "twathead", "twatty", "twunt", "twunter", "v14gra",
		"v1gra", "vagina", "viagra", "vulva", "w00se", "wang", "wank", "wanker", "wanky", "whoar",
		"whore", "willies", "willy", "xrated", "блеат", "блеад", "бляд", "блят", "бляя", "ёб",
		"еба", "ёба", "ебе", "ёбе", "еби", "ёби", "ебк", "ёбк", "ебл", "ёбл",
		"ебн", "ёбн", "ебо", "ёбо", "ебс", "ёбс", "ебт", "ёбт", "ебу", "ёбу",
		"ёбы", "ебы", "ебю", "ёбю", "ёбя", "ё6", "е6а", "ё6а", "е6е", "ё6е",
		"е6и", "ё6и", "е6к", "ё6к", "е6л", "ё6л", "е6н", "ё6н", "е6о", "ё6о",
		"е6с", "ё6с", "е6т", "ё6т", "е6у", "ё6у", "ё6ы", "е6ы", "е6ю", "ё6ю",
		"ё6я", "долбоеб", "дуроуеб", "пизд", "пезд", "пида", "хуе", "хуё", "хуи", "хуй",
		"хул", "хуя", "хую", "уйн", "гомосек", "жоп", "манда", "мандо", "мокрощелка", "мудо",
		"срать", "срал", "сран", "сук", "суч", "трах", "гондо", "член", "шлюх", "abrutie",
		"baise", "baisé", "baiser", "batard", "bougnoul", "branleur", "burne", "chier", "connard", "connasse",
		"conne", "couille", "couillon", "couillonne", "crevard", "encule", "enculé", "enculee", "enculée", "enculer",
		"enfoire", "enfoiré", "foutre", "merde", "negre", "nègre", "negresse", "négresse", "nique", "niquer",
		"partouze", "pédé", "petasse", "pétasse", "pouffe", "pouffiasse", "putain", "salaud", "salop", "salopard",
		"salope", "sodomie", "sucer", "tapette", "taré", "vagin", "analritter", "arsch", "arschficker", "arschlecker",
		"arschloch", "bimbo", "bratze", "bumsen", "bonze", "dödel", "fick", "ficken", "flittchen", "fratze",
		"hackfresse", "hure", "hurensohn", "ische", "kackbratze", "kacke", "kacken", "kackwurst", "kampflesbe", "kanake",
		"kimme", "lümmel", "milf", "möpse", "morgenlatte", "möse", "mufti", "muschi", "nackt", "neger",
		"nigger", "nippel", "nutte", "onanieren", "orgasmus", "pimmel", "pimpern", "pinkeln", "pissen", "pisser",
		"popel", "poppen", "porno", "reudig", "rosette", "schabracke", "scheiße", "schlampe", "schnackeln", "schwanzlutscher",
		"schwuchtel", "tittchen", "titten", "vögeln", "vollpfosten", "wichse", "wichsen", "wichser", "burdel", "burdelmama",
		"chuj", "chujnia", "ciota", "debil", "dmuchać", "nędzy", "dupek", "duperele", "dziwka", "fiut",
		"gówno", "prawda", "jajco", "jajeczko", "jajko", "jajo", "pierdolę", "jebać", "jebany", "kurwa",
		"kurwy", "kutafon", "kutas", "lizać", "pałę", "obciągać", "chuja", "fiuta", "pieprzyć", "pierdolec",
		"pierdolić", "pierdolnięty", "pierdoła", "pierdzieć", "pizda", "pojeb", "popierdolony", "robic", "robić", "ruchać",
		"rzygać", "skurwysyn", "sraczka", "srać", "suka", "wkurwiać", "zajebisty", "1488", "A55hole", "abortion",
		"ahole", "AIDs", "ainujin", "ainuzin", "akimekura", "Anal", "anus", "anuses", "Anushead", "anuslick",
		"anuss", "aokan", "Arsch", "Arschloch", "arse", "arsed", "arsehole", "arseholed", "arseholes", "arseholing",
		"arselicker", "arses", "Ass", "asshat", "asshole", "Auschwitz", "b00bz", "b1tc", "Baise", "bakachon",
		"bakatyon", "Ballsack", "BAMF", "Bastard", "Beaner", "Beeatch", "beeeyotch", "beefwhistle", "beeotch", "Beetch",
		"beeyotch", "Bellend", "bestiality", "beyitch", "beyotch", "Biach", "binladen", "biotch", "bitch", "Bitching",
		"blad", "bladt", "blowjob", "blowme", "blyad", "blyadt", "bon3r", "boner", "boobs", "Btch",
		"Bukakke", "Bullshit", "bung", "butagorosi", "butthead", "Butthole", "Buttplug", "c0ck", "Cabron", "Cacca",
		"Cadela", "Cagada", "Cameljockey", "Caralho", "castrate", "Cazzo", "ceemen", "ch1nk", "chankoro", "chieokure",
		"chikusatsu", "Chinga", "Chingado", "Chingate", "chink", "chinpo", "Chlamydia", "choad", "chode", "chonga",
		"chonko", "chonkoro", "chourimbo", "chourinbo", "chourippo", "chuurembo", "chuurenbo", "circlejerk", "cl1t", "cli7",
		"clit", "clitoris", "cocain", "Cocaine", "cock", "Cocksucker", "Coglione", "Coglioni", "coitus", "coituss",
		"cojelon", "cojones", "condom", "coon", "coonhunt", "coonkill", "Cooter", "cottonpic", "cottonpik", "Crackhead",
		"CSAM", "Culear", "Culero", "Culo", "Cum", "cun7", "cunt", "cvn7", "cvnt", "cyka",
		"d1kc", "d4go", "dago", "Darkie", "deeznut", "deeznuts", "Dickhead", "dikc", "dildo", "dong",
		"dongs", "douche", "Downie", "Downy", "Dumbass", "Dyke", "Ejaculate", "Encule", "enjokousai", "enzyokousai",
		"etahinin", "etambo", "etanbo", "f0ck", "f0kc", "f3lch", "facking", "fag", "faggot", "Fanculo",
		"Fanny", "fatass", "fck", "Fckn", "fcuk", "fcuuk", "felch", "Fetish", "Fgt", "Fick",
		"FiCKDiCH", "fku", "fock", "fokc", "foreskin", "Fotze", "Foutre", "fucc", "fuck", "Fucking",
		"fuct", "fujinoyamai", "fukashokumin", "Fupa", "fuuck", "fuuuck", "fuuuuck", "fuuuuuck", "fuuuuuuck", "fuuuuuuuck",
		"fuuuuuuuuck", "fuuuuuuuuuck", "fuuuuuuuuuu", "fvck", "fxck", "fxuxcxk", "g000k", "g00k", "g0ok", "gestapo",
		"go0k", "goldenshowers", "golliwogg", "gollywog", "Gooch", "gook", "goook", "Gyp", "h0m0", "h0mo",
		"h1tl3", "h1tle", "hairpie", "hakujakusha", "hakuroubyo", "hakuzyakusya", "hantoujin", "hantouzin", "Herpes", "hitl3r",
		"hitler", "hitlr", "holocaust", "hom0", "homo", "honky", "Hooker", "hor3", "hukasyokumin", "Hure",
		"Hurensohn", "huzinoyamai", "hymen", "inc3st", "incest", "Inculato", "Injun", "intercourse", "inugoroshi", "inugorosi",
		"j1g4b0", "j1g4bo", "j1gab0", "j1gabo", "jackass", "jap", "JerkOff", "jig4b0", "jig4bo", "jigabo",
		"Jigaboo", "jiggaboo", "jizz", "Joder", "Joto", "junglebunny", "k1k3", "kichigai", "kik3", "Kike",
		"kikeiji", "kikeizi", "Kilurself", "kitigai", "kkk", "kluklux", "knobhead", "koonhunt", "koonkill", "koroshiteyaru",
		"koumoujin", "koumouzin", "kun7", "kurombo", "Kurva", "Kurwa", "kxkxk", "l3sb0", "lezbo", "lezzie",
		"m07th3rfukr", "m0th3rfvk3r", "m0th3rfvker", "manberries", "manko", "manshaft", "Maricon", "Masterbat", "masterbate", "Masturbacion",
		"masturbait", "Masturbare", "Masturbate", "Masturbazione", "Merda", "Merde", "Meth", "Mierda", "milf", "Minge",
		"Miststück", "mitsukuchi", "mitukuti", "Molest", "molester", "molestor", "Mong", "moth3rfucer", "moth3rfvk3r", "moth3rfvker",
		"motherfucker", "Mulatto", "n1663r", "n1664", "n166a", "n166er", "n1g3r", "n1German", "n1gg3r", "n1gGerman",
		"n3gro", "n4g3r", "n4gg3r", "n4gGerman", "n4z1", "nag3r", "nagg3r", "nagGerman", "natzi", "naz1",
		"nazi", "nazl", "neGerman", "ngGerman", "nggr", "NhigGerman", "ni666", "ni66a", "ni66er", "ni66g",
		"ni6g", "ni6g6", "ni6gg", "Nig", "nig66", "nig6g", "nigar", "niGerman", "nigg3", "nigg6",
		"nigga", "niggaz", "nigGerman", "nigglet", "niggr", "nigguh", "niggur", "niggy", "niglet", "Nignog",
		"nimpinin", "ninpinin", "Nipples", "niqqa", "niqqer", "Nonce", "nugga", "Nutsack", "Nutted", "nygGerman",
		"omeko", "Orgy", "p3n15", "p3n1s", "p3ni5", "p3nis", "p3nl5", "p3nls", "Paki", "Panties",
		"Pedo", "pedoph", "pedophile", "pen15", "pen1s", "Pendejo", "peni5", "penile", "penis", "Penis",
		"penl5", "penls", "penus", "Perra", "phaggot", "phagot", "phuck", "Pikey", "Pinche", "Pizda",
		"Polla", "Porn", "Porra", "pr1ck", "preteen", "prick", "pu555y", "pu55y", "pub1c", "Pube",
		"pubic", "pun4ni", "pun4nl", "Punal", "punan1", "punani", "punanl", "puss1", "puss3", "puss5",
		"pusse", "pussi", "Pussies", "pusss1", "pussse", "pusssi", "pusssl", "pusssy", "Pussy", "Puta",
		"Putain", "Pute", "Puto", "Puttana", "Puttane", "Puttaniere", "puzzy", "pvssy", "queef", "r3c7um",
		"r4p15t", "r4p1st", "r4p3", "r4pi5t", "r4pist", "raape", "raghead", "raibyo", "Raip", "rap15t",
		"rap1st", "Rapage", "rape", "Raped", "rapi5t", "Raping", "rapist", "rectum", "Reggin", "reipu",
		"retard", "Ricchione", "rimjob", "rizzape", "rompari", "Salaud", "Salope", "sangokujin", "sangokuzin", "santorum",
		"Scheiße", "Schlampe", "Schlampe", "schlong", "Schwuchtel", "Scrote", "secks", "seishinhakujaku", "seishinijo", "seisinhakuzyaku",
		"seisinizyo", "Semen", "semushiotoko", "semusiotoko", "sh|t", "sh17", "sh1t", "Shat", "Shemale", "shi7",
		"shinajin", "shinheimin", "shirakko", "shit", "Shitty", "shl7", "shlt", "shokubutsuningen", "sinazin", "sinheimin",
		"Skank", "slut", "SMD", "Sodom", "sofaking", "Spanishick", "Spanishook", "Spanishunk", "STD", "STDs",
		"suckmy", "syokubutuningen", "Taint", "Tampon", "Tapatte", "Tapette", "Tard", "Tarlouse", "teabag", "teebag",
		"teensex", "teino", "Testicles", "Thot", "tieokure", "tinpo", "Tits", "tokushugakkyu", "tokusyugakkyu", "torukoburo",
		"torukojo", "torukozyo", "tosatsu", "tosatu", "towelhead", "Tranny", "tunbo", "tw47", "tw4t", "twat",
		"tyankoro", "tyonga", "tyonko", "tyonkoro", "tyourinbo", "tyourippo", "tyurenbo", "ushigoroshi", "usigorosi", "v461n4",
		"v461na", "v46in4", "v46ina", "v4g1n4", "v4g1na", "v4gin4", "v4gina", "va61n4", "va61na", "va6in4",
		"va6ina", "Vaccagare", "Vaffanculo", "Vag", "vag1n4", "vag1na", "vagin4", "vagina", "VateFaire", "vvhitepower",
		"w3tb4ck", "w3tback", "Wank", "wanker", "wetb4ck", "wetback", "wh0r3", "wh0re", "whitepower", "whor3",
		"whore", "Wog", "Wop", "x8lp3t", "XBLPET", "XBLRewards", "Xl3LPET", "yabunirami", "Zipperhead", "Блядь",
		"сука", "アオカン", "あおかん", "イヌゴロシ", "いぬごろし", "インバイ", "いんばい", "オナニー", "おなにー", "オメコ",
		"カワラコジキ", "かわらこじき", "カワラモノ", "かわらもの", "キケイジ", "きけいじ", "キチガイ", "きちがい", "キンタマ", "きんたま",
		"クロンボ", "くろんぼ", "コロシテヤル", "ころしてやる", "シナジン", "しなじん", "タチンボ", "たちんぼ", "チョンコウ", "ちょんこう",
		"チョンコロ", "ちょんころ", "ちょん公", "チンポ", "ちんぽ", "ツンボ", "つんぼ", "とるこじょう", "とるこぶろ", "トルコ嬢",
		"トルコ風呂", "ニガー", "ニグロ", "にんぴにん", "はんとうじん", "マンコ", "まんこ", "レイプ", "れいぷ", "低能",
		"屠殺", "強姦", "援交", "支那人", "精薄", "精薄者", "輪姦"
	};
}
