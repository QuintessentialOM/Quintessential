using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Quintessential.Serialization;
[DataContract]
public class PuzzleModel {

    [DataMember] public string NameKey { get; set; }
    [DataMember] public string ID { get; set; }
    [DataMember] public string Author { get; set; }
    [DataMember] public List<PuzzleIoM> Inputs { get; set; } = [];
    [DataMember] public List<PuzzleIoM> Outputs { get; set; } = [];
    [DataMember(EmitDefaultValue = false)] public int OutputMultiplier { get; set; } = 1;
	[JsonConverter(typeof(FlagEnumJsonConverter<PuzzlePermissions>))]
    [DataMember] public PuzzlePermissions PermissionFlags { get; set; }
    [DataMember] public List<string> CustomPermissions { get; set; } = [];
    [DataMember(EmitDefaultValue = false)] public HashSet<HexIndexM> Highlights { get; set; } = [];
    [DataMember(EmitDefaultValue = false)] public ProductionInfoM ProductionInfo { get; set; } = null;
    [DataMember(EmitDefaultValue = false)] public List<ConduitM> Conduits { get; set; } = null;

	public static PuzzleModel FromPuzzle(Puzzle puzzle) {
		PuzzleModel model = new(){
			ID = puzzle.puzzleId,
			PermissionFlags = puzzle.permissionFlags,
			NameKey = puzzle.puzzleName != null ? ((patch_LocString)(object)puzzle.puzzleName).Key : "",
			Author = puzzle.journalAuthor.HasValue() ? puzzle.journalAuthor.GetValue() : "",
			CustomPermissions = [..((patch_Puzzle)(object)puzzle).CustomPermissions ?? []],
			OutputMultiplier = puzzle.outputMultiplier
        };
		foreach(var @in in puzzle.inputs)
			model.Inputs.Add(@in);
		foreach(var @out in puzzle.outputs)
			model.Outputs.Add(@out);
		foreach(var item in puzzle.highlights)
			model.Highlights.Add(item);
		if(puzzle.productionInfo.HasValue())
			// if there's production cabinet info, use that
			model.ProductionInfo = puzzle.productionInfo.GetValue();
		else if (((patch_Puzzle)(object)puzzle).EngineConduits.HasValue())
		{
			// otherwise, populate the engine conduits
			model.Conduits = [.. ((patch_Puzzle)(object)puzzle).EngineConduits.GetValue()];
		}

		return model;
	}

	public static Puzzle FromModel(PuzzleModel model) {
		Puzzle ret = new(){
            puzzleId = model.ID,
            puzzleName = patch_Translations.TranslateByType(model.NameKey),
            inputs = [.. model.Inputs],
            outputs = [.. model.Outputs],
            permissionFlags = model.PermissionFlags,
            journalAuthor = model.Author.Equals("") ? new Maybe<string>(false, null) : model.Author,
            highlights = [.. model.Highlights],
            outputMultiplier = model.OutputMultiplier
		};
		if(model.ProductionInfo != null) {
			if (model.ProductionInfo.Chambers.Count > 0)
			{
				ret.productionInfo = (ProductionInfo)model.ProductionInfo;
				// Calculate bounds
				ret.CalculateCabinetBounds();
			}
		}
		else if (model.Conduits != null)
		{
			// if it's not a cabinet, use these
			((patch_Puzzle)(object)ret).EngineConduits = (PlacedConduit[])[.. model.Conduits];
		}
		((patch_Puzzle)(object)ret).CustomPermissions = [..model.CustomPermissions];

		return ret;
	}

    [DataContract]
    public class HexIndexM {
        [DataMember] public string Pos { get; set; }

        public HexIndexM() { }
        public static implicit operator HexIndexM (HexIndex ind) {
			return new() {
				Pos = ind.Q + "," + ind.R
			};
		}
        public static implicit operator HexIndex (HexIndexM ind) {
			return new(ind.Q(), ind.R());
		}

		public int Q() {
			return int.Parse(Pos.Split(',')[0]);
		}
		public int R() {
			return int.Parse(Pos.Split(',')[1]);
		}
	}

    [DataContract]
    public class PuzzleIoM {
        [DataMember] public MoleculeM Molecule { get; set; }
        [DataMember(EmitDefaultValue = false)] public int AmountOverride { get; set; } = 0;

