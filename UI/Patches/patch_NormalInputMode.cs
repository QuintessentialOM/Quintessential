using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System.Linq;

internal class patch_NormalInputMode {

    [MonoModILInject("Process")]
    static void PatchMoleculeDisplayName(MethodDefinition method, CustomAttribute attribute) {

        ILCursor cursor = new(new ILContext(method)); // Create cursor
        MethodDefinition getDisplayName = MonoModRule.Modder.FindType("Molecule").Resolve().Methods.First(f => f.Name.Equals("GetDisplayName"));

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdflda("Molecule", "displayName"))){
            cursor.Prev.OpCode = OpCodes.Callvirt;
            cursor.Prev.Operand = getDisplayName;

            method.Body.Variables.Add(new VariableDefinition(getDisplayName.ReturnType));
            var maybeVar = method.Body.Variables[^1];
            cursor.EmitStloc(maybeVar);
            cursor.EmitLdloca(maybeVar);
        }
    }
}
