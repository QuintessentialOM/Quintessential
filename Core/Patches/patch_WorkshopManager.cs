using MonoMod;

internal class patch_WorkshopManager {

    [MonoModPublic] [MonoModIgnore]
    public extern string CustomPuzzlePath(Puzzle puzzle);
}
