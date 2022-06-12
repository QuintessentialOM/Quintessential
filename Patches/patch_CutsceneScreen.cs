using MonoMod;
using System;
using System.Reflection;
using System.Collections.Generic;
using Quintessential;
using SDL2;
using Texture = class_256;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
[MonoModPatch("class_252")]

class patch_CutsceneScreen
{
	#pragma warning disable CS0649 // Field '---' is never assigned to, and will always have its default value null
	#pragma warning disable CS0414 // The field '---' is assigned but its value is never used
	private readonly class_264 field_2038;
	private readonly List<patch_CutsceneScreen.class_253> field_2039 = new List<patch_CutsceneScreen.class_253>();
	private int field_2040;
	private float field_2041;
	private Maybe<float> field_2042;
	private float field_2043;
	private static readonly int field_2044 = -10;
	private static readonly float field_2045 = 0.005f;

	private static Dictionary<string, string> CutsceneBackgrounds = new()
	{
		{"intro-1","textures/cinematic/backgrounds/workshop"},
		{"outro-1","textures/cinematic/backgrounds/workshop"},
		{"intro-2","textures/cinematic/backgrounds/greathall_a"},
		{"outro-2","textures/cinematic/backgrounds/greathall_b"},
		{"intro-3","textures/cinematic/backgrounds/tailor_a"},
		{"middle-3","textures/cinematic/backgrounds/tailor_a"},
		{"outro-3","textures/cinematic/backgrounds/tailor_a"},
		{"intro-4","textures/cinematic/backgrounds/tailor_a"},
		{"outro-4","textures/cinematic/backgrounds/tailor_b"},
		{"intro-5","textures/cinematic/backgrounds/greathall_c"},
		{"outro-5","textures/cinematic/backgrounds/tailor_a"},
	};
	private static Dictionary<string, string> CutsceneLocations = new()
	{
		{"intro-1","Alchemist's Workshop, House Van Tassen"},
		{"outro-1","Alchemist's Workshop, House Van Tassen"},
		{"intro-2","The Greathall, House Van Tassen"},
		{"outro-2","The Greathall, House Van Tassen"},
		{"intro-3","Abandoned Tailor Shop, The Downriver Quarters"},
		{"middle-3","Abandoned Tailor Shop, The Downriver Quarters"},
		{"outro-3","Abandoned Tailor Shop, The Downriver Quarters"},
		{"intro-4","Abandoned Tailor Shop, The Downriver Quarters"},
		{"outro-4","Abandoned Tailor Shop, The Downriver Quarters"},
		{"intro-5","The Greathall, House Van Tassen"},
		{"outro-5","Abandoned Tailor Shop, The Downriver Quarters"},
	};
	private static List<string> CutsceneSlowFades = new()
	{
		"outro-5",
	};

	public static void addCutsceneData(string ID, string location, string background, bool slowFade)
	{
		CutsceneLocations.Add(ID, location);
		CutsceneBackgrounds.Add(ID, background);
		if (slowFade)
		{
			CutsceneSlowFades.Add(ID);
		}
	}

	public sealed class class_253
	{
		public class_230 field_2046;
		public Vector2 field_2047;
		public Maybe<patch_CutsceneScreen.class_255> field_2048;
		public bool field_2049;
	}
	public sealed class class_254
	{
		public patch_CutsceneScreen field_2050;
		public bool field_2051;

		internal extern void orig_method_685(VignetteEvent.LineFields param_4177);
		internal void method_685(VignetteEvent.LineFields param_4177)
		{
			orig_method_685(param_4177);
		}
		internal extern void orig_method_686(VignetteEvent.struct_131 param_4178);
		internal void method_686(VignetteEvent.struct_131 param_4178)
		{
			orig_method_686(param_4178);
		}
		internal extern void orig_method_687(VignetteEvent.struct_132 param_4179);
		internal void method_687(VignetteEvent.struct_132 param_4179)
		{
			orig_method_687(param_4179);
		}

	}
		public sealed class class_255
	{
		public class_230 field_2052;
		public class_230 field_2053;
		public Vector2 field_2054;
		public Vector2 field_2055;
	}

