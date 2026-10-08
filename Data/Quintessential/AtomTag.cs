using System;
using System.Collections.Generic;

namespace Quintessential;

/// <summary>
/// A collection of <see cref="AtomType"/> <see cref="Identifier"/>s.
/// </summary>
public class AtomTag : Tag {
    public static readonly string FileName = "atomTags.jsonc";
    /// <inheritdoc cref="Tag(Identifier, bool)"/>
    public AtomTag(Identifier id, bool isTable = false) : base(id, isTable) { }


    /// <summary>
    /// A Dictionary of all <see cref="AtomTag"/>s added.
    /// </summary>
    public static Dictionary<Identifier, AtomTag> AtomTags = [];
    /// <summary>
    /// Dumps all <see cref="AtomTag"/>s.
    /// </summary>
    public static void DumpTags() {
        DumpTags(AtomTags, FileName);
    }


    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified atom.
    /// </summary>
    /// <param name="atom">The atom to get the id for.</param>
    public bool HasAtom(AtomType atom) => HasEntry(atom.Id);
    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified atom.
    /// </summary>
    /// <param name="atom">The atom to get the id for.</param>
    public bool HasAtom(Atom atom) => HasEntry(atom.atomType.Id);
    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified atom.
    /// </summary>
    /// <param name="atom">The atom to get the id for.</param>
    public bool HasAtom(AtomReference atom ) => HasEntry(atom.atomType.Id);
    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified atom.<br/>
    /// Additionally returns the mapped value from table tags or null if the tag isn't one.
    /// </summary>
    /// <param name="atom">The atom to get the id for.</param>
    /// <param name="mapped"><inheritdoc cref="Tag.HasEntry(Identifier, out Identifier?)"/></param>
    public bool HasAtom(AtomType atom, out Identifier? mapped) => HasEntry(atom.Id, out mapped);
    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified atom.<br/>
    /// Additionally returns the mapped value from table tags or null if the tag isn't one.
    /// </summary>
    /// <param name="atom">The atom to get the id for.</param>
    /// <param name="mapped"><inheritdoc cref="Tag.HasEntry(Identifier, out Identifier?)"/></param>
    public bool HasAtom(Atom atom, out Identifier? mapped) => HasEntry(atom.atomType.Id, out mapped);
    /// <summary>
    /// <inheritdoc cref="Tag.HasEntry(Identifier)"/><br/>
    /// Corresponding to the specified atom.<br/>
    /// Additionally returns the mapped value from table tags or null if the tag isn't one.
    /// </summary>
    /// <param name="atom">The atom to get the id for.</param>
    /// <param name="mapped"><inheritdoc cref="Tag.HasEntry(Identifier, out Identifier?)"/></param>
    public bool HasAtom(AtomReference atom, out Identifier? mapped) => HasEntry(atom.atomType.Id, out mapped);
    public Identifier? GetMapped(AtomType atom) => GetMapped(atom.Id);
    public Identifier? GetPair(Atom atom) => GetMapped(atom.atomType.Id);
    public Identifier? GetMapped(AtomReference atom) => GetMapped(atom.atomType.Id);

}

public class AtomTagJsonConverter : TagJsonConverter<AtomTag> {
    private static AtomTagJsonConverter Instance;
    private static bool wasInit = false;
    public static AtomTagJsonConverter Get() {
        if (!wasInit) {
            Instance = new(AtomTag.AtomTags, (id, isTable) => new AtomTag(id, isTable));
            wasInit = true;
        }
        return Instance;
    }

    private AtomTagJsonConverter(Dictionary<Identifier, AtomTag> GlobalTags, Func<Identifier, bool, AtomTag> CtorForType) : base(GlobalTags, CtorForType) {}
}
