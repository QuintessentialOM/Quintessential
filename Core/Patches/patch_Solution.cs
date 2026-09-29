using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using System;
using System.Linq;

public class patch_Solution
{
    public static bool GetConduits(Puzzle puzzle, out PlacedConduit[] conduits)
    {
        if (puzzle.productionInfo.GetOrDefault(out ProductionInfo cabinetInfo))  {
            conduits = cabinetInfo.conduits;
            return true;
        }
        return ((patch_Puzzle)(object)puzzle).EngineConduits.GetOrDefault(out conduits);
    } 

    public static void ApplyChanges(Puzzle puzzle, Solution solution)
    {
        if (((patch_Puzzle)(object)puzzle).Payloads.GetOrDefault(out Payloads payloads)) {
            foreach (Payloads.Payload p in payloads.SolutionInitialization)
            {
                foreach (var handler in QApi.SolutionPayloadHandler)
                {
                    if (p.Address.Equals(handler.Item1))
                    {
                        handler.Item2(solution, p.Data);
                    }
                }
            }
        }
    }

    [MonoModILInject("FromPuzzle")]
    public static void PatchSolutionInitializer(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching solution initializer");
        if (!method.HasBody) {
            Console.WriteLine("Failed to modify solution initializer (no body)!");
            throw new Exception();
        }

        ILCursor cursor = new(new ILContext(method));

        if (!cursor.TryGotoNext(MoveType.Before,
           instr => instr.MatchLdarg(0),
           instr => instr.OpCode == OpCodes.Ldfld,
           instr => instr.MatchLdloca(1),
           instr => instr.MatchCall(out MethodReference m) && m.ReturnType.Name == "Boolean",
           instr => instr.OpCode == OpCodes.Brfalse)) {
            Console.WriteLine("Failed to modify solution initializer (no production info branch)");
            throw new Exception();
        }
        int begin = cursor.Index;
        Instruction ifEnd = (Instruction)cursor.Instrs[cursor.Index + 4].Operand;
        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdloc(1),
            instr => instr.OpCode == OpCodes.Ldfld,
            instr => instr.MatchStloc(3)
        )) {
            Console.WriteLine("Failed to modify solution initializer (no conduit assignment)");
            throw new Exception();
        }
        int end = cursor.Index;
        cursor.Index = begin;
        cursor.RemoveRange(end - begin);

        TypeDefinition holder = MonoModRule.Modder.FindType("Solution").Resolve();
        MethodDefinition to = holder.Methods.First(m => m.Name.Equals("GetConduits"));
        // Puzzle
        cursor.Emit(OpCodes.Ldarg_0);
        // conduit list address
        cursor.Emit(OpCodes.Ldloca, 3);
        // Solution.GetConduits
        cursor.Emit(OpCodes.Call, to);
        // if body skipping
        cursor.Emit(OpCodes.Brfalse, ifEnd);
        Instruction branch = cursor.Prev;
        // assign the first conduit's ID to 100
        cursor.Emit(OpCodes.Ldc_I4, 100);
        cursor.Emit(OpCodes.Stloc, 2);

        // jump to end of if statement
        if (!cursor.TryGotoNext(instr => instr == ifEnd)) {
            Console.WriteLine("Failed to modify solution initializer (no end of if body)");
            throw new Exception();
        }
        to = holder.Methods.First(m => m.Name.Equals("ApplyChanges"));

        // Why does cursor.MoveAfterLabels not work like I expect?
        cursor.Emit(OpCodes.Ldarg_0);
        branch.Operand = cursor.Prev;
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.Emit(OpCodes.Call, to);

    }

    [MonoModILInject(".ctor")]
    public static void PatchSolutionCtor(MethodDefinition method, CustomAttribute attrib) {
        method.IsPublic = true;
    }
}
