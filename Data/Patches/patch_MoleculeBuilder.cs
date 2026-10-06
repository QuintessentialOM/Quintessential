using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System.Collections.Generic;
using System.Linq;

[MonoModPatch("MoleculeBuilder")]
class patch_MoleculeBuilder {
    [MonoModIgnore] patch_Molecule molecule;
    [MonoModIgnore] System.Collections.Generic.Stack<HexIndex> hexPath;

    public patch_MoleculeBuilder Bond(BondType bondType, int offsetQ, int offsetR) {
        HexIndex hexIndex = new(offsetQ, offsetR);
        molecule.AddBond(bondType, hexPath.Peek(), hexPath.Peek() + hexIndex);
        hexPath.Push(hexPath.Peek() + hexIndex);
        return this;
    }
    public patch_MoleculeBuilder Bond(List<BondType> bondTypes, int offsetQ, int offsetR) {
        HexIndex hexIndex = new(offsetQ, offsetR);
        foreach (var bondType in bondTypes) {
            molecule.AddBond(bondType, hexPath.Peek(), hexPath.Peek() + hexIndex);
        }
        hexPath.Push(hexPath.Peek() + hexIndex);
        return this;
    }

    [MonoModReplace]
    public patch_MoleculeBuilder Bond(int offsetQ, int offsetR) {
        HexIndex hexIndex = new(offsetQ, offsetR);
        molecule.AddBond("om:standard", hexPath.Peek(), hexPath.Peek() + hexIndex);
        hexPath.Push(hexPath.Peek() + hexIndex);
        return this;
    }
    [MonoModReplace]
    public patch_MoleculeBuilder PrismaBond(int offsetQ, int offsetR) {
        HexIndex hexIndex = new(offsetQ, offsetR);
        molecule.AddBond("om:prisma0", hexPath.Peek(), hexPath.Peek() + hexIndex);
        molecule.AddBond("om:prisma1", hexPath.Peek(), hexPath.Peek() + hexIndex);
        molecule.AddBond("om:prisma2", hexPath.Peek(), hexPath.Peek() + hexIndex);
        hexPath.Push(hexPath.Peek() + hexIndex);
        return this;
    }
    [MonoModILInject("DuplicateSixfoldAboutOrigin")]
    static void PatchSixfoldAddBond(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodReference newAdd = MonoModRule.Modder.FindType("Molecule").Resolve().Methods.First(f => f.Name.Equals("SetBond"));
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt(out var m) && m.FullName == "System.Boolean Molecule::AddBond(BondTypeEnum,HexIndex,HexIndex)")) {
            cursor.Prev.Operand = newAdd;
            cursor.Remove(); // Remove a Pop instruction since SetBond doesn't return a boolean.
            cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdfld("Bond", "type"));
            cursor.Remove();
        }
    }
}
