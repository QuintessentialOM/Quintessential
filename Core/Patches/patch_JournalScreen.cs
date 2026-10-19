using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using System;
using System.Linq;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

public class patch_JournalScreen {

	public static int currentJournal;
	
	private static Texture JournalGoLeft, JournalGoLeftHover, JournalGoRight, JournalGoRightHover;
	
	public extern void orig_RenderFrame(float deltaTime);
	public void RenderFrame(float deltaTime){
        orig_RenderFrame(deltaTime);

		if(QuintessentialLoader.AllJournals.Count == 1)
			return;
		
		JournalGoLeft ??= AssetLoaderHelper.LoadTexture("textures/journal_go_left");
		JournalGoLeftHover ??= AssetLoaderHelper.LoadTexture("textures/journal_go_left_hover");
		JournalGoRight ??= AssetLoaderHelper.LoadTexture("textures/journal_go_right");
		JournalGoRightHover ??= AssetLoaderHelper.LoadTexture("textures/journal_go_right_hover");
		
		Vector2 size = new(1516f, 922f);
		Vector2 corner = (InputManager.screenSize / 2 - size / 2 + new Vector2(-2f, -11f)).Rounded();
		Vector2 lPos = corner + new Vector2(84, 812f);
		Vector2 rPos = corner + new Vector2(188, 812f);
		bool inLeftBound = Bounds2.WithSize(lPos, JournalGoLeft.size.ToVector2()).Contains(InputManager.MousePos());
		bool inRightBound = Bounds2.WithSize(rPos, JournalGoRight.size.ToVector2()).Contains(InputManager.MousePos());
		TextureRenderer.Render(inLeftBound ? JournalGoLeftHover : JournalGoLeft, lPos);
        TextureRenderer.Render(inRightBound ? JournalGoRightHover : JournalGoRight, rPos);
        UI.DrawText($"{currentJournal + 1}/{QuintessentialLoader.AllJournals.Count}", corner + new Vector2(157, 824f), UI.Text, UI.TextColor, TextAlignment.Center);
        
		if(InputManager.IsClickPressed(MouseButtonType.LeftClick) && (inLeftBound || inRightBound)){
            Assets.sounds.click_button.PlaySound(1f);
			if (inLeftBound) {
                SetJournal(currentJournal - 1);
            } else
                SetJournal(currentJournal + 1);
        }
	}

    public static void SetJournal(int newJournalIndex) {
        currentJournal = Utils.Modulo(newJournalIndex, QuintessentialLoader.AllJournals.Count);
        JournalVolumes.volumes = [.. QuintessentialLoader.AllJournals[currentJournal].volumes];
        GameLogic.instance.saveData.SetJournalVolume(int.Clamp(GameLogic.instance.saveData.GetJournalVolume(), 0, JournalVolumes.volumes.Length - 1));
        UI.InstantCloseScreen();
        UI.OpenScreen(new JournalScreen(false));
    }

    public static LocString CurrentJournalTitle() {
        return QuintessentialLoader.AllJournals[currentJournal].title;
	}

	public static Texture CurrentJournalBg(Texture before, bool large){
        if (currentJournal == 0)
        	return before;
        var journal = QuintessentialLoader.AllJournals[currentJournal];
        return large switch{
        	true => journal.puzzleBackgroundLarge ?? before,
        	false => journal.puzzleBackgroundSmall ?? before
        };
    }

    [MonoModILInject("RenderFrame")]
    static void PatchJournalScreen(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching journal screen");
        if (method.HasBody) {
            ILCursor cursor = new(new ILContext(method));
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdstr("The Journal of Alchemical Engineering"))) {
                cursor.Remove();
                cursor.Remove();
                TypeDefinition holder = MonoModRule.Modder.FindType("JournalScreen").Resolve();
                MethodDefinition to = holder.Methods.First(m => m.Name.Equals("CurrentJournalTitle"));
                cursor.Emit(OpCodes.Call, to);
            } else {
                Console.WriteLine("Failed to modify journal screen (no match)!");
                throw new Exception();
            }
        } else {
            Console.WriteLine("Failed to modify journal screen (no body)!");
            throw new Exception();
        }
    }

    [MonoModILInject("RenderPuzzleSelect")]
    static void PatchJournalPuzzleBackgrounds(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching journal screen puzzle backgrounds");
        if (method.HasBody) {
            ILCursor cursor = new(new ILContext(method));
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(1))) {
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.Emit(OpCodes.Ldarg_3);
                TypeDefinition holder = MonoModRule.Modder.FindType("JournalScreen").Resolve();
                MethodDefinition to = holder.Methods.First(m => m.Name.Equals("CurrentJournalBg"));
                cursor.Emit(OpCodes.Call, to);
                cursor.Emit(OpCodes.Stloc_1);
            } else {
                Console.WriteLine("Failed to modify journal screen puzzle backgrounds (no match)!");
                throw new Exception();
            }
        } else {
            Console.WriteLine("Failed to modify journal screen puzzle backgrounds (no body)!");
            throw new Exception();
        }
    }
}