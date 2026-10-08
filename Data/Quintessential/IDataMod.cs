using Quintessential.Serialization;
using System.Collections.Generic;
using System.IO;

namespace Quintessential;

/// <summary>
/// An extension for <see cref="QuintessentialMod"/>s to enable automatic interaction with the Quintessential Data Api.
/// </summary>
public interface IDataMod {

    /// <summary>
    /// Called inbetween <see cref="QuintessentialMod.LoadContent()"/> and<br/>
    /// <see cref="QuintessentialMod.LoadCompatContent()"/> to automatically<br/>
    /// load data objects ( like <see cref="Tag"/>s ) from the mod's contend directories.
    /// </summary>
    public void LoadTags() {
        if (this is QuintessentialMod mod) {
            LoadTagsFromFiles(mod);
        }
    }

    /// <summary>
    /// Called to load tags from the mod's contend directories.
    /// </summary>
    public static sealed void LoadTagsFromFiles(QuintessentialMod mod) {
        if (File.Exists(Path.Combine(mod.Meta.PathToDirectory, "Content", "tags", AtomTag.FileName))) {
            DataSerializer.Deserialize<Dictionary<Identifier, AtomTag>>(Path.Combine(mod.Meta.PathToDirectory, "Content", "tags", AtomTag.FileName));
            Logger.Log($"Loaded {AtomTag.FileName} from '{mod.ModId}'");
        }
        if (File.Exists(Path.Combine(mod.Meta.PathToDirectory, "Content", "tags", PartTag.FileName))) {
            DataSerializer.Deserialize<Dictionary<Identifier, PartTag>>(Path.Combine(mod.Meta.PathToDirectory, "Content", "tags", PartTag.FileName));
            Logger.Log($"Loaded {PartTag.FileName} from '{mod.ModId}'");
        }
    }
}
