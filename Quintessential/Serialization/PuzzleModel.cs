using System.Collections.Generic;

using Bond = class_277;
using BondType = enum_126;
using PermissionFlags = enum_149;
using ProductionInfo = class_261;
//using ChamberType = class_183;
using Chamber = class_189;

namespace Quintessential.Serialization;

public class PuzzleModel {

	// display name, internal name, journal author
	public string Name, ID, Author;
	// the inputs
	public IList<PuzzleIoM> Inputs = new List<PuzzleIoM>();
	// the outputs
	public IList<PuzzleIoM> Outputs = new List<PuzzleIoM>();
	// vanilla permission info
	public PermissionFlags PermissionFlags;
	// modded permisisons, can be used for parts, instructions, or anything else
	public ISet<string> CustomPermissions = new HashSet<string>();
	// number of times you can place each part
	// 0 = disallowed, negative = unlimited, not present = default (unlimited for non-unique parts)
	public IDictionary<string, int> PartQuotas = new Dictionary<string, int>();
	// set of highlighted hexes
	public ISet<HexIndexM> Highlights = new HashSet<HexIndexM>();
	// production-related stuff, or null for non-production puzzles
	public ProductionInfoM ProductionInfo = null;

	public static PuzzleModel FromPuzzle(Puzzle puzzle) {
		PuzzleModel model = new PuzzleModel();
		model.ID = puzzle.field_2766;
		foreach(var @in in puzzle.field_2770)
			model.Inputs.Add(new PuzzleIoM(@in));
		foreach(var @out in puzzle.field_2771)
			model.Outputs.Add(new PuzzleIoM(@out));
		model.PermissionFlags = puzzle.field_2773;
		model.Name = puzzle.field_2767.method_620();
		model.Author = puzzle.field_2768.method_1085() ? puzzle.field_2768.method_1087() : "";
		foreach(var item in puzzle.field_2774)
			model.Highlights.Add(new HexIndexM(item));
		if(puzzle.field_2779.method_1085())
			model.ProductionInfo = new ProductionInfoM(puzzle.field_2779.method_1087());
		return model;
	}

	public class HexIndexM {
		public string Pos;

		public HexIndexM(HexIndex ind) {
			Pos = ind.Q + "," + ind.R;
		}
	}

	public class PuzzleIoM {
		public MoleculeM Molecule;

		// for an output, the number of times this must be output to complete the puzzle.
		// negative = 6
		// for an input, the number of times this input will produce a molecule
		// negative or 0 = unlimited
		public int Amount = 6;

		public PuzzleIoM(PuzzleInputOutput io) {
			Molecule = new MoleculeM(io.field_2813);
		}
	}

	public class MoleculeM {
		public IList<AtomM> Atoms = new List<AtomM>();
		public IList<BondM> Bonds = new List<BondM>();

		public MoleculeM(Molecule mol) {
			foreach(var atom in mol.method_1100())
				Atoms.Add(new AtomM(((patch_AtomType)(object)atom.Value.field_2275).QuintAtomType, new HexIndexM(atom.Key)));
			foreach(var bond in mol.method_1101())
				Bonds.Add(new BondM(bond));

		}
	}

	public class AtomM {
		public string AtomType;
		public HexIndexM Position;

		public AtomM(string type, HexIndexM hex) {
			AtomType = type;
			Position = hex;
		}
	}

	public class BondM {
		public HexIndexM A, B;
		public ISet<string> BondTypes = new HashSet<string>();

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
		public IList<HexIndexM> Nodes = new List<HexIndexM>();
	}

	public class ProductionInfoM {
		public IList<ChamberM> Chambers = new List<ChamberM>();
		public bool Isolation = false;

		public ProductionInfoM(ProductionInfo info) {
			foreach(var chamber in info.field_2071) {
				Chambers.Add(new ChamberM(chamber));
			}
			Isolation = info.field_2077;
		}
	}

	public class ChamberM {
		public string ChamberType;
		public HexIndexM Position;

		public ChamberM(Chamber chamber) {
			ChamberType = chamber.field_1747.field_1727;
			Position = new HexIndexM(chamber.field_1746);
		}
	}

	public class HintM {
		public HexIndexM Position;
		public int AnchorType;
		public string Text;
	}
}
