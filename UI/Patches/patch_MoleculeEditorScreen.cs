
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable IDE1006


using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using System;
using System.Collections.Generic;
using System.Linq;

class patch_MoleculeEditorScreen
{

    internal Puzzle editing;

    private static readonly Texture prevAtoms = AssetLoaderHelper.LoadTexture("textures/editor_go_left");
    private static readonly Texture prevAtomsFaded = AssetLoaderHelper.LoadTexture("textures/editor_go_left_faded");
    private static readonly Texture prevAtomsHover = AssetLoaderHelper.LoadTexture("textures/editor_go_left_hover");
    private static readonly Texture nextAtoms = AssetLoaderHelper.LoadTexture("textures/editor_go_right");
    private static readonly Texture nextAtomsFaded = AssetLoaderHelper.LoadTexture("textures/editor_go_right_faded");
    private static readonly Texture nextAtomsHover = AssetLoaderHelper.LoadTexture("textures/editor_go_right_hover");
    // for measuring stuff in debugging
    //private static readonly Texture dot = class_235.method_618(Color.Red);

    public static int currentPage = 0;



    public extern void orig_RenderFrame(float detalTime);
    public void RenderFrame(float detalTime)
    {
        orig_RenderFrame(detalTime);

        // This was being instantiated before all other mods could call LoadContent, causing the list to be unpopulated.
        // This only occurred when Reductive Metallurgy Campaign is loaded for me, though I've had other mods on.
        // Thankfully it's only an integer division, but I should find a more sensible patch.
        int LastPage = (QApi.ModAtomTypes.Count + 14) / 15;

        Vector2 uiSize = new(1516f, 922f);
        Vector2 corner = (InputManager.screenSize / 2 - uiSize / 2 + new Vector2(-2f, -11f)).Rounded();
        Vector2 lPos = corner + new Vector2(90f, 800f);
        Vector2 rPos = lPos;
        rPos.X += 350 - nextAtoms.size.X;
        bool inLeftBound = Bounds2.WithSize(lPos, prevAtoms.size.ToVector2()).Contains(InputManager.MousePos());
        bool inRightBound = Bounds2.WithSize(rPos, nextAtoms.size.ToVector2()).Contains(InputManager.MousePos());
        TextureRenderer.Render(currentPage > 0 ? inLeftBound ? prevAtomsHover : prevAtoms : prevAtomsFaded, lPos);
        TextureRenderer.Render(currentPage < LastPage ? inRightBound ? nextAtomsHover : nextAtoms : nextAtomsFaded, rPos);
        UI.DrawText($"{currentPage + 1}/{LastPage + 1}", corner + new Vector2(262f, 800f), UI.Text, UI.TextColor, TextAlignment.Left);
        if (InputManager.IsClickPressed(MouseButtonType.LeftClick) && (inLeftBound || inRightBound))
        {
            Assets.sounds.click_button.method_28(1f);

            if (inLeftBound && currentPage > 0)
            {
                currentPage--;
            }

            if (inRightBound && currentPage < LastPage)
            {
                currentPage++;
            }
        }
    }

