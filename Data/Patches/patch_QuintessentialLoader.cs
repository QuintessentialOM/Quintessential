using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using System;
using System.Linq;

[MonoModPatch("Quintessential.QuintessentialLoader")]
public class patch_QuintessentialLoader {

    static void DataContentInit() {
        foreach (var mod in QuintessentialLoader.CodeMods) {
            if (mod is IDataMod dataMod) {
                dataMod.LoadTags();
            }
        }
    }

    [MonoModILInject("ModContentInit")]
    static void InjectDataContentInit(MethodDefinition method, CustomAttribute attribute) {
        MonoModRule.Modder.Log("Patching DataContent init.");
        if (!method.HasBody) {
            throw new Exception("Unable to inject DataContent init. (no body)");
        }

        ILCursor cursor = new(new ILContext(method));

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchCallvirt("Quintessential.QuintessentialMod", "LoadCompatContent")
        )) {
            throw new Exception("Unable to inject DataContent init. (no target)");
        }
        if (!cursor.TryGotoPrev(MoveType.Before,
            instr => instr.MatchLdsfld("Quintessential.QuintessentialLoader", "CodeMods")
        )) {
            throw new Exception("Unable to inject DataContent init. (no target - 2)");
        }

        TypeDefinition holder = MonoModRule.Modder.FindType("Quintessential.QuintessentialLoader").Resolve();
        MethodDefinition to = holder.Methods.First(m => m.Name.Equals("DataContentInit"));

        cursor.EmitCall(to);
        cursor.EmitNop();
    }
}
