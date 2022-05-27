using System.Collections.Generic;
using System.Linq;

using Bond = class_277;
using BondType = enum_126;
using PermissionFlags = enum_149;
using ProductionInfo = class_261;
using ChamberType = class_183;
using Chamber = class_189;
using AtomTypes = class_175;

namespace Quintessential.Serialization;

public class PuzzleModel {

	// display name, internal name, journal author
	public string Name, ID, Author;
	// the inputs
	public List<PuzzleIoM> Inputs = new();
	// the outputs
	public List<PuzzleIoM> Outputs = new();
	// vanilla permission info
	public PermissionFlags PermissionFlags;
	// modded permisisons, can be used for parts, instructions, or anything else
	public HashSet<string> CustomPermissions = new();
	// number of times you can place each part
	// 0 = disallowed, negative = unlimited, not present = default (unlimited for non-unique parts)
	public Dictionary<string, int> PartQuotas = new();
	// set of highlighted hexes
	public HashSet<HexIndexM> Highlights = new();
	// production-related stuff, or null for non-production puzzles
	public ProductionInfoM ProductionInfo = null;

	public static PuzzleModel FromPuzzle(Puzzle puzzle) {
		PuzzleModel model = new();
		model.ID = puzzle.field_2766;
		foreach(var @in in puzzle.field_2770)
			model.Inputs.Add(new PuzzleIoM(@in));
		foreach(var @out in puzzle.field_2771)
			model.Outputs.Add(new PuzzleIoM(@out));
		model.PermissionFlags = puzzle.field_2773;
		model.Name = puzzle.field_2767?.method_620() ?? "Unnamed";
		model.Author = puzzle.field_2768.method_1085() ? puzzle.field_2768.method_1087() : "";
		foreach(var item in puzzle.field_2774)
			model.Highlights.Add(new HexIndexM(item));
		if(puzzle.field_2779.method_1085())
			model.ProductionInfo = new ProductionInfoM(puzzle.field_2779.method_1087());
		return model;
	}

	public static Puzzle FromModel(PuzzleModel model) {
		Puzzle ret = new();
		ret.field_2766 = model.ID;
		ret.field_2767 = class_134.method_253(model.Name, string.Empty);
		ret.field_2770 = model.Inputs.Select(k => k.FromModel()).ToArray();
		ret.field_2771 = model.Outputs.Select(k => k.FromModel()).ToArray();
		ret.field_2773 = model.PermissionFlags;
		ret.field_2768 = model.Author.Equals("") ? new Maybe<string>(false, null) : model.Author;
		ret.field_2774 = model.Highlights.Select(k => k.FromModel()).ToArray();
		if(model.ProductionInfo != null && model.ProductionInfo.Chambers.Count > 0)
			ret.field_2779 = model.ProductionInfo.FromModel();
		return ret;
	}

	public class HexIndexM {
		public string Pos;

		public HexIndexM(HexIndex ind) {
			Pos = ind.Q + "," + ind.R;
		}

		public HexIndexM(){}

		public HexIndex FromModel() {
			return new(Q(), R());
		}

		public int Q() {
			return int.Parse(Pos.Split(',')[0]);
		}

		public int R() {
			return int.Parse(Pos.Split(',')[1]);
		}
	}

	public class PuzzleIoM {
		public MoleculeM Molecule;

		// for an output, the number of times this must be output to complete the puzzle.
		// negative = 6
		// for an input, the number of times this input will produce a molecule
		// negative or 0 = unlimited
		public int Amount = -1;

		public PuzzleIoM(PuzzleInputOutput io) {
			Molecule = new MoleculeM(io.field_2813);
		}

		public PuzzleIoM(){}

		public PuzzleInputOutput FromModel() {
			return new(Molecule.FromModel());
		}
	}

	public class MoleculeM {
		public List<AtomM> Atoms = new();
		public List<BondM> Bonds = new();
		public string Name = "";