    [MonoMod.MonoModIgnore]
    private extern void AtomTypeSelector(Vector2 pos, AtomType type, bool b);
    internal void DrawAtoms(Vector2 corner, Vector2 spacing)
    {
        bool useVanillaList = currentPage == 0;
        List<AtomType> atoms = useVanillaList ? new()
        {
            AtomTypes.salt,
            AtomTypes.air,
            AtomTypes.fire,
            AtomTypes.quicksilver,
            AtomTypes.water,
            AtomTypes.earth,
            AtomTypes.lead,
            AtomTypes.tin,
            AtomTypes.iron,
            AtomTypes.copper,
            AtomTypes.silver,
            AtomTypes.gold,
            AtomTypes.vitae,
            AtomTypes.mors,
            AtomTypes.quintessence
        } : QApi.ModAtomTypes;

        Vector2 pos = corner;
        bool showExtra = InputManager.IsModifierKeyHeld(0);
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                int index = (useVanillaList ? 0 : 15 * (currentPage - 1)) + 3 * y + x;
                if (index >= atoms.Count)
                {
                    goto outer;
                }
                this.AtomTypeSelector(pos, atoms[index], true);
                if (showExtra)
                {
                    bool hovering = Bounds2.WithSize(pos - new Vector2(30, 30), new Vector2(61, 61)).Contains(InputManager.MousePos());
                    if (hovering)
                    {
                        TextureRenderer.RenderText(atoms[index].defaultName, pos + new Vector2(0, -40), Assets.fonts.crimson_9_75, UI.TextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
                    }
                }
                pos.X += spacing.X;
            }
            pos.X = corner.X;
            pos.Y += spacing.Y;
        }
    outer:
        // ...
        return;
    }


    [MonoModILInject("RenderFrame")]
    static void PatchMoleculeEditorScreenAtomTray(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching molecule editor screen atom tray");
        if (!method.HasBody) {
            Console.WriteLine("Failed to modify molecule editor rendering (no body)!");
            throw new Exception();
        }
        ILCursor cursor = new(new ILContext(method)); // Create cursor
        if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdloc(9),
            instr => instr.MatchLdsfld("AtomTypes", "salt"),
            instr => instr.MatchLdcI4(1),
            instr => instr.MatchCallvirt("MoleculeEditorScreen", "AtomTypeSelector") // Move to the function call
        )) {
            Console.WriteLine("Failed to modify molecule editor rendering (no start match)!");
            throw new Exception();
        }
        int start = cursor.Index;
        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdloc(9),
            instr => instr.MatchLdsfld("AtomTypes", "quintessence"),
            instr => instr.MatchLdcI4(1),
            instr => instr.MatchCallvirt("MoleculeEditorScreen", "AtomTypeSelector") // Move to the function call
        )) {
            Console.WriteLine("Failed to modify molecule editor rendering (no near end match)!");
            throw new Exception();
        }
        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchStindR4()
        )) {
            Console.WriteLine("Failed to modify molecule editor rendering (no end match)!");
            throw new Exception();
        }
        int end = cursor.Index;

        TypeDefinition holder = MonoModRule.Modder.FindType("MoleculeEditorScreen").Resolve();
        MethodDefinition to = holder.Methods.First(m => m.Name.Equals("DrawAtoms"));

        cursor.Goto(start);
        cursor.RemoveRange(end - start); // Go bye with you
        cursor.Emit(OpCodes.Ldarg_0); // this
        cursor.Emit(OpCodes.Ldloc, 9);
        cursor.Emit(OpCodes.Ldloc, 8);
        cursor.Emit(OpCodes.Callvirt, to);
    }

    [MonoModILInject("MoleculeError")]
    public static void PatchMoleculeEditorScreenMoleculeError(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching molecule editor screen error detector");
        if (!method.HasBody) {
            Console.WriteLine("Failed to modify molecule editor error detector (no body)!");
            throw new Exception();
        }
        ILCursor cursor = new(new ILContext(method)); // Create cursor

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld(out FieldReference f) && f.Name == "molecule",
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.MatchStloc(1),
            instr => instr.OpCode == OpCodes.Br
        )) {
            Console.WriteLine("Failed to modify molecule editor error detector (no bond checker)!");
            throw new Exception();
        }
        Instruction start = cursor.Previous;
        cursor.Goto((Instruction)cursor.Previous.Operand);

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdloc(1),
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.OpCode == OpCodes.Brtrue,
            instr => instr.OpCode == OpCodes.Leave
        )) {
            Console.WriteLine("Failed to modify molecule editor error detector (no last leave)");
            throw new Exception();
        }
        Instruction end = cursor.Previous;
        // Immediately leaves the loop
        start.Operand = end;

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld(out FieldReference f) && f.Name == "molecule",
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.OpCode == OpCodes.Callvirt,
            instr => instr.MatchStloc(1),
            instr => instr.OpCode == OpCodes.Br
        )) {
            Console.WriteLine("Failed to modify molecule editor error detector (second error)");
            throw new Exception();
        }
        Instruction start2 = cursor.Previous;
        cursor.Goto((Instruction)cursor.Previous.Operand);

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Brtrue,
            instr => instr.OpCode == OpCodes.Leave_S
        )) {
            Console.WriteLine("Failed to modify molecule editor error detector (no second last leave)");
            throw new Exception();
        }
        Instruction end2 = cursor.Previous;
        // Immediately leaves the loop
        start2.Operand = end2;
    }
}