        public PuzzleIoM() { }
        public static implicit operator PuzzleIoM (PuzzleInputOutput io) {
			return new() {
				Molecule = io.molecule,
				AmountOverride = ((patch_PuzzleInputOutput)(object)io).AmountOverride
			};
		}
        public static implicit operator PuzzleInputOutput (PuzzleIoM io) {
			PuzzleInputOutput _io = new(io.Molecule);
			((patch_PuzzleInputOutput)(object)_io).AmountOverride = io.AmountOverride;
			return _io;
		}
	}

    [DataContract]
    public class MoleculeM {
        [DataMember] public List<AtomM> Atoms { get; set; } = [];
        [DataMember] public List<BondM> Bonds { get; set; } = [];
        [DataMember] public string NameKey { get; set; } = "";

        public MoleculeM() { }
        public static implicit operator MoleculeM (Molecule mol) {
            MoleculeM toReturn = new() {
				NameKey = mol.displayName.HasValue() ? ((patch_LocString)(object)mol.displayName.GetValue()).Key : "",
			};
			foreach (var atom in mol.GetMonomer().GetAtoms())
                toReturn.Atoms.Add(new AtomM(atom.Value, atom.Key));
			foreach(var bond in mol.GetMonomer().GetBonds())
                toReturn.Bonds.Add(bond);
			return toReturn;
		}
        public static implicit operator Molecule ( MoleculeM mol) {
			Molecule monomer = new();
			foreach(var item in mol.Atoms)
                monomer.AddAtom(item, item.Position);
			foreach(var item in mol.Bonds)
                monomer.AddBond((BondTypeEnum)item.BondBits(), item.A, item.B);
			Molecule toReturn = Molecule.RepeatMonomer(monomer);
			if (!string.IsNullOrEmpty(mol.NameKey))
				toReturn.displayName = patch_Translations.TranslateByType(mol.NameKey);
			else toReturn.displayName = new(false, null);
            return toReturn;
        }
    }

    [DataContract]
    public class AtomM {
        [DataMember] public string AtomType { get; set; }
        [DataMember] public HexIndexM Position { get; set; }

        public AtomM() { }
        public AtomM(Atom atom, HexIndexM hex) {
			AtomType = ((patch_AtomType)(object)atom.atomType).Id;
			Position = hex;
		}
        public static implicit operator Atom (AtomM model) {
			if(model.AtomType == null)
				throw new NullReferenceException("Missing atom type!");

			return new Atom(
				AtomTypes.atoms.FirstOrDefault(k => model.AtomType.Equals(((patch_AtomType)(object)k).Id))
				?? throw new Exception($"Atom type \"{model.AtomType}\" does not exist!")
			);
		}
	}

    [DataContract]
    public class BondM {
		[DataMember] public HexIndexM A { get; set; }
        [DataMember] public HexIndexM B { get; set; }
        [DataMember] public HashSet<string> BondTypes { get; set; } = [];

        public BondM() { }
        public static implicit operator BondM(Bond bond) {
			BondM toReturn = new() {
				A = bond.hexPos1,
				B = bond.hexPos2,
			};
			if((bond.type & BondTypeEnum.Standard) == BondTypeEnum.Standard)
                toReturn.BondTypes.Add("standard");
			if((bond.type & BondTypeEnum.Prisma0) == BondTypeEnum.Prisma0)
                toReturn.BondTypes.Add("triplex_0");
			if((bond.type & BondTypeEnum.Prisma1) == BondTypeEnum.Prisma1)
                toReturn.BondTypes.Add("triplex_1");
			if((bond.type & BondTypeEnum.Prisma2) == BondTypeEnum.Prisma2)
                toReturn.BondTypes.Add("triplex_2");
			return toReturn;
		}
		public byte BondBits() {
			byte bits = 0;
			if(BondTypes.Contains("standard"))
				bits |= (byte)BondTypeEnum.Standard;
			if(BondTypes.Contains("triplex_0"))
				bits |= (byte)BondTypeEnum.Prisma0;
			if(BondTypes.Contains("triplex_1"))
				bits |= (byte)BondTypeEnum.Prisma1;
			if(BondTypes.Contains("triplex_2"))
				bits |= (byte)BondTypeEnum.Prisma2;
			return bits;
		}
	}

