using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System;
using System.Collections.Generic;
using System.Linq;

[MonoModPatch("PartTypes")]
class patch_PartTypes {


    [MonoModILInject("Init")]
    static void PatchPartTypesInitBonders(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));

        MethodDefinition referenceCode = method.DeclaringType.Methods.First(f => f.Name.Equals("PatchPartTypesInitBondersCodeReference"));
        ILCursor referenceCulsor = new(new ILContext(referenceCode));
        FieldReference fireAtom = MonoModRule.Modder.FindType("AtomTypes").Resolve().Fields.First(f => f.Name.Equals("fire"));

        MethodReference simpleNewCtor = null;
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchNewobj(out simpleNewCtor));
        MethodReference createList = null;
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchNewobj(out createList));
        MethodReference converToBondType = null;
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchCall(out converToBondType));
        MethodReference addToList = null;
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchCallvirt(out addToList));
        MethodReference createListAtomType = null;
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchNewobj(out createListAtomType));
        MethodReference addToListAtomType = null;
        referenceCulsor.GotoNext(MoveType.After, instr => instr.MatchCallvirt(out addToListAtomType));
        referenceCulsor.GotoNext(MoveType.Before, instr => instr.MatchCastclass(out var _));
        MethodReference complexNewCtor = referenceCulsor.Prev.Operand as MethodReference;

        int referenceIndex = 0;
        (bool, string, FieldReference)[] bondIds = {
            (false, "om:standard", null),
            (true, "om:standard, om:prisma0, om:prisma1, om:prisma2", null),
            (false, "om:standard", null),
            (false, "om:standard", null),
            (false, "om:standard", null),
            (false, "om:prisma0", fireAtom),
            (false, "om:prisma1", fireAtom),
            (false, "om:prisma2", fireAtom),
        };

        while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchNewobj(out var oldCtor) && oldCtor.DeclaringType.Name == "BonderInfo")) {
            cursor.Next.Operand = bondIds[referenceIndex].Item3 == null ? simpleNewCtor : complexNewCtor;
            cursor.Index -= 3;
            cursor.RemoveRange(3);
            cursor.GotoPrev(MoveType.After, instr => instr.MatchDup());
            cursor.Index++;
            cursor.EmitNewobj(createList);
            foreach (var bondId in bondIds[referenceIndex].Item2.Split(", ")) {
                cursor.EmitDup();
                cursor.EmitLdstr(bondId);
                cursor.EmitCall(converToBondType);
                cursor.EmitCallvirt(addToList);
            }
            cursor.EmitLdcI4(bondIds[referenceIndex].Item1 ? 1 : 0);
            if (bondIds[referenceIndex].Item3 != null) {
                cursor.EmitNewobj(createListAtomType);
                cursor.EmitDup();
                cursor.EmitLdsfld(bondIds[referenceIndex].Item3);
                cursor.EmitCallvirt(addToListAtomType);
            }
            referenceIndex++;
            cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj(out var _));
        }
        if (referenceIndex != bondIds.Length) throw new Exception("Failed to find all bonder assignments.");

        referenceCode.DeclaringType.Methods.Remove(referenceCode);
    }
    public static void PatchPartTypesInitBondersCodeReference() {
        BonderInfo a = (BonderInfo)(object)new patch_BonderInfo(default, false, default, default);
        BonderInfo b = (BonderInfo)(object)new patch_BonderInfo(new List<BondType>() { "standard" }, false, new List<AtomType> { AtomTypes.fire }, new HexIndex(0, 0), new HexIndex(1, 0));
    }
}
