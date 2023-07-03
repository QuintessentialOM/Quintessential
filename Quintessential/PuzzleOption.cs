using System.Collections.Generic;

namespace Quintessential;

using PartType = class_139;
using PartTypes = class_191;
using AtomTypes = class_175;

public class PuzzleOption{
	
	// Puzzle options are always saved as strings
	// booleans -> ID present or not
	// multi-choice -> {ID}::{choice}
	// atom -> {ID}::{atom ID}
	// part -> {ID}::{part ID}
	
	public string ID, Name, SectionName;
	public PuzzleOptionType Type;

	private List<string> choices;

	public static PuzzleOption BoolOption(string id, string name, string sectionName){
		return new PuzzleOption{
			ID = id,
			Name = name,
			SectionName = sectionName,
			Type = PuzzleOptionType.Boolean
		};
	}
	
	public static PuzzleOption MultiChoiceOption(string id, string name, string sectionName, params string[] choices){
		return new PuzzleOption{
			ID = id,
			Name = name,
			SectionName = sectionName,
			Type = PuzzleOptionType.MultiChoice,
			choices = new List<string>(choices)
		};
	}
	
	public static PuzzleOption PartTypeOption(string id, string name, string sectionName){
		return new PuzzleOption{
			ID = id,
			Name = name,
			SectionName = sectionName,
			Type = PuzzleOptionType.Part
		};
	}
	
	public static PuzzleOption AtomTypeOption(string id, string name, string sectionName){
		return new PuzzleOption{
			ID = id,
			Name = name,
			SectionName = sectionName,
			Type = PuzzleOptionType.Atom
		};
	}

	// Getters that each correspond to a PuzzleOptionType
	
	public bool EnabledIn(Puzzle from){
		return Convert(from).CustomPermissions.Contains(ID);
	}

	public string ChoiceIn(Puzzle from){
		foreach(string permission in Convert(from).CustomPermissions)
			if(permission.StartsWith(ID + "::"))
				return permission.Substring(ID.Length + 2);
		return null;
	}

	public PartType PartIn(Puzzle from){
		string choice = ChoiceIn(from);
		foreach(PartType type in PartTypes.field_1785)
			if(type.field_1528.Equals(choice))
				return type;

		return null;
	}

	public AtomType AtomIn(Puzzle from){
		string choice = ChoiceIn(from);
		foreach(AtomType type in AtomTypes.field_1691)
			if(Convert(type).QuintAtomType.Equals(choice))
				return type;

		return null;
	}

	public void SetEnabledIn(Puzzle from, bool enabled){
		if(enabled)
			Convert(from).CustomPermissions.Add(ID);
		else
			Convert(from).CustomPermissions.Remove(ID);
	}
	
	public void SetChoiceIn(Puzzle from, string choice){
		var perms = Convert(from).CustomPermissions;
		perms.RemoveWhere(s => s.StartsWith(ID + "::"));
		perms.Add(ID + "::" + choice);
	}

	public void SetAtomIn(Puzzle from, AtomType atom){
		SetChoiceIn(from, Convert(atom).QuintAtomType);
	}

	public void SetPartIn(Puzzle from, PartType part){
		SetChoiceIn(from, part.field_1528);
	}

	private static patch_Puzzle Convert(Puzzle from){
		return (patch_Puzzle)(object)from;
	}

	private static patch_AtomType Convert(AtomType from){
		return (patch_AtomType)(object)from;
	}
}

public enum PuzzleOptionType{
	Boolean,
	MultiChoice,
	Part,
	Atom
}