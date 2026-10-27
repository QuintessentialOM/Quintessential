using MonoMod;
using Quintessential;
using System;
using System.Collections.Generic;
using System.IO;
internal class patch_AudioManager {
    private static readonly Dictionary<string, AudioSource> AudioSources = [];
    [MonoModIgnore]
    public static extern AudioSource orig_CreateAudioSource(string filePath);

    public static AudioSource CreateAudioSource(string filePath) {
        if (AudioSources.TryGetValue(filePath, out AudioSource value)) return value;

        AudioSource file;
        for (int i = QuintessentialLoader.ModContentDirectories.Count - 1; i >= 0; i--) {
            string contentDir = QuintessentialLoader.ModContentDirectories[i];
            try {
                string path = Path.Combine(contentDir, filePath);
                if (!File.Exists(path)) continue;
                file = orig_CreateAudioSource(path);
                file.sourceFileName = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(filePath));
                AudioSources[filePath] = file;
                return file;
            } catch {}
        }
        if (!File.Exists(filePath)) throw new Exception("Failed to find file at path: " + filePath);
        file = orig_CreateAudioSource(filePath);
        file.sourceFileName = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
        AudioSources[filePath] = file;
        return file;
    }
}
