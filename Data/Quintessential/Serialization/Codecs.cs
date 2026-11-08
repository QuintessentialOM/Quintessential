using Quintessential.Internal;
using System;
using System.Collections.Generic;

namespace Quintessential.Serialization;

public static class Codecs {
    public static readonly Codec<string> STRING = new StringCodec();
    public static readonly Codec<int> INT = new IntCodec();
    public static readonly Codec<long> LONG = new LongCodec();
    public static readonly Codec<float> FLOAT = new FloatCodec();
    public static readonly Codec<double> DOUBLE = new DoubleCodec();
    public static readonly Codec<bool> BOOL = new BoolCodec();
    public static readonly Codec<byte> BYTE = new ByteCodec();
    public static readonly Codec<Identifier> ID = new IdentifierCodec();

    public static readonly Codec<List<int>> LIST_INT = ListCodec<int>.Create(INT);
    public static readonly Codec<List<float>> LIST_FLOAT = ListCodec<float>.Create(FLOAT);
    public static readonly Codec<List<string>> LIST_STRING = ListCodec<string>.Create(STRING);
    public static readonly Codec<List<Identifier>> LIST_ID = ListCodec<Identifier>.Create(ID);

    public static readonly Codec<Vector2> VECTOR2 = Codec<Vector2>.Create(
        FLOAT.Seal("X", (Vector2 vec) =>  vec.X),
        FLOAT.Seal("Y", (Vector2 vec) => vec.Y),
        (x, y) => new Vector2(x, y)
    );
    public static readonly Codec<Vector3> VECTOR3 = Codec<Vector3>.Create(
        FLOAT.Seal("X", (Vector3 vec) => vec.X),
        FLOAT.Seal("Y", (Vector3 vec) => vec.Y),
        FLOAT.Seal("Z", (Vector3 vec) => vec.Z),
        (x, y, z) => new Vector3(x, y, z)
    );
    public static readonly Codec<Index2> INDEX2 = Codec<Index2>.Create(
        INT.Seal("X", (Index2 index) => index.X),
        INT.Seal("Y", (Index2 index) => index.Y),
        (x, y) => new Index2(x, y)
    );
    public static readonly Codec<HexIndex> HEXINDEX = Codec<HexIndex>.Create(
        INT.Seal("Q", (HexIndex index) => index.Q),
        INT.Seal("R", (HexIndex index) => index.R),
        (q, r) => new HexIndex(q, r)
    );
    public static readonly Codec<HexRotation> HEXROT = Codec<HexRotation>.Create(
        INT.Seal("Turns", (HexRotation rot) => rot.GetNumberOfTurns()),
        (turns) => new HexRotation(turns)
    );
    public static readonly Codec<Color> COLOR = Codec<Color>.Create(
        FLOAT.Seal("R", (Color color) => color.R),
        FLOAT.Seal("G", (Color color) => color.G),
        FLOAT.Seal("B", (Color color) => color.B),
        FLOAT.Seal("A", (Color color) => color.A),
        (r, g, b, a) => new Color(r, g, b, a)
    );
    public static readonly Codec<Matrix4> MATRIX = Codec<Matrix4>.Create(
        LIST_FLOAT.Seal("Values", (Matrix4 m) => [
            m.xx, m.xy, m.xz, m.xw, m.yx, m.yy, m.yz, m.yw, m.zx, m.zy, m.zz, m.zw, m.wx, m.wy, m.wz, m.ww
        ]),
        (values) => new Matrix4(
            values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7],
            values[8], values[9], values[10], values[11], values[12], values[13], values[14], values[15]
        )
    );
    public static readonly Codec<List<HexIndex>> LIST_HEXINDEX = ListCodec<HexIndex>.Create(HEXINDEX);

    public static readonly Codec<Maybe<LocString>> M_LOCSTRING = new MLocStringCodec();
    public static readonly Codec<LocString> LOCSTRING = new LocStringCodec();
    public static readonly Codec<AtomType> ATOMTYPE = new AtomTypeCodec();
    public static readonly Codec<PartType> PARTTYPE = new PartTypeCodec();
    public static readonly Codec<InstructionType> INSTRTYPE = ConverterCodec<InstructionType, Identifier>.Create(
        ID, (instr) => instr.Id, (id) => InstructionTypes.GetById(id).GetValue()
    );
    public static readonly Codec<PuzzlePermissions> PERMISSIONS = EnumCodec<PuzzlePermissions>.Create(true);

    public static Codec<Atom> ATOM => patch_Atom.ATOM;
    public static Codec<Bond> BOND => patch_Bond.BOND;
    public static Codec<patch_Molecule> MOLECULE => patch_Molecule.MOLECULE;
    public static Codec<Part> PART => patch_Part.PART;
    public static Codec<Solution> SOLUTION => patch_Solution.SOLUTION;
    public static Codec<Puzzle> PUZZLE => patch_Puzzle.PUZZLE;

    public static Codec<CampaignItem> CAMPAIGN_ITEM => CampaignSerializer.CAMPAIGN_ITEM;
    public static Codec<CampaignChapter> CHAPTER => CampaignSerializer.CHAPTER;
    public static Codec<Campaign> CAMPAIGN => CampaignSerializer.CAMPAIGN;
    public static Codec<JournalVolume> VOLUME => CampaignSerializer.VOLUME;
    public static Codec<Journal> JOURNAL => CampaignSerializer.JOURNAL;

    public static readonly Codec<ModMeta> MOD = new ModMetaCodec();
    public static readonly Codec<Version> VERSION = new VersionCodec();
    public static readonly Codec<VersionRange> VERSIONRANGE = new VersionRangeCodec();
    public static readonly Codec<GlyphRecipe> RECIPE = Codec<GlyphRecipe>.Create(
        ID.Seal("GlyphId", (GlyphRecipe r) => r.RecipeGlyphId),
        ID.Seal("RecipeId", (GlyphRecipe r) => r.RecipeId),
        (gId, rId) => GlyphRecipe.Recipes[gId][rId]
    );
    public static readonly Codec<AtomTag> ATOMTAG = CodecFactory.CreateTag<AtomTag>((id, isTable) => new(id, isTable));
    public static readonly Codec<PartTag> PARTTAG = CodecFactory.CreateTag<PartTag>((id, isTable) => new(id, isTable));
}
