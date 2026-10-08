using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using SDL2;
using System;
using System.Linq;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable IDE1006 // Naming Styles

class patch_GameLogic {
    [MonoModIgnore] public patch_Settings settingsData;
    public extern void orig_GameInit();
    public extern void orig_GameUnload(int exitCode);
	public extern void orig_ContentInit();


	public void GameInit(){
		QuintessentialLoader.PreInit();
        orig_GameInit();
		QuintessentialLoader.PostInit();
	}

	public void GameUnload(int exitCode) {
        Logger.Log("Starting mod unloading.");
        foreach (var mod in QuintessentialLoader.CodeMods)
            mod.Unload();

        Logger.Log("Finished unloading.");
        orig_GameUnload(exitCode);
	}

	public void ContentInit(){
        orig_ContentInit();
        QuintessentialLoader.ModContentInit();
    }

    public void SetWindowOffsetsToDefaults(Window window) {
        SDL.SDL_SetWindowPosition(window.window, settingsData.windowXPos.Get(), settingsData.windowYPos.Get());
    }

    [MonoModILInject("method_944")]
    static void PatchWindowPositionSet1(MethodDefinition method, CustomAttribute attribute) {
        if (!method.HasBody) {
            throw new Exception("Unable to patch window position set1. (no body)");
        }
        TypeDefinition holder = MonoModRule.Modder.FindType("GameLogic").Resolve();
        MethodDefinition call = holder.Methods.First((f) => f.Name == "SetWindowOffsetsToDefaults");

        ILCursor cursor = new(new ILContext(method));

        FieldReference window = null;
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Window", "ShowWindow"));
        cursor.TryGotoPrev(MoveType.Before, instr => instr.MatchLdfld(out window));
        cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Window", "ShowWindow"));
        cursor.EmitLdarg0();
        cursor.EmitLdarg0();
        cursor.EmitLdfld(window);
        cursor.EmitCallvirt(call);
    }

    [MonoModILInject("method_959")]
    static void PatchWindowPositionSet2(MethodDefinition method, CustomAttribute attribute) {
        if (!method.HasBody) {
            throw new Exception("Unable to patch window position set2. (no body)");
        }
        TypeDefinition holder = MonoModRule.Modder.FindType("GameLogic").Resolve();
        MethodDefinition call = holder.Methods.First((f) => f.Name == "SetWindowOffsetsToDefaults");

        ILCursor cursor = new(new ILContext(method));

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchCall("SDL2.SDL", "SDL_SetWindowPosition")
        )) {
            throw new Exception("Unable to patch window position set2. (no call)");
        }
        int last = cursor.Index;
        cursor.TryGotoPrev(MoveType.Before, instr => instr.MatchLdfld(out var _));
        cursor.RemoveRange(last - cursor.Index);
        cursor.EmitCallvirt(call);
        cursor.Index -= 2;
        cursor.EmitLdarg0();
    }

    [MonoModILInject("CreateWindow")]
    static void PatchWindowCreate(MethodDefinition method, CustomAttribute attribute) {
        if (!method.HasBody) {
            throw new Exception("Unable to patch window create. (no body)");
        }
        TypeDefinition holder = MonoModRule.Modder.FindType("GameLogic").Resolve();
        FieldDefinition settings = holder.Fields.First((f) => f.Name == "settingsData");
        FieldDefinition windowXPos = settings.FieldType.Resolve().Fields.First((f) => f.Name == "windowXPos");
        FieldDefinition windowYPos = settings.FieldType.Resolve().Fields.First((f) => f.Name == "windowYPos");

        MethodDefinition setDefault = holder.Methods.First((f) => f.Name == "SetWindowOffsetsToDefaults");
        ILCursor referenceCursor = new(new ILContext(setDefault));
        MethodReference get = null;
        referenceCursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt(out get));

        ILCursor cursor = new(new ILContext(method));
        cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdarg(4 - 1),
            instr => instr.MatchCall("SDL2.SDL", "SDL_WINDOWPOS_CENTERED_DISPLAY")
        );
        cursor.RemoveRange(2);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(settings);
        cursor.EmitLdfld(windowXPos);
        cursor.EmitCallvirt(get);

        cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdarg(4 - 1),
            instr => instr.MatchCall("SDL2.SDL", "SDL_WINDOWPOS_CENTERED_DISPLAY")
        );
        cursor.RemoveRange(2);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(settings);
        cursor.EmitLdfld(windowYPos);
        cursor.EmitCallvirt(get);
    }

    [MonoModILInject("GameLoop")]
    static void PatchWindowPositionSaveOnExit(MethodDefinition method, CustomAttribute attribute) {
        if (!method.HasBody) {
            throw new Exception("Unable to patch window position set save. (no body)");
        }

        ILCursor cursor = new(new ILContext(method));

        if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld("GameLogic","settingsData"),
            instr => instr.MatchCallvirt("Settings", "GetConfig"),
            instr => instr.MatchCallvirt("ConfigFile", "WriteFile")
        )) {
            throw new Exception("Unable to patch window position set. (no call)");
        }
        FieldReference settings = cursor.Next.Next.Operand as FieldReference;

        TypeDefinition holder = MonoModRule.Modder.FindType("Settings").Resolve();
        MethodDefinition call = holder.Methods.First((f) => f.Name == "SaveWindowPosToCurrent");

        TypeDefinition gameL = MonoModRule.Modder.FindType("GameLogic").Resolve();
        FieldDefinition window = gameL.Fields.First((f) => f.Name == "gameWindow");

        cursor.EmitLdarg0();
        cursor.EmitLdfld(settings);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(window);
        cursor.Emit(OpCodes.Callvirt, call);
    }
}