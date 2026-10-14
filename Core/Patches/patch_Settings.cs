
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using SDL2;
using System;
using System.Linq;

internal class patch_Settings : Settings {
    [MonoModIgnore] public patch_Settings(string filePath) : base(filePath) { }
    [MonoModIgnore] private static extern Maybe<int> ParseInt(string value);
    [MonoModIgnore] private static extern string IntToString(int value);

    public SettingsItem<int> windowXPos;
    public SettingsItem<int> windowYPos;

    public void SaveWindowPosToCurrent(Window window) {
        SDL.SDL_GetWindowPosition(window.window, out int x, out int y);
        windowXPos.Set(x);
        windowYPos.Set(y);
    }
    public void SaveWindowPosAsValue(int x, int y) {
        windowXPos.Set(x);
        windowYPos.Set(y);
    }
    public void WindowCtor() {
        int def = SDL.SDL_WINDOWPOS_CENTERED_DISPLAY(AppConsts.field_1008 ? 1 : 0);
        windowXPos = new SettingsItem<int>(GetConfig(), "QuintWindowPos.X", def, new Func<string, Maybe<int>>(ParseInt), new Func<int, string>(IntToString));
        windowYPos = new SettingsItem<int>(GetConfig(), "QuintWindowPos.Y", def, new Func<string, Maybe<int>>(ParseInt), new Func<int, string>(IntToString));
    }


    [MonoModILInject(".ctor")]
    public static void PatchWindowPositionSetting(MethodDefinition method, CustomAttribute attribute) {

        MonoModRule.Modder.Log("Patching window position settings init");

        if (!method.HasBody) {
            throw new Exception("Unable to patch window position settings init. (no body)");
        }

        ILCursor cursor = new(new ILContext(method));

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchCallvirt("Settings", "SetConfig")
        )) {
            throw new Exception("Unable to patch window position settings init. (no call)");
        }

        TypeDefinition holder = MonoModRule.Modder.FindType("Settings").Resolve();
        MethodDefinition call = holder.Methods.First((f) => f.Name == "WindowCtor");

        cursor.EmitLdarg0();
        cursor.Emit(OpCodes.Call, call);
    }
}
