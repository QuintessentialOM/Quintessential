using MonoMod;
using System.Collections.Generic;

internal class patch_SoundAssets {
    static internal List<Sound> Sounds = [];

    [MonoModReplace]
    public void ResetPlayingSounds() {
        foreach (var sound in Sounds) {
            sound.isPlaying = false;
        }
    }
}
