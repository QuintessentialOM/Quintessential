using System;
using System.Collections.Generic;
using System.Linq;

using Bond = class_277;
using BondType = enum_126;
using PermissionFlags = enum_149;
using ProductionInfo = class_261;
using Chamber = class_189;
using Conduit = class_117;
using Vial = class_128;
using AtomTypes = class_175;

namespace Quintessential.Serialization;

public class PuzzleModel {

	// display name, internal name, journal author
	public string Name, ID, Author;
	// the inputs
	public List<PuzzleIoM> Inputs = new();
	// the outputs
	public List<PuzzleIoM> Outputs = new();
	// output multiplier
	public int OutputMultiplier = 1;
	// vanilla permission info
	public PermissionFlags PermissionFlags;
	// modded permisisons, can be used for parts, instructions, or anything else
	public HashSet<string> CustomPermissions = new();
	// set of highlighted hexes
	public HashSet<HexIndexM> Highlights = new();
	// production-related stuff, or null for non-production puzzles
	public ProductionInfoM ProductionInfo = null;

	public static PuzzleModel FromPuzzle(Puzzle puzzle) {
		PuzzleModel model = new(){
			ID = puzzle.field_2766,
			PermissionFlags = puzzle.field_2773,
			Name = puzzle.field_2767?.method_620() ?? "Unnamed",
			Author = puzzle.field_2768.method_1085() ? puzzle.field_2768.method_1087() : "",
			CustomPermissions = ((patch_Puzzle)(object)puzzle).CustomPermissions,
			OutputMultiplier = puzzle.field_2780
		};
		foreach(var @in in puzzle.field_2770)
			model.Inputs.Add(new PuzzleIoM(@in));
		foreach(var @out in puzzle.field_2771)
			model.Outputs.Add(new PuzzleIoM(@out));
		foreach(var item in puzzle.field_2774)
			model.Highlights.Add(new HexIndexM(item));
		if(puzzle.field_2779.method_1085())
			model.ProductionInfo = new ProductionInfoM(puzzle.field_2779.method_1087());
		return model;
	}

	public static Puzzle FromModel(PuzzleModel model) {
		Puzzle ret = new(){
			field_2766 = model.ID,
			field_2767 = class_134.method_253(model.Name, string.Empty),
			field_2770 = model.Inputs.Select(k => k.FromModel()).ToArray(),
			field_2771 = model.Outputs.Select(k => k.FromModel()).ToArray(),
			field_2773 = model.PermissionFlags,
			field_2768 = model.Author.Equals("") ? new Maybe<string>(false, null) : model.Author,
			field_2774 = model.Highlights.Select(k => k.FromModel()).ToArray(),
			field_2780 = model.OutputMultiplier
		};
		if(model.ProductionInfo != null && model.ProductionInfo.Chambers.Count > 0){
			ret.field_2779 = model.ProductionInfo.FromModel();
			// Calculate bounds
			ret.method_1247();
		}
		((patch_Puzzle)(object)ret).CustomPermissions = model.CustomPermissions;
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
		public int AmountOverride = 0;

		public PuzzleIoM(PuzzleInputOutput io) {
			Molecule = new MoleculeM(io.field_2813);
			AmountOverride = ((patch_PuzzleInputOutput)(object)io).AmountOverride;
		}

		public PuzzleIoM(){}

		public PuzzleInputOutput FromModel(){
			PuzzleInputOutput io = new PuzzleInputOutput(Molecule.FromModel());
			((patch_PuzzleInputOutput)(object)(io)).AmountOverride = AmountOverride;
			return io;
		}
	}

	public class MoleculeM {
		public List<AtomM> Atoms = new();
		public List<BondM> Bonds = new();
		public string Name = "";

		public MoleculeM(Molecule mol) {
			// clean molecules first
			mol = MoleculeEditorScreen.method_1134(mol);
			foreach(var atom in mol.method_1100())
				Atoms.Add(new AtomM(atom.Value, new HexIndexM(atom.Key)));
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
			return MoleculeEditorScreen.method_1133(ret, class_181.field_1716);
		}
	}

	public class AtomM {
		public string AtomType;
		public HexIndexM Position;

		public AtomM(Atom atom, HexIndexM hex) {
			AtomType = ((patch_AtomType)(object)atom.field_2275).QuintAtomType;
			Position = hex;
		}

		public AtomM(){}

		public Atom FromModel() {
			return new Atom(
				AtomTypes.field_1691.FirstOrDefault(k => AtomType.Equals(((patch_AtomType)(object)k).QuintAtomType))
				?? throw new Exception($"Atom type \"{AtomType}\" does not exist!")
			);
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

	public class ProductionInfoM {
		public List<ChamberM> Chambers = new();
		public List<ConduitM> Conduits = new();
		public List<VialM> Vials = new();
		public bool Isolation = false, ShrinkLeft = false, ShrinkRight = false;

		public ProductionInfoM(ProductionInfo info) {
			foreach(var chamber in info.field_2071)
				Chambers.Add(new ChamberM(chamber));
			foreach(Conduit conduit in info.field_2072)
				Conduits.Add(new ConduitM(conduit));
			foreach(Vial vial in info.field_2073)
				Vials.Add(new VialM(vial));
			ShrinkLeft = info.field_2075;
			ShrinkRight = info.field_2076;
			Isolation = info.field_2077;
		}

		public ProductionInfoM(){}

		public ProductionInfo FromModel() {
			ProductionInfo ret = new(){
				field_2071 = Chambers.Select(k => k.FromModel()).ToArray(),
				field_2072 = Conduits.Select(k => k.FromModel()).ToArray(),
				field_2073 = Vials.Select(k => k.FromModel()).ToArray(),
				field_2075 = ShrinkLeft,
				field_2076 = ShrinkRight,
				field_2077 = Isolation
			};
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

	public class ConduitM {
		public HexIndexM PosA, PosB;
		public List<HexIndexM> Shape = new();

		public ConduitM(){}

		public ConduitM(Conduit c) {
			foreach(HexIndex hex in c.field_1440)
				Shape.Add(new HexIndexM(hex));
			// TODO: when are there ever more than two?
			PosA = new HexIndexM(c.field_1441[0].field_1879);
			PosB = new HexIndexM(c.field_1441[1].field_1879);
		}

		public Conduit FromModel() {
			return new Conduit(PosA.Q(), PosA.R(), PosB.Q(), PosB.R(), Shape.Select(k => k.FromModel()).ToArray());
		}
	}

	public class VialM {
		public HexIndexM Position;
		public bool Top;
		public List<Pair<string, string>> Sprites = new();

		public VialM(){}

		public VialM(Vial v) {
			Position = new HexIndexM(v.field_1471);
			Top = v.field_1472;
			foreach(Tuple<class_256, class_256> sprites in v.field_1473)
				Sprites.Add(new(CleanName(sprites.Item1), CleanName(sprites.Item2)));
		}

		public Vial FromModel() {
			return new Vial(Position.Q(), Position.R(), Top,
				Sprites.Select(xs => Tuple.Create(class_235.method_615(xs.Left), class_235.method_615(xs.Right))).ToArray());
		}

		private static string CleanName(class_256 texture){
			string name = texture.field_2062.method_1087();
			if(name.StartsWith("Content/") || name.StartsWith("Content\\"))
				name = name.Substring("Content/".Length);
			return name;
		}
	}
}
