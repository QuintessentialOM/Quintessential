using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using SDL2;
using System;
using System.Linq;

internal class patch_Window : Window {
    [MonoModIgnore] public patch_Window(nint window, nint renderWindow) : base(window, renderWindow) { }

    public void FixWindowAboveGrabRange() {
        int x = ((patch_Settings)(object)GameLogic.instance.settingsData).windowXPos.Get();
        int y = ((patch_Settings)(object)GameLogic.instance.settingsData).windowYPos.Get();
        //SDL.SDL_GetWindowPosition(window, out int x, out int y);
        if (y < 5) {
            SDL.SDL_SetWindowPosition(window, x, y + 5);
            ((patch_Settings)(object)GameLogic.instance.settingsData).SaveWindowPosAsValue(x, y + 5);
        }
    }

    [MonoModILInject("SetFullscreen")]
    public static void PatchFixFullscreenStuckBug(MethodDefinition method, CustomAttribute attribute) {

        MonoModRule.Modder.Log("Patching window position set");

        if (!method.HasBody) {
            throw new Exception("Unable to patch window position set. (no body)");
        }
        ILCursor cursor = new(new ILContext(method));

        TypeDefinition holder = MonoModRule.Modder.FindType("Window").Resolve();
        MethodDefinition call = holder.Methods.First((f) => f.Name == "FixWindowAboveGrabRange");

        cursor.EmitLdarg1();
        cursor.Emit(OpCodes.Brtrue, cursor.Next);
        cursor.EmitLdarg0();
        cursor.EmitCallvirt(call);
    }
}
