using Quintessential.Serialization;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Quintessential;

/// <summary>
/// Metadata for a mod
/// </summary>
public class ModMeta {

    public string ModId { get; set; } = "";

    [JsonConverter(typeof(VersionJsonConverter))]
    public Version Version { get; set; }
    public string ModPageURL { get; set; } = "";
    public string DLL { get; set; } = "";
    public string[] Authors { get; set; } = [];
    public string Icon { get; set; } = "";
    public string Mappings { get; set; } = "";
    public Dictionary<string, VersionRange> Dependencies { get; set; } = [];
    public string[] Conflicts { get; set; } = [];

    [JsonIgnore] public string PathToDirectory;
    [JsonIgnore] public Texture IconCache = null;
}
