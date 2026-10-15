using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using System;
using System.Linq;

#pragma warning disable IDE1006 // Naming Styles

[MonoModPatch("TextureEffect")]
public class patch_TextureEffect {
    public Color Color;

    public patch_TextureEffect WithColor(Color color)
    {
        Color = color;
        return this;
    }

    [MonoModILInject(".ctor")]
    public static void PatchGlyphEffectConstructor(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching texture effect (1/2)");

        if (!method.HasBody) {
            Console.WriteLine("Unable to patch texture effect constructor (no body)");
            throw new Exception();
        }

        ILCursor gremlin = new(new ILContext(method));

        if (!gremlin.TryGotoNext(MoveType.Before,
            instr => instr.MatchRet()
        )) {
            Console.WriteLine("Unable to patch texture effect constructor (no return)");
            throw new Exception();
        }

        TypeDefinition holder = MonoModRule.Modder.FindType("TextureEffect").Resolve();
        FieldDefinition colorProp = holder.Fields.First((f) => f.Name == "Color");

        holder = MonoModRule.Modder.FindType("Color").Resolve();
        FieldDefinition colorWhite = holder.Fields.First((f) => f.IsStatic && f.Name == "White");


        gremlin.Emit(OpCodes.Ldarg_0);
        gremlin.Emit(OpCodes.Ldsfld, colorWhite);
        gremlin.Emit(OpCodes.Stfld, colorProp);

    }

    [MonoModILInject("RenderEffect")]
    public static void PatchTextureEffectRenderer(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching texture effect (2/2)");

        if (!method.HasBody) {
            Console.WriteLine("Unable to patch texture effect renderer (no body)");
            throw new Exception();
        }

        ILCursor gremlin = new(new ILContext(method));

        TypeDefinition holder = MonoModRule.Modder.FindType("Color").Resolve();
        FieldDefinition colorWhite = holder.Fields.First((f) => f.IsStatic && f.Name == "White");

        if (!gremlin.TryGotoNext(MoveType.Before,
            instr => {
                FieldReference testOperand = instr.Operand as FieldReference;
                return instr.OpCode == OpCodes.Ldsfld && testOperand.FieldType == colorWhite.FieldType && testOperand.Name == colorWhite.Name;
            }
        )) {
            Console.WriteLine("Unable to patch texture effect renderer (no draw call)");
            throw new Exception();
        }

        holder = MonoModRule.Modder.FindType("TextureEffect").Resolve();
        FieldDefinition colorProp = holder.Fields.First((f) => f.Name == "Color");

        gremlin.Remove();
        gremlin.Emit(OpCodes.Ldarg_0);
        gremlin.Emit(OpCodes.Ldfld, colorProp);
    }
}