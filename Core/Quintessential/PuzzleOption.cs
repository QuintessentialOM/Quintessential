using System.Collections.Generic;

namespace Quintessential;

/// <summary>
/// Puzzle data stored as a string
/// </summary>
public class PuzzleOption{

    public Identifier ID;
    public LocString Name, SectionName;
	public int length;
    public PuzzleOptionType Type;

	private List<LocString> choices;

    /// <summary>
    /// Boolean data, stored as ID present or not 
    /// </summary>
    /// <param name="id">The id of the PuzzleOption.</param>
    /// <param name="name">The localized name of the PuzzleOption.</param>
    /// <param name="sectionName">The localized name of the section that this PuzzleOption belongs to.</param>
    /// <param name="length"></param>// TODO what does this parameter do?
    /// <returns>The created PuzzleOption</returns>
    public static PuzzleOption BoolOption(Identifier id, LocString name, LocString sectionName, int length){
		return new PuzzleOption{
			ID = id,
			Name = name,
			SectionName = sectionName,
			Type = PuzzleOptionType.Boolean,
            length = length,
        };
	}

    // TODO fix issues caused by choice being localised
    /// <summary> 
    /// Multi choice data, stored as <c>{ID}__{choice}</c>
    /// </summary>
    /// <param name="id">The id of the PuzzleOption.</param>
    /// <param name="name">The localized name of the PuzzleOption.</param>
    /// <param name="sectionName">The localized name of the section that this PuzzleOption belongs to.</param>
    /// <param name="choices">The names of the choices</param>
    /// <returns>The created PuzzleOption</returns>
    public static PuzzleOption MultiChoiceOption(Identifier id, LocString name, LocString sectionName, params LocString[] choices){
		return new PuzzleOption{
			ID = id,
			Name = name,
			SectionName = sectionName,
			Type = PuzzleOptionType.MultiChoice,
			choices = [.. choices]
        };
	}

    /// <summary>
    /// Puzzle option for choosing a specific part, stored as <c>{ID}__{part ID}</c>
    /// </summary>
    /// <param name="id">The id of the PuzzleOption.</param>
    /// <param name="name">The localized name of the PuzzleOption.</param>
    /// <param name="sectionName">The localized name of the section that this PuzzleOption belongs to.</param>
    /// <returns>The created PuzzleOption</returns>
    public static PuzzleOption PartTypeOption(Identifier id, LocString name, LocString sectionName){
		return new PuzzleOption{
			ID = id,
			Name = name,
			SectionName = sectionName,
			Type = PuzzleOptionType.Part
		};
	}

    /// <summary>
    /// Puzzle option for choosing a specific part, stored as <c>{ID}__{atom ID}</c>
    /// </summary>
    /// <param name="id">The id of the PuzzleOption.</param>
    /// <param name="name">The localized name of the PuzzleOption.</param>
    /// <param name="sectionName">The localized name of the section that this PuzzleOption belongs to.</param>
    /// <returns>The created PuzzleOption</returns>
    public static PuzzleOption AtomTypeOption(Identifier id, LocString name, LocString sectionName){
		return new PuzzleOption{
			ID = id,
			Name = name,
			SectionName = sectionName,
			Type = PuzzleOptionType.Atom
		};
	}

    // Getters that each correspond to a PuzzleOptionType

    /// <summary>
    /// Get boolean Option state for the provided puzzle.
    /// </summary>
    /// <param name="from">The puzzle to check for.</param>
    public bool EnabledIn(Puzzle from){
		return ((patch_Puzzle)(object)from).CustomPermissions?.Contains(ID) ?? false;
	}

    /// <summary>
    /// Get multi choice Option state for the provided puzzle.
    /// </summary>
    /// <param name="from">The puzzle to check for.</param>
    public string ChoiceIn(Puzzle from){
		foreach(string permission in ((patch_Puzzle)(object)from).CustomPermissions)
			if(permission.StartsWith(ID + "__"))
				return permission[(ID.ToString().Length + 2)..];
		return null;
	}

    /// <summary>
    /// Get part choice Option state for the provided puzzle.
    /// </summary>
    /// <param name="from">The puzzle to check for.</param>
    public PartType PartIn(Puzzle from){
		string choice = ChoiceIn(from);
		foreach(PartType type in PartTypes.partTypes)
			if(type.id.Equals(choice)) // TODO use new part id
				return type;

		return null;
	}

    /// <summary>
    /// Get atom choice Option state for the provided puzzle.
    /// </summary>
    /// <param name="from">The puzzle to check for.</param>
    public AtomType AtomIn(Puzzle from){
		string choice = ChoiceIn(from);
		foreach(AtomType type in AtomTypes.atoms)
			if(((patch_AtomType)(object)type).Id.Equals(choice))
				return type;

		return null;
	}

    /// <summary>
    /// Set multi boolean Option state for the provided puzzle.
    /// </summary>
    /// <param name="from">The puzzle to set for.</param>
    /// <param name="enabled">The new boolean state.</param>
    public void SetEnabledIn(Puzzle from, bool enabled){
		if(enabled)
			((patch_Puzzle)(object)from).CustomPermissions.Add(ID);
		else
			((patch_Puzzle)(object)from).CustomPermissions.Remove(ID);
	}

    /// <summary>
    /// Set multi choice Option state for the provided puzzle.
    /// </summary>
    /// <param name="from">The puzzle to set for.</param>
    /// <param name="choice">The new state.</param>
    public void SetChoiceIn(Puzzle from, string choice){ // TODO localized string fix here too
		var perms = ((patch_Puzzle)(object)from).CustomPermissions;
		perms.RemoveWhere(s => s.ToString().StartsWith(ID + "__"));
		perms.Add(ID + "__" + choice);
	}

    /// <summary>
    /// Set atom choice Option state for the provided puzzle.
    /// </summary>
    /// <param name="from">The puzzle to set for.</param>
    /// <param name="atom">The new atom.</param>
    public void SetAtomIn(Puzzle from, AtomType atom){
		SetChoiceIn(from, ((patch_AtomType)(object)atom).Id);
	}

    /// <summary>
    /// Set part choice Option state for the provided puzzle.
    /// </summary>
    /// <param name="from">The puzzle to set for.</param>
    /// <param name="part">The new part.</param>
    public void SetPartIn(Puzzle from, PartType part){
		SetChoiceIn(from, part.id);
	}
}

public enum PuzzleOptionType{
	Boolean,
	MultiChoice,
	Part,
	Atom,
} // TODO add Tag based puzzle option
// TODO actually use this enum for option type validation