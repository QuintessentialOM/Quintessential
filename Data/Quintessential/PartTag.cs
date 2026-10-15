using System;
using System.Collections.Generic;

namespace Quintessential;

/// <summary>
/// A collection of <see cref="PartType"/> <see cref="Identifier"/>s.
/// </summary>
public class PartTag : Tag {
    public static readonly string FileName = "partTags.jsonc";
    /// <inheritdoc cref="Tag(Identifier, bool)"/>
    public PartTag(Identifier id, bool isTable = false) : base(id, isTable) { }


    /// <summary>
    /// A Dictionary of all <see cref="PartTag"/>s added.
    /// </summary>
    public static Dictionary<Identifier, PartTag> PartTags = [];
    /// <summary>
    /// Dumps all <see cref="PartTag"/>s.
    /// </summary>
    public static void DumpTags() {
        DumpTags(PartTags, FileName);
    }


    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified part.
    /// </summary>
    /// <param name="part">The atom to get the id for.</param>
    public bool HasPart(PartType part) => HasEntry(part.Id);
    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified part.
    /// </summary>
    /// <param name="part">The atom to get the id for.</param>
    public bool HasPart(Part part) => HasEntry(part.GetType().Id);
    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified part.<br/>
    /// Additionally returns the mapped value from table tags or null if the tag isn't one.
    /// </summary>
    /// <param name="part">The atom to get the id for.</param>
    /// <param name="mapped"><inheritdoc cref="Tag.HasEntry(Identifier, out Identifier?)"/></param>
    public bool HasPart(PartType part, out Identifier? mapped) => HasEntry(part.Id, out mapped);
    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified part.<br/>
    /// Additionally returns the mapped value from table tags or null if the tag isn't one.
    /// </summary>
    /// <param name="part">The atom to get the id for.</param>
    /// <param name="mapped"><inheritdoc cref="Tag.HasEntry(Identifier, out Identifier?)"/></param>
    public bool HasPart(Part part, out Identifier? mapped) => HasEntry(part.GetType().Id, out mapped);
    public Identifier? GetMapped(PartType part) => GetMapped(part.Id);
    public Identifier? GetMapped(Part part) => GetMapped(part.GetType().Id);

}

public class PartTagJsonConverter : TagJsonConverter<PartTag> {
    private static PartTagJsonConverter Instance;
    private static bool wasInit = false;
    public static PartTagJsonConverter Get() {
        if (!wasInit) {
            Instance = new(PartTag.PartTags, (id, isTable) => new PartTag(id, isTable));
            wasInit = true;
        }
        return Instance;
    }

    private PartTagJsonConverter(Dictionary<Identifier, PartTag> GlobalTags, Func<Identifier, bool, PartTag> CtorForType) : base(GlobalTags, CtorForType) {}
}
