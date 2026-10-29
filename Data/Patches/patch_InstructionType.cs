using MonoMod;
using Quintessential;
using Quintessential.Settings;
using SDL2;
using System;
using System.Collections.Generic;

internal class patch_InstructionType : InstructionType {
    [MonoModIgnore]
    internal char id;

    [MonoModInternalF]
    public SDL.SDLKey hotkey;

    [MonoModIgnore]
    public extern SDL.SDLKey orig_GetHotkey();
    public SDL.SDLKey GetHotkey() {
        if (Hotkey != null) return SDL.SDL_GetKeyFromName(Hotkey.Key);
        return orig_GetHotkey();
    }

    public Keybinding Hotkey;
    public Predicate<HashSet<Identifier>> CustomPermissionCheck;
}
