using MonoMod;
using System.Collections.Generic;
using System.IO;

internal class patch_Volume {
    [MonoModIgnore]
    private static Dictionary<string, float> volumeDict;
    private static Dictionary<string, float> pathVolumeDict = [];
    [MonoModReplace]
    private static float GetVolume(Sound sound) {
        if (GameLogic.instance.solutionQueue.Count > 0) return 0f;
        if (!pathVolumeDict.TryGetValue(sound.id, out float num)) {
            if (!volumeDict.TryGetValue(Path.GetFileNameWithoutExtension(sound.id), out num)) {
                return 1;
                //throw new OpusMagnumException(string.Format("Missing volume entry for sound '{0}'.", sound.id));
            }
        }
        return num;
    }

    public static void SetVoulme(Sound sound, float volume) {
        pathVolumeDict[sound.id] = volume;
    }
    public static void SetVoulme(string path, float volume) {
        pathVolumeDict[path] = volume;
    }
}
