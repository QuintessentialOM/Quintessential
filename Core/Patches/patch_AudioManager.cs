using MonoMod;
using Quintessential;
using System;
using System.Collections.Generic;
using System.IO;
internal class patch_AudioManager {
    private static readonly Dictionary<string, AudioFile> AudioFiles = [];
    [MonoModIgnore]
    public static extern AudioFile orig_LoadAudioFile(string filePath);

    public static AudioFile LoadAudioFile(string filePath) {
        filePath = Path.GetRelativePath("Content", filePath);
        if (AudioFiles.TryGetValue(filePath, out AudioFile value)) return value;

        AudioFile file;
        for (int i = QuintessentialLoader.ModContentDirectories.Count - 1; i >= 0; i--) {
            string contentDir = QuintessentialLoader.ModContentDirectories[i];
            try {
                string path = Path.Combine(contentDir);
                if (!File.Exists(path)) continue;
                file = orig_LoadAudioFile(path);
                file.sourceFileName = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(filePath));
                AudioFiles[filePath] = file;
                return file;
            } catch {}
        }
        if (!File.Exists(Path.Combine("Content", filePath))) throw new Exception("Failed to find file at path: " + filePath);
        file = orig_LoadAudioFile(Path.Combine("Content", filePath));
        file.sourceFileName = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
        AudioFiles[filePath] = file;
        return file;
    }
}