    [DataContract]
    public class ProductionInfoM {
        [DataMember] public List<ChamberM> Chambers { get; set; } = [];
        [DataMember] public List<ConduitM> Conduits { get; set; } = [];
        [DataMember] public List<VialM> Vials { get; set; } = [];
		[DataMember] public bool Isolation { get; set; } = false;
		[DataMember] public bool ShrinkLeft { get; set; } = false;
		[DataMember] public bool ShrinkRight { get; set; } = false;

        public ProductionInfoM() { }
        public static implicit operator ProductionInfoM (ProductionInfo info) {
            ProductionInfoM toReturn = new() {
				ShrinkLeft = info.tightLeftBound,
				ShrinkRight = info.tightRightBound,
				Isolation = info.requireIsolation
			};
            foreach (PlacedChamber chamber in info.chambers)
                toReturn.Chambers.Add(chamber);
            foreach (PlacedConduit conduit in info.conduits)
                toReturn.Conduits.Add(conduit);
            foreach (PlacedVial vial in info.vials)
                toReturn.Vials.Add(vial);
			return toReturn;
        }
        public static implicit operator ProductionInfo (ProductionInfoM info) {
            return new(){
                chambers = [.. info.Chambers],
                conduits = [.. info.Conduits],
                vials = [.. info.Vials],
                tightLeftBound = info.ShrinkLeft,
                tightRightBound = info.ShrinkRight,
                requireIsolation = info.Isolation
			};
		}
	}

    [DataContract]
    public class ChamberM {
        [DataMember] public string ChamberType { get; set; }
        [DataMember] public HexIndexM Position { get; set; }

        public ChamberM() { }
        public static implicit operator ChamberM (PlacedChamber chamber) {
			return new() {
				ChamberType = chamber.chamber.name,
				Position = chamber.hexPos
			};
        }
        public static implicit operator PlacedChamber (ChamberM chamber) {
			return new(chamber.Position.Q(), chamber.Position.R(), Puzzles.prodChambers.First(k => k.name.Equals(chamber.ChamberType)));
		}
	}

    [DataContract]
    public class ConduitM {
        [DataMember] public HexIndexM PosA { get; set; }
        [DataMember] public HexIndexM PosB { get; set; }
        [DataMember] public List<HexIndexM> Shape { get; set; } = [];

        public ConduitM() { }
        public static implicit operator ConduitM (PlacedConduit c) {
			ConduitM toReturn = new() {
                PosA = c.conduitTransforms[0].translation,
                PosB = c.conduitTransforms[1].translation
            };
			foreach(HexIndex hex in c.conduitHexes)
                toReturn.Shape.Add(hex);
			return toReturn;
		}
        public static implicit operator PlacedConduit (ConduitM c) {
			return new PlacedConduit(c.PosA.Q(), c.PosA.R(), c.PosB.Q(), c.PosB.R(), [..c.Shape]);
		}
	}

    [DataContract]
    public class VialM {
        [DataMember] public HexIndexM Position { get; set; }
        [DataMember] public bool Top { get; set; }
        [DataMember] public List<Tuple<string, string>> Sprites { get; set; } = [];

        public VialM() { }
        public static implicit operator VialM(PlacedVial v) {
			VialM toReturn = new() {
				Position = v.hexPos,
				Top = v.isTopConnected
			};
			foreach(Tuple<Texture, Texture> sprites in v.textures)
                toReturn.Sprites.Add(new(CleanName(sprites.Item1), CleanName(sprites.Item2)));
			return toReturn;
		}
        public static implicit operator PlacedVial (VialM v) {
			return new PlacedVial(v.Position.Q(), v.Position.R(), v.Top,
                [.. v.Sprites.Select(xs => Tuple.Create(AssetLoaderHelper.LoadTexture(xs.Item1), AssetLoaderHelper.LoadTexture(xs.Item2)))]);
		}
		private static string CleanName(Texture texture){
			string name = texture.sourceFile.GetValue();
			if(name.StartsWith("Content/") || name.StartsWith("Content\\"))
				name = name["Content/".Length..];
			return name;
		}
	}
}