	private extern bool orig_method_678();
	private bool method_678()
	{
		return orig_method_678();
	}
	private extern int orig_method_680();
	private int method_680()
	{
		return orig_method_680();
	}
	private extern int orig_method_681();
	private int method_681()
	{
		return orig_method_681();
	}
	private void method_684()
	{
		//reimplements the method
		GameLogic.field_2434.field_2451.method_574(this.field_2038);
		if (CutsceneSlowFades.Contains(this.field_2038.field_2090))	{
			GameLogic.field_2434.method_947((Maybe<class_124>)Transitions.field_4109, (Maybe<class_124>)Transitions.field_4108);
		} else {
			GameLogic.field_2434.method_947((Maybe<class_124>)Transitions.field_4107, (Maybe<class_124>)Transitions.field_4106);
		}
			
	}

	public extern void orig_method_50(float param_4176);
	public void method_50(float param_4176)
	{
		//reimplements the method
		Vignette vignette = this.field_2038.method_712();
		VignetteEvent vignetteEvent1 = vignette.field_4125[0][this.field_2040];

		float a = 0.0f;
		if (this.field_2042.method_1085())
		{
			this.field_2041 += param_4176;
			a = class_162.method_406(this.field_2041 / this.field_2042.method_1087());
			if ((double)a >= 1.0)
			{
				this.field_2042 = (Maybe<float>)struct_18.field_1431;
				foreach (var class253 in this.field_2039)
				{
					if (class253.field_2048.method_1085())
					{
						class253.field_2046 = class253.field_2048.method_1087().field_2053;
						class253.field_2047 = class253.field_2048.method_1087().field_2055;
						class253.field_2048 = (Maybe<patch_CutsceneScreen.class_255>)struct_18.field_1431;
					}
				}
			}
		}
		this.field_2043 += param_4176;

		string puzzleID = this.field_2038.field_2090;
		Texture background = class_238.field_1989.field_73; // transparent background
		string location = string.Empty;
		if (CutsceneLocations.ContainsKey(puzzleID))
		{
			location = (string)class_134.method_253(CutsceneLocations[puzzleID], string.Empty);
		}
		if (CutsceneBackgrounds.ContainsKey(puzzleID))
		{
			background = QApi.fetchBackground(CutsceneBackgrounds[puzzleID]);
		}
		
		Texture class256_1 = background;
		string str = location;

		class_135.method_279(Color.Black, Vector2.Zero, class_115.field_1433);
		float num = class_115.field_1433.Y / (float)class256_1.field_2056.Y;
		Vector2 vector2_1 = class256_1.field_2056.ToVector2() * num;
		class_135.method_263(class256_1, Color.White, class_115.field_1433 / 2 - vector2_1 / 2, vector2_1);
		Vector2 vector2_2 = new Vector2(class_115.field_1433.X / 2f - (float)(class_238.field_1989.field_84.field_531.field_2056.X / 2), class_115.field_1433.Y - 87f);
		class_135.method_272(class_238.field_1989.field_84.field_531, vector2_2.Rounded());
		class_135.method_290(str, new Vector2(class_115.field_1433.X / 2f, class_115.field_1433.Y - 68f), class_238.field_1990.field_2146, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);

		patch_CutsceneScreen.class_254 class254 = new patch_CutsceneScreen.class_254();
		class254.field_2050 = (patch_CutsceneScreen)this;
		class254.field_2051 = true;
		foreach (patch_CutsceneScreen.class_253 class253 in this.field_2039)
		{
			Vector2 vector2_3 = ((class253.field_2048.method_1085() ? class_162.method_413(class253.field_2048.method_1087().field_2054, class253.field_2048.method_1087().field_2055, class_162.method_417(0.0f, 1f, a)) : class253.field_2047) + new Vector2((float)this.method_680(), (float)this.method_681()) / 2).Rounded();
			bool flag = vignetteEvent1.method_2215() && vignetteEvent1.method_2218().field_4136 == class253.field_2046;
			enum_1 enum1 = class253.field_2049 ? (enum_1)1 : (enum_1)0;
			Vector2 vector2_4 = flag ? new Vector2(260f, 395f) : new Vector2(245f, 372f);
			Vector2 vector2_5 = flag ? new Vector2(-132f, -174f) : new Vector2(-124f, -163f);
			if (this.method_678())
			{
				vector2_4 = flag ? new Vector2(197f, 300f) : new Vector2(185f, 281f);
				vector2_5 = flag ? new Vector2(-100f, -132f) : new Vector2(-94f, -123f);
			}
			Vector2 vector2_6 = new Vector2(2f, 2f);
			class_135.method_266(class253.field_2046.field_1955, Color.White, vector2_3 + vector2_5 - vector2_6, vector2_4 + vector2_6 * 2, Bounds2.WithSize(0.0f, 0.0f, 1f, 1f), (enum_130)0, enum1);
			if (class253.field_2048.method_1085())
				class_135.method_266(class253.field_2048.method_1087().field_2053.field_1955, Color.White.WithAlpha(a), vector2_3 + vector2_5 - vector2_6, vector2_4 + vector2_6 * 2, Bounds2.WithSize(0.0f, 0.0f, 1f, 1f), (enum_130)0, enum1);
			class_256 class256_2 = flag ? class_238.field_1989.field_84.field_532 : class_238.field_1989.field_84.field_533;
			Vector2 vector2_7 = new Vector2((float)this.method_680(), (float)this.method_681());
			Vector2 vector2_8 = vector2_3 - vector2_7 / 2;
			Color white = Color.White;
			Vector2 vector2_9 = vector2_8.Rounded();
			Vector2 vector2_10 = vector2_7;
			class_135.method_263(class256_2, white, vector2_9, vector2_10);
			float y = flag ? -206f : -193f;
			class_1 class1 = class_238.field_1990.field_2144;
			if (this.method_678())
			{
				y = flag ? -157f : -148f;
				class1 = class_238.field_1990.field_2143;
			}
			class_135.method_290(class253.field_2046.field_1954.method_1060(), vector2_3 + new Vector2(0.0f, y), class1, Color.White, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, -2, Color.Black, class_238.field_1989.field_99.field_706.field_751, int.MaxValue, false, true);
		}

		vignetteEvent1.method_2221(new Action<VignetteEvent.LineFields>(class254.method_685));

		bool ESCPressed = class_115.method_198(SDL.enum_160.SDLK_ESCAPE);
		if (GameLogic.field_2434.field_2451.method_575(this.field_2038) && ESCPressed)
		{
			this.method_684();
		}

		//redid the early-return logic so it's easier to read
		bool leftClickPressed = class_115.method_206((enum_142)1);
		bool tabPressed = class_115.method_198(SDL.enum_160.SDLK_TAB);
		bool spacePressed = class_115.method_198(SDL.enum_160.SDLK_SPACE);
		bool flagAdvanceDialogue = leftClickPressed || tabPressed || spacePressed;

		bool flag2042 = this.field_2042.method_1085();
		bool flag254 = !class254.field_2051;
		if (flag254 || flag2042 || !flagAdvanceDialogue)
		{
			return;
		}
		//otherwise, we can advance the cutscene
		if (this.field_2040 == vignette.field_4125[0].Count - 1)
		{
			this.method_684();
		}
		else
		{
			++this.field_2040;
			this.field_2043 = 0.0f;
			VignetteEvent vignetteEvent2 = vignette.field_4125[0][this.field_2040];
			vignetteEvent2.method_2222(new Action<VignetteEvent.struct_131>(class254.method_686));
			vignetteEvent2.method_2223(new Action<VignetteEvent.struct_132>(class254.method_687));
			if (this.field_2042.method_1085())
			{
				++this.field_2040;
				class_162.method_403(this.field_2040 < vignette.field_4125[0].Count && vignette.field_4125[0][this.field_2040].method_2215(), "There must be a line after a non-line vignette event.");
			}
		}
		class_238.field_1991.field_1824.method_28(1f);
	}
}