		public MoleculeM(Molecule mol) {
			foreach(var atom in mol.method_1100())
				Atoms.Add(new AtomM(((patch_AtomType)(object)atom.Value.field_2275).QuintAtomType, new HexIndexM(atom.Key)));
			foreach(var bond in mol.method_1101())
				Bonds.Add(new BondM(bond));
			Name = mol.field_2639.method_1090(null)?.method_620() ?? "";
		}

		public MoleculeM(){}

		public Molecule FromModel() {
			Molecule ret = new();
			foreach(var item in Atoms)
				ret.method_1105(item.FromModel(), item.Position.FromModel());
			foreach(var item in Bonds)
				ret.method_1111((BondType)item.BondBits(), item.A.FromModel(), item.B.FromModel());
			if(!Name.Equals(""))
				ret.field_2639 = class_134.method_253(Name, string.Empty);
			return ret;
		}
	}

	public class AtomM {
		public string AtomType;
		public HexIndexM Position;

		public AtomM(string type, HexIndexM hex) {
			AtomType = type;
			Position = hex;
		}

		public AtomM(){}

		public Atom FromModel() {
			return new Atom(AtomTypes.field_1691.First(k => ((patch_AtomType)(object)k).QuintAtomType.Equals(AtomType)));
		}
	}

	public class BondM {
		public HexIndexM A, B;
		public HashSet<string> BondTypes = new();

		public BondM(Bond bond) {
			A = new HexIndexM(bond.field_2187);
			B = new HexIndexM(bond.field_2188);
			if((bond.field_2186 & BondType.Standard) == BondType.Standard)
				BondTypes.Add("standard");
			if((bond.field_2186 & BondType.Prisma0) == BondType.Prisma0)
				BondTypes.Add("triplex_0");
			if((bond.field_2186 & BondType.Prisma1) == BondType.Prisma1)
				BondTypes.Add("triplex_1");
			if((bond.field_2186 & BondType.Prisma2) == BondType.Prisma2)
				BondTypes.Add("triplex_2");
		}

		public BondM(){}

		public byte BondBits() {
			byte bits = 0;
			if(BondTypes.Contains("standard"))
				bits |= (byte)BondType.Standard;
			if(BondTypes.Contains("triplex_0"))
				bits |= (byte)BondType.Prisma0;
			if(BondTypes.Contains("triplex_1"))
				bits |= (byte)BondType.Prisma1;
			if(BondTypes.Contains("triplex_2"))
				bits |= (byte)BondType.Prisma2;
			return bits;
		}
	}

	public class PartM {
		public string PartType;
		public int Rotation;
		// only used for arms
		public int Extension;
		// for most parts, Nodes[0] = position
		// for tracks, Nodes = track segments
		// for conduits, Nodes[0] and Nodes[1] are the conduit positions
		// modded parts can also use these
		public List<HexIndexM> Nodes = new();
	}

	public class ProductionInfoM {
		public List<ChamberM> Chambers = new();
		public bool Isolation = false;

		public ProductionInfoM(ProductionInfo info) {
			foreach(var chamber in info.field_2071) {
				Chambers.Add(new ChamberM(chamber));
			}
			Isolation = info.field_2077;
		}

		public ProductionInfoM(){}

		public ProductionInfo FromModel() {
			ProductionInfo ret = new();
			ret.field_2071 = Chambers.Select(k => k.FromModel()).ToArray();
			ret.field_2077 = Isolation;
			return ret;
		}
	}

	public class ChamberM {
		public string ChamberType;
		public HexIndexM Position;

		public ChamberM(Chamber chamber) {
			ChamberType = chamber.field_1747.field_1727;
			Position = new HexIndexM(chamber.field_1746);
		}

		public ChamberM(){}

		public Chamber FromModel() {
			return new(Position.Q(), Position.R(), Puzzles.field_2932.First(k => k.field_1727.Equals(ChamberType)));
		}
	}

	public class HintM {
		public HexIndexM Position;
		public int AnchorType;
		public string Text;
	}
}
