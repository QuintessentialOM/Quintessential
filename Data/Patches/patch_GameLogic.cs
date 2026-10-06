using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System;
using System.Linq;

internal class patch_GameLogic {

    [MonoModILInject("ContentInit")]
    static void ContentInitBondTypeInit(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching bond type init");

        if (!method.HasBody) {
            throw new Exception("Unable to patch bond types init. (no body)");
        }

        ILCursor cursor = new(new ILContext(method));

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchCall("BondTextures", "Init")
        )) {
            throw new Exception("Unable to patch bond types init. (no call)");
        }

        TypeDefinition holder = MonoModRule.Modder.FindType("BondTypes").Resolve();
        MethodDefinition call = holder.Methods.First((f) => f.Name == "InitBonds");

        cursor.Emit(OpCodes.Call, call);
    }
}
