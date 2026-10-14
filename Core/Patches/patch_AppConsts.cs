#pragma warning disable CS0649 // never assigned to

using MonoMod;

class patch_AppConsts {

    [MonoModRemoveReadOnly]
    public static readonly bool isDevEnv; // isDevEnv

    [MonoModRemoveReadOnly]
    public static readonly bool showLogWindow; // showLogWindow

    [MonoModRemoveReadOnly]
    public static readonly bool allowPseudo; // allowPseudo

    [MonoModRemoveReadOnly]
    public static readonly bool logSimulationState; // logSimulationState
}