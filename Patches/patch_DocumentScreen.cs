using System;
using System.Collections.Generic;
using Quintessential;
using SDL2;
using Texture = class_256;
using Font = class_1;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_DocumentScreen
{
	#pragma warning disable CS0649 // Field '---' is never assigned to, and will always have its default value null
	#pragma warning disable CS0414 // The field '---' is assigned but its value is never used
	private readonly string[] field_2408;
	private readonly class_264 field_2409;
	public static readonly Color field_2410 = Color.FromHex(2299924); // dark brown
	public static readonly Color field_2411 = Color.FromHex(6169623); // brown
	public static readonly Color field_2412 = Color.FromHex(3809793).WithAlpha(0.9f); // dark yellow

	private static Dictionary<string, Action<Language, string[]>> DocumentBank;
	public static void initializeDocumentDictionary()
	{
		if (DocumentBank != null) return;

		DocumentBank = new();
		createDocument("letter-0", class_238.field_1989.field_85.field_563, drawLetter0);
		createDocument("letter-1", class_238.field_1989.field_85.field_565, drawLetter1);
		createDocument("letter-2", class_238.field_1989.field_85.field_566, drawLetter2);
		createDocument("letter-3", class_238.field_1989.field_85.field_567, drawLetter3);
		createDocument("letter-4", class_238.field_1989.field_85.field_568, drawLetter4);
		createDocument("letter-5", class_238.field_1989.field_85.field_570, drawLetter5);
		createDocument("letter-6", class_238.field_1989.field_85.field_571, drawLetter6, class_238.field_1989.field_85.field_572);
		//createDocument("letter-7", class_238.field_1989.field_85.field_573, drawLetter7); // "letter-7" was removed from the game.
		createDocument("letter-9", class_238.field_1989.field_85.field_574, drawLetter9);
		createDocument("letter-response", class_238.field_1989.field_85.field_575, drawLetterResponse);
		createDocument("outro-6", class_238.field_1989.field_85.field_566, drawOutro6);
		createDocument("intro-6", class_238.field_1989.field_85.field_566, drawIntro6);
	}

	public static Font method_894() // returns font "cinzel 21" if not Russian or Ukranian, otherwise returns font "cormorant 22.5"
	{
		Language lang = class_134.field_1504;
		return lang == Language.Russian || lang == Language.Ukrainian ? class_238.field_1990.field_2148 : class_238.field_1990.field_2147;
	}

	public static Font getHandwrittenFont()
	{
		return class_134.field_1504 == Language.Korean ? class_238.field_1990.field_2154 /*naver_regular_17_25*/ : class_238.field_1990.field_2153 /*reenie_regular_17_25*/;
	}

	public static Bounds2 drawText(
		string text,
		Vector2 position,
		Font font,
		Color color,
		enum_0 alignment = (enum_0)0,
		float lineSpacing = 1f,
		float columnWidth = float.MaxValue,
		float truncateWidth = float.MaxValue,
		int maxCharactersDrawn = int.MaxValue,
		bool returnBoundingBox = false)
	{
		return class_135.method_290(text, position, font, color, alignment, lineSpacing,
				0.6f/*default for documents, not sure what it does*/,
				columnWidth,
				truncateWidth,
				0/*default for documents, not sure what it does*/,
				new Color()/*default for documents, not sure what it does*/,
				(class_256)null/*default for documents, changing it seems to affect the color somehow, not sure what it actually does*/,
				Math.Max(-1, maxCharactersDrawn - 1),
				returnBoundingBox,
				true/*false will hide the text - however, this can be done by setting maxCharactersDrawn == 0*/);
	}

	//helper struct for custom campaigns
	//  no need for truncateWidth, maxCharactersDrawn, returnBoundingBox
	public sealed class TextItem
	{
		Vector2 position;
		Font font;
		Color color;
		enum_0 alignment;
		float lineSpacing;
		float columnWidth;
		bool handwritten;

		public TextItem(Vector2 position, Font font, Color color, enum_0 alignment, float lineSpacing, float columnWidth, bool handwritten)
		{
			this.position = position;
			this.font = font;
			this.color = color;
			this.alignment = alignment;
			this.lineSpacing = lineSpacing;
			this.columnWidth = columnWidth;
			this.handwritten = handwritten;
		}

		public void draw(Language lang, Vector2 origin, string text)
		{
			if (handwritten) font = getHandwrittenFont();
			drawText(text, origin + position, font, color, alignment, lineSpacing, columnWidth, float.MaxValue, int.MaxValue, true);
		}
	}

	public static void createSimpleDocument(string documentID, Texture documentTexture, List<TextItem> textItems, List<Vector2> pipPositions, Texture overlayTexture)
	{
		//construct the simple document drawing function
		Action<Language, Vector2, string[]> draw = (lang, origin, textArray) => {
			int maxIndex = Math.Min(textArray.Length, textItems.Count);
			for (int i = 0; i < maxIndex; i++)
			{
				textItems[i].draw(lang, origin, textArray[i]);
			}
			foreach (var pos in pipPositions)
			{
				class_135.method_272(class_238.field_1989.field_85.field_576, origin + pos);
			}
		};
		//save function in bank
		createDocument(documentID, documentTexture, draw, overlayTexture);
	}

	public static void createDocument(string documentID, Texture documentTexture, Action<Language, Vector2, string[]> draw, Texture overlayTexture = null)
	{
		//construct document drawing function
		Action<Language, string[]> action = (lang, textArray) =>	{
			Vector2 origin = class_115.field_1433 / 2 - documentTexture.field_2056.ToVector2() / 2;
			class_135.method_272(documentTexture, origin.Rounded()); //draw document
			draw(lang, origin, textArray);
			if (overlayTexture != null)
			{
				//draw overlayTexture
				class_135.method_271(overlayTexture, Color.White, origin.Rounded());
			}
		};
		//save function in bank
		if (DocumentBank.ContainsKey(documentID))
		{
			DocumentBank[documentID] = action;
			Logger.Log($"patch_DocumentScreen.createDocument: document \"{documentID}\" already exists - overwriting.");
		}
		else
		{
			DocumentBank.Add(documentID, action);
		}
	}

	public extern void orig_method_50(float param_4176);
	public void method_50(float param_4176)
	{
		// reimplements the method

		Language lang = class_134.field_1504;
		string documentID = this.field_2409.field_2090;
		string[] textArray = this.field_2408;

		// draw the document
		if (DocumentBank.ContainsKey(documentID))
		{
			DocumentBank[documentID](lang, textArray);
		}
		else
		{
			throw new class_266($"DocumentScreen.method50: There is no document defined for \"{documentID}\".");
		}

		// player input / scene transition
		if (class_115.method_206((enum_142)1) || class_115.method_198(SDL.enum_160.SDLK_ESCAPE))
		{
			GameLogic.field_2434.field_2451.method_574(this.field_2409);
			if (this.field_2409.field_2090 == "letter-6")
			{
				GameLogic.field_2434.method_945((IScreen)new CreditsScreen(), (Maybe<class_124>)Transitions.field_4109, (Maybe<class_124>)Transitions.field_4108);
			}
			else
			{
				GameLogic.field_2434.method_949();
				class_238.field_1991.field_1875.method_28(1f); // sound effect: ui_paper_back
			}
		}

		//debugging
		if (!class_110.field_1010)
			return;
		Language old_lang = lang;
		Dictionary<SDL.enum_160, Language> lookupTable = new()
		{
			{SDL.enum_160.SDLK_1, Language.English},
			{SDL.enum_160.SDLK_2, Language.German},
			{SDL.enum_160.SDLK_3, Language.French},
			{SDL.enum_160.SDLK_4, Language.Russian},
			{SDL.enum_160.SDLK_5, Language.Chinese},
			{SDL.enum_160.SDLK_6, Language.Japanese},
			{SDL.enum_160.SDLK_7, Language.Spanish},
			{SDL.enum_160.SDLK_8, Language.Korean},
			{SDL.enum_160.SDLK_9, Language.Turkish},
			{SDL.enum_160.SDLK_0, Language.Ukrainian},
		};
		foreach (var item in lookupTable)
		{
			if (class_115.method_198(item.Key)) lang = item.Value;
		}
		class_134.field_1504 = lang;
		if ((int)old_lang == (int)lang)
			return;
		GameLogic.field_2434.method_949();
		GameLogic.field_2434.method_946((IScreen)new DocumentScreen(this.field_2409));
	}

	//vanilla documents // clean these up later, maybe?
	private static void drawLetter0(Language lang, Vector2 origin, string[] textArray)
	{
		Texture texture_letter_0_bar = class_238.field_1989.field_85.field_564;
		Texture texture_pip = class_238.field_1989.field_85.field_576;

		float x = 707f - class_135.method_290(textArray[0], origin + new Vector2(390f, 631f), class_238.field_1990.field_2149, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true).Width;
		class_135.method_276(texture_letter_0_bar, Color.White, origin + new Vector2(1114f - x, 631f), new Vector2(x, 7f));
		float num = 0.0f;
		for (int index = 1; index <= 6; ++index)
			num += class_135.method_290(textArray[index], Vector2.Zero, class_238.field_1990.field_2150, Color.White, (enum_0)0, 1f, 0.6f, 680f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false).Height;
		origin += new Vector2(348f, (float)(456.0 + (double)num / 2.0));
		class_135.method_272(texture_pip, origin);
		Bounds2 bounds2_2;
		bounds2_2 = class_135.method_290(textArray[1], origin + new Vector2(32f, -1f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 680f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
		origin += new Vector2(0.0f, -bounds2_2.Height);
		bounds2_2 = class_135.method_290(textArray[2], origin + new Vector2(32f, -1f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 680f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
		origin += new Vector2(0.0f, -20f - bounds2_2.Height);
		class_135.method_272(texture_pip, origin);
		bounds2_2 = class_135.method_290(textArray[3], origin + new Vector2(32f, -1f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 680f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
		origin += new Vector2(0.0f, -20f - bounds2_2.Height);
		class_135.method_272(texture_pip, origin);
		bounds2_2 = class_135.method_290(textArray[4], origin + new Vector2(32f, -1f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 680f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
		origin += new Vector2(0.0f, -20f - bounds2_2.Height);
		class_135.method_272(texture_pip, origin);
		bounds2_2 = class_135.method_290(textArray[5], origin + new Vector2(32f, -1f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 680f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
		origin += new Vector2(0.0f, -20f - bounds2_2.Height);
		class_135.method_272(texture_pip, origin);
		bounds2_2 = class_135.method_290(textArray[6], origin + new Vector2(32f, -1f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true);
		if (lang == Language.Japanese)
			origin += new Vector2(401f, -34f);
		else
			origin += new Vector2(49f, -3f) + new Vector2(bounds2_2.Width, 0.0f);
		class_135.method_290(textArray[7], origin, getHandwrittenFont(), DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
	}
	private static void drawLetter1(Language lang, Vector2 origin, string[] textArray)
	{
		Bounds2 bounds2 = class_135.method_290(textArray[0], Vector2.Zero, class_238.field_1990.field_2151, Color.White, (enum_0)0, 1f, 0.6f, 960f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false);
		class_135.method_290(textArray[0], origin + new Vector2(0.0f, bounds2.Height / 2f) + new Vector2(236f, 455f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 960f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290(textArray[1], origin + new Vector2(236f, 250f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
	}
	private static void drawLetter2(Language lang, Vector2 origin, string[] textArray)
	{
		class_135.method_290(textArray[0].method_441(), origin + new Vector2(444f, 723f), class_238.field_1990.field_2148, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290(textArray[1], origin + new Vector2(444f, 694f), class_238.field_1990.field_2149, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290(textArray[2], origin + new Vector2(444f, 667f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290("5", origin + new Vector2(667f, 201f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		Bounds2 bounds2 = class_135.method_290(textArray[3], Vector2.Zero, class_238.field_1990.field_2151, Color.White, (enum_0)0, 1f, 0.6f, 375f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false);
		class_135.method_290(textArray[3], origin + new Vector2(0.0f, bounds2.Height) + new Vector2(272f, 245f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 375f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
	}
	private static void drawLetter3(Language lang, Vector2 origin, string[] textArray)
	{
		class_135.method_290(textArray[0], origin + new Vector2(668f, 569f), patch_DocumentScreen.method_894(), DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290(textArray[1], origin + new Vector2(668f, 490f), class_238.field_1990.field_2149, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290(textArray[2], origin + new Vector2(668f, 464f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		Bounds2 bounds2 = class_135.method_290(textArray[3], Vector2.Zero, class_238.field_1990.field_2150, Color.White, (enum_0)1, 1f, 0.6f, 775f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false);
		class_135.method_290(textArray[3], origin + new Vector2(0.0f, bounds2.Height / 2f) + new Vector2(668f, 370f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, 775f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
	}
	private static void drawLetter4(Language lang, Vector2 origin, string[] textArray)
	{
		Bounds2 bounds2 = class_135.method_290(textArray[0], Vector2.Zero, getHandwrittenFont(), Color.White, (enum_0)0, 1f, 0.6f, 760f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false);
		class_135.method_290(textArray[0], origin + new Vector2(0.0f, bounds2.Height / 2f) + new Vector2(165f, 235f), getHandwrittenFont(), DocumentScreen.field_2411, (enum_0)0, 1f, 0.6f, 760f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_271(class_238.field_1989.field_85.field_569, Color.White.WithAlpha(0.8f), origin.Rounded());
	}
	private static void drawLetter5(Language lang, Vector2 origin, string[] textArray)
	{
		void method895(Vector2 vec2, string param_4547, float param_4548)
		{
			float width = class_135.method_290(param_4547.method_634(), vec2 + new Vector2(313f, param_4548), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true).Width;
			class_135.method_272(class_238.field_1989.field_85.field_576, vec2 + new Vector2((float)(313.0 - (double)width / 2.0 - 20.0), param_4548 + 1f).Rounded());
			class_135.method_272(class_238.field_1989.field_85.field_576, vec2 + new Vector2((float)(313.0 + (double)width / 2.0 + 10.0), param_4548 + 1f).Rounded());
		}

		Action<Vector2, string, float> action = new Action<Vector2, string, float>(method895);
		class_135.method_290(textArray[0], origin + new Vector2(313f, 653f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290(textArray[1].method_634(), origin + new Vector2(313f, 620f), patch_DocumentScreen.method_894(), DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290(textArray[2], origin + new Vector2(313f, 590f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		action(origin, textArray[3], 515f);
		class_135.method_290(textArray[4], origin + new Vector2(313f, 480f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)1, 1.6f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		action(origin, textArray[5], 373f);
		class_135.method_290(textArray[6], origin + new Vector2(313f, 339f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)1, 1.6f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		action(origin, textArray[7], 297f);
		class_135.method_290(textArray[8], origin + new Vector2(313f, 260f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)1, 1.6f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		action(origin, textArray[9], 184f);
		class_135.method_290(textArray[10], origin + new Vector2(313f, 149f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)1, 1.6f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
	}
	private static void drawLetter6(Language lang, Vector2 origin, string[] textArray)
	{
		Bounds2 bounds2 = class_135.method_290(textArray[0], Vector2.Zero, class_238.field_1990.field_2150, Color.White, (enum_0)0, 1f, 0.6f, 775f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false);
		class_135.method_290(textArray[0], origin + new Vector2(0.0f, bounds2.Height / 2f) + new Vector2(237f, 407f), class_238.field_1990.field_2150, DocumentScreen.field_2412, (enum_0)0, 1f, 0.6f, 775f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
	}
	/* "letter-7" was removed from the game.
	private static void drawLetter7(Language lang, Vector2 origin, string[] textArray)
	{
		class_135.method_290(textArray[0].method_634(), origin + new Vector2(596f, 584f), patch_DocumentScreen.method_894(), DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		Bounds2 bounds2 = class_135.method_290(textArray[1], Vector2.Zero, class_238.field_1990.field_2149, Color.White, (enum_0)0, 1f, 0.6f, 800f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false);
		class_135.method_290(textArray[1], origin + new Vector2(0.0f, bounds2.Height / 2f) + new Vector2(201f, 331f), class_238.field_1990.field_2149, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 800f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
	}
	*/
	private static void drawLetter9(Language lang, Vector2 origin, string[] textArray)
	{
		Bounds2 bounds2 = class_135.method_290(textArray[0], Vector2.Zero, getHandwrittenFont(), Color.White, (enum_0)0, 1f, 0.6f, 800f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false);
		class_135.method_290(textArray[0], origin + new Vector2(0.0f, bounds2.Height / 2f) + new Vector2(150f, 286f), getHandwrittenFont(), DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 800f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290(textArray[1], origin + new Vector2(146f, 122f), getHandwrittenFont(), DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
	}
	private static void drawLetterResponse(Language lang, Vector2 origin, string[] textArray)
	{
		Bounds2 bounds2 = class_135.method_290(textArray[0], Vector2.Zero, class_238.field_1990.field_2150, Color.White, (enum_0)0, 1f, 0.6f, 795f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false);
		class_135.method_290(textArray[0], origin + new Vector2(0.0f, bounds2.Height / 2f) + new Vector2(206f, 471f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 795f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		class_135.method_290(textArray[2], origin + new Vector2(206f, 208f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		float width = class_135.method_290(textArray[1], origin + new Vector2(206f, 256f), class_238.field_1990.field_2150, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, true).Width;
		class_135.method_263(class_238.field_1989.field_101.field_794, Color.White, origin + new Vector2(205f, 246f), new Vector2(width + 5f, 3f));
	}
	private static void drawOutro6(Language lang, Vector2 origin, string[] textArray)
	{
		class_135.method_292(textArray[0], origin + new Vector2(272f, 504f), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 375f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue);
	}
	private static void drawIntro6(Language lang, Vector2 origin, string[] textArray)
	{
		int num = 0;
		switch (lang)
		{
			case Language.French:
				num = 15;
				break;
			case Language.Russian:
			case Language.Ukrainian:
				num = 30;
				break;
		}
		class_135.method_290(textArray[0], origin + new Vector2(444f, (float)(723 + num)), class_238.field_1990.field_2148, DocumentScreen.field_2410, (enum_0)1, 1f, 0.6f, 375f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
		Bounds2 bounds2 = class_135.method_290(textArray[1], Vector2.Zero, class_238.field_1990.field_2151, Color.White, (enum_0)0, 1f, 0.6f, 375f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, true, false);
		class_135.method_290(textArray[1], origin + new Vector2(0.0f, bounds2.Height) + new Vector2(272f, (float)(245 - num)), class_238.field_1990.field_2151, DocumentScreen.field_2410, (enum_0)0, 1f, 0.6f, 375f, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
	}
}