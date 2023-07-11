using MonoMod;
using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Texture = class_256;

public class patch_JournalScreen{

	public static int currentJournal;
	
	private static Texture JournalGoLeft, JournalGoLeftHover, JournalGoRight, JournalGoRightHover;
	
	// mirror real version
	private static int field_2554;
	
	[PatchJournalScreen]
	public extern void orig_method_50(float deltaTime);
	public void method_50(float deltaTime){
		orig_method_50(deltaTime);

		if(QuintessentialLoader.AllJournals.Count == 1)
			return;
		
		JournalGoLeft ??= class_235.method_615("Quintessential/journal_go_left");
		JournalGoLeftHover ??= class_235.method_615("Quintessential/journal_go_left_hover");
		JournalGoRight ??= class_235.method_615("Quintessential/journal_go_right");
		JournalGoRightHover ??= class_235.method_615("Quintessential/journal_go_right_hover");
		
		Vector2 size = new Vector2(1516f, 922f);
		Vector2 corner = (Input.ScreenSize() / 2 - size / 2 + new Vector2(-2f, -11f)).Rounded();
		Vector2 lPos = corner + new Vector2(84, 812f);
		Vector2 rPos = corner + new Vector2(188, 812f);
		bool inLeftBound = Bounds2.WithSize(lPos, JournalGoLeft.field_2056.ToVector2()).Contains(Input.MousePos());
		bool inRightBound = Bounds2.WithSize(rPos, JournalGoRight.field_2056.ToVector2()).Contains(Input.MousePos());
		UI.DrawTexture(inLeftBound ? JournalGoLeftHover : JournalGoLeft, lPos);
		UI.DrawTexture(inRightBound ? JournalGoRightHover : JournalGoRight, rPos);
		UI.DrawText($"{currentJournal + 1}/{QuintessentialLoader.AllJournals.Count}", corner + new Vector2(157, 824f), UI.Text, UI.TextColor, TextAlignment.Centred);

		if(Input.IsLeftClickPressed() && (inLeftBound || inRightBound)){
			class_238.field_1991.field_1821.method_28(1f);
			
			if(inLeftBound){
				var next = currentJournal - 1;
				if(next < 0)
					next += QuintessentialLoader.AllJournals.Count;
				currentJournal = next;
			}

			if(inRightBound){
				var next = currentJournal + 1;
				if(next >= QuintessentialLoader.AllJournals.Count)
					next = 0;
				currentJournal = next;
			}

			JournalVolumes.field_2572 = QuintessentialLoader.AllJournals[currentJournal].ToArray();
			field_2554 = JournalVolumes.field_2572.Length - 1;
			UI.InstantCloseScreen();
			UI.OpenScreen(new JournalScreen(false));
		}
	}

	[MonoModIgnore]
	[PatchJournalPuzzleBackgrounds]
	private extern void method_1040(Puzzle puzzle, Vector2 pos, bool big);

	public static void ResetPosition(){
		currentJournal = 0;
		field_2554 = JournalVolumes.field_2572.Length - 1;
	}

	// found by name in MonoModRules
	public static string CurrentJournalName(){
		return currentJournal == 0 ? "The Journal of Alchemical Engineering" : QuintessentialLoader.ModJournalModels[currentJournal - 1].Title;
	}

	public static Texture CurrentJournalBg(Texture before, bool large){
		if(currentJournal == 0)
			return before;
		var journal = QuintessentialLoader.ModJournalModels[currentJournal - 1];
		return large switch{
			true when !string.IsNullOrWhiteSpace(journal.PuzzleBackgroundLarge) => (journal.PuzzleBackgroundLargeTex ??= class_235.method_615(journal.PuzzleBackgroundLarge)),
			false when !string.IsNullOrWhiteSpace(journal.PuzzleBackgroundSmall) => (journal.PuzzleBackgroundSmallTex ??= class_235.method_615(journal.PuzzleBackgroundSmall)),
			_ => before
		};
	}
}