using MonoMod;

internal class patch_WorkshopManager {
    [MonoModReplace]
    public void UploadPuzzle() { }

    [MonoModPublic] [MonoModIgnore]
    public extern string CustomPuzzlePath(Puzzle puzzle);
}
