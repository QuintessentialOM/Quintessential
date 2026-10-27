using MonoMod;
using System.Collections.Generic;

internal class patch_SoundAssets {
    static internal List<Sound> Sounds = [];

    [MonoModReplace]
    public void ResetPlayedThisFrame() {
        foreach (var sound in Sounds) {
            sound.playedThisFrame = false;
        }
    }
}
