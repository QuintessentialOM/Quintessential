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

    public static readonly Codec<ModMeta> MOD = new ModMetaCodec();
    public static readonly Codec<Version> VERSION = new VersionCodec();
    public static readonly Codec<VersionRange> VERSIONRANGE = new VersionRangeCodec();
    public static readonly Codec<GlyphRecipe> RECIPE = Codec<GlyphRecipe>.Create(
        ID.Seal("GlyphId", (GlyphRecipe r) => r.RecipeGlyphId),
        ID.Seal("RecipeId", (GlyphRecipe r) => r.RecipeId),
        (gId, rId) => GlyphRecipe.Recipes[gId][rId]
    );
    public static readonly Codec<AtomTag> ATOMTAG = CodecFactory.CreateTag<AtomTag>((id, isTable) => new(id, isTable));
}

/*// TODO: Additional Codecs of type.
 * Campaign,
 * CampaignChapter,
 * CampaignItem,
 * 
 * PuzzleOption
 */

/*
public class CodecsBenchmark {

    public CodecsBenchmark() {
        holderCodec = Codec<IntHolder>.Create(
            Codecs.INT.Seal("myInt", (IntHolder I) => I.A),
            static int0 =>
                new IntHolder(int0)
        );
        primitiveTupleCodec = new ElementalTestCodec();
        tupleCodec = Codec<Tuple<float, int>>.Create(
            Codecs.FLOAT.Seal("Float", (Tuple<float, int> I) => I.Item1),
            Codecs.INT.Seal("Int", (Tuple<float, int> I) => I.Item2),
            static (int1, int2) =>
                new Tuple<float, int>(int1, int2)
        );
        var listCodec = ListCodec<Tuple<float, int>>.Create(tupleCodec);
        complexCodec = Codec<ListTest>.Create(
            Codecs.BYTE.Seal("Id", (ListTest L) => L.Id),
            listCodec.Seal("List", (ListTest L) => L.Data),
            static (id, data) =>
                new ListTest(id, data)
        );
    }
    internal class ElementalTestCodec : PrimitiveCodec<Tuple<float, int>> {
        public override Tuple<float, int> Decode<TData>(CodecMap<TData> map, TData encoding) {
            var obj = map.ReadObject(encoding);
            float f = map.ReadFloat(obj["Float"]);
            int i = map.ReadInt(obj["Int"]);
            return new(f, i);
        }
        public override TData Encode<TData>(CodecMap<TData> map, Tuple<float, int> item) {
            Dictionary<string, TData> obj = new() {
                ["Float"] = map.WriteFloat(item.Item1),
                ["Int"] = map.WriteInt(item.Item2)
            };
            return map.WriteObject(obj);
        }
    }

    internal record IntHolder(int A);
    internal record ListTest(byte Id, List<Tuple<float, int>> Data);
    Codec<IntHolder> holderCodec;
    Codec<Tuple<float, int>> tupleCodec;
    Codec<Tuple<float, int>> primitiveTupleCodec;
    Codec<ListTest> complexCodec;
    public void TestCompareDynamic(int N, bool logObjects) {
        static Tuple<float, int> T(float f, int i) => new(f, i);
        Tuple<float, int> tuple = T(2.71828f, 6);
        Console.WriteLine($"\n----- Benchmark for {N} iterations:");
        Test(primitiveTupleCodec, tuple, N, logObjects);
        Test(tupleCodec, tuple, N, logObjects);
    }
    public void Test(int N, bool logObjects) {
        (double, double) totalT = (0, 0);
        void R((double, double) b) => totalT = (totalT.Item1 + b.Item1, totalT.Item2 + b.Item2);
        static Tuple<float, int> T(float f, int i) => new(f, i);

        IntHolder a = new(5); // Just a class with an int A
        Tuple<float, int> tuple = T(3.1415f, 6);
        ListTest complex = new(235, [T(3.44f, 4), T(5.775f, 634), T(52.234f, -123), T(644, -3)]);

        Console.WriteLine($"\n----- Benchmark for {N} iterations:");

        R(Test(Codecs.STRING, "testString", N, logObjects));
        R(Test(holderCodec, a, N, logObjects));
        R(Test(tupleCodec, tuple, N, logObjects));
        R(Test(complexCodec, complex, N, logObjects));
        R(Test(Codecs.ATOMTAG, AtomTag.AtomTags["om:duplicatable"], N, logObjects));
        R(Test(Codecs.ATOMTAG, AtomTag.AtomTags["om:$successor_proj"], N, logObjects));
        R(Test(Codecs.PERMISSIONS, Puzzles.airshipFuel.permissionFlags, N, logObjects));
        R(Test(Codecs.PERMISSIONS, Puzzles.universalSolvent.permissionFlags, N, logObjects));

        (string, string, string) writtenT = (((float)(totalT.Item1 / 1000d)).ToString(), ((float)(totalT.Item2 / 1000d)).ToString(), ((float)((totalT.Item1 + totalT.Item2) / 1000d)).ToString());
        writtenT = (writtenT.Item1 + new string(' ', 9 - writtenT.Item1.Length), writtenT.Item2 + new string(' ', 9 - writtenT.Item2.Length), writtenT.Item3 + new string(' ', 9 - writtenT.Item3.Length));
        Console.WriteLine($"\n----- Total Times: {writtenT.Item1} μs, {writtenT.Item2} μs");
        Console.WriteLine(  $"----- Total Total: {writtenT.Item3} μs");

    }

    private static (double, double) Test<T>(Codec<T> codec, T item, int N, bool logObjects) {
        var data = codec.Encode(JsonCodecMap.Instance, item);
        T dec = codec.Decode(JsonCodecMap.Instance, data);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < N; i++) {
            data = codec.Encode(JsonCodecMap.Instance, dec);
        }
        double encodeT = sw.Elapsed.TotalNanoseconds / N;
        sw.Restart();
        for (int i = 0; i < N; i++) {
            dec = codec.Decode(JsonCodecMap.Instance, data);
        }
        double decodeT = sw.Elapsed.TotalNanoseconds / N;
        if (logObjects) {
            Console.WriteLine($"----- Encoded: {encodeT}{new string(' ',8 - encodeT.ToString().Length)} ns\n{data}");
            Console.WriteLine($"----- Decoded: {decodeT}{new string(' ',8 - decodeT.ToString().Length)} ns\n{dec}");
        }else {
            Console.WriteLine($"----- Encoded: {encodeT}{new string(' ',8 - encodeT.ToString().Length)} ns for {typeof(T)}");
            Console.WriteLine($"----- Decoded: {decodeT}{new string(' ',8 - decodeT.ToString().Length)} ns for {typeof(T)}");
        }
        return (encodeT, decodeT);
    }
}
//*/
