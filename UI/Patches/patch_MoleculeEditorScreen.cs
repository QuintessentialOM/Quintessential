
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
    //private static readonly Texture dot = AssetLoaderHelper.ColorTexture(Color.Red);

    private static BondType summedTriplexPlaceHolder = new("quintessential_ui:triplex_placeholder", null, null, null);

    public static int currentPage = 0;
    public static int currentPageBonds = 0;
    private BondType selectedBondType;
    private HexIndex? selectedBondIndex;


    [MonoMod.MonoModIgnore]
    private bool isProduct;
    [MonoMod.MonoModIgnore]
    private Molecule molecule;

    public extern void orig_RenderFrame(float detalTime);
    public void RenderFrame(float detalTime)
    {
        orig_RenderFrame(detalTime);

        // This was being instantiated before all other mods could call LoadContent, causing the list to be unpopulated.
        // This only occurred when Reductive Metallurgy Campaign is loaded for me, though I've had other mods on.
        // Thankfully it's only an integer division, but I should find a more sensible patch.
        int LastPage = (QApi.ModAtomTypes.Count + 14) / 15;
        int LastPageBonds = (QApi.ModBondTypes.Count + 2) / 3 + 1;

        Vector2 uiSize = new(1516f, 922f);
        Vector2 corner = (InputManager.screenSize / 2 - uiSize / 2 + new Vector2(-2f, -11f)).Rounded();
        if (QApi.ModAtomTypes.Count > 0 || true) {
            Vector2 lPos = corner + new Vector2(90f, 800f);
            Vector2 rPos = lPos;
            rPos.X += 350 - nextAtoms.size.X;
            bool inLeftBound = Bounds2.WithSize(lPos, prevAtoms.size.ToVector2()).Contains(InputManager.MousePos());
            bool inRightBound = Bounds2.WithSize(rPos, nextAtoms.size.ToVector2()).Contains(InputManager.MousePos());
            TextureRenderer.Render(currentPage > 0 ? inLeftBound ? prevAtomsHover : prevAtoms : prevAtomsFaded, lPos);
            TextureRenderer.Render(currentPage < LastPage ? inRightBound ? nextAtomsHover : nextAtoms : nextAtomsFaded, rPos);
            UI.DrawText($"{currentPage + 1}/{LastPage + 1}", corner + new Vector2(262f, 800f), UI.Text, UI.TextColor, TextAlignment.Center);
            if (InputManager.IsClickPressed(MouseButtonType.LeftClick) && (inLeftBound || inRightBound)) {
                Assets.sounds.click_button.PlaySound(1f);

                if (inLeftBound && currentPage > 0) {
                    currentPage--;
                } else if (inRightBound && currentPage < LastPage) {
                    currentPage++;
                }
            }
        }
        {
            Vector2 lPos = corner + new Vector2(90f, 277f);
            Vector2 rPos = lPos;
            rPos.X += 350 - nextAtoms.size.X;
            bool inLeftBound = Bounds2.WithSize(lPos, prevAtoms.size.ToVector2()).Contains(InputManager.MousePos());
            bool inRightBound = Bounds2.WithSize(rPos, nextAtoms.size.ToVector2()).Contains(InputManager.MousePos());
            TextureRenderer.Render(currentPageBonds > 0 ? inLeftBound ? prevAtomsHover : prevAtoms : prevAtomsFaded, lPos);
            TextureRenderer.Render(currentPageBonds < LastPageBonds ? inRightBound ? nextAtomsHover : nextAtoms : nextAtomsFaded, rPos);
            UI.DrawText($"{currentPageBonds + 1}/{LastPageBonds + 1}", corner + new Vector2(262f, 277f), UI.Text, UI.TextColor, TextAlignment.Center);
            if (InputManager.IsClickPressed(MouseButtonType.LeftClick) && (inLeftBound || inRightBound)) {
                Assets.sounds.click_button.PlaySound(1f);

                if (inLeftBound && currentPageBonds > 0) {
                    currentPageBonds--;
                } else if (inRightBound && currentPageBonds < LastPageBonds) {
                    currentPageBonds++;
                }
            }
        }
    }

    [MonoMod.MonoModIgnore]
    private extern void AtomTypeSelector(Vector2 pos, AtomType type, bool b);
    internal void DrawAtoms(Vector2 corner, Vector2 spacing) {
        bool useVanillaList = currentPage == 0;
        List<AtomType> atoms = useVanillaList ? [
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
        ] : QApi.ModAtomTypes;

        Vector2 pos = corner;
        bool showExtra = InputManager.IsModifierKeyHeld(ModifierKeyType.Shift);
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

    [MonoModRemove]
    private extern void BondTypeSelector(Vector2 pos, BondTypeEnum bondType, Texture texture);
    internal void DrawBonds(Vector2 corner, Vector2 spacing) {
        selectedBondType ??= BondTypes.standard;
        bool showExtra = InputManager.IsModifierKeyHeld(ModifierKeyType.Shift);
        if (currentPageBonds == 0) {
            BondTypeSelector(corner, BondTypes.standard);
            if (showExtra) {
                bool hovering = Bounds2.WithSize(corner - new Vector2(30, 30), new Vector2(61, 61)).Contains(InputManager.MousePos());
                if (hovering) {
                    TextureRenderer.RenderText(BondTypes.standard.Id, corner + new Vector2(0, -40), Assets.fonts.crimson_9_75, UI.TextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
                }
            }
            BondTypeSelector(corner + new Vector2(spacing.X, 0), summedTriplexPlaceHolder);
            AtomTypeSelector(corner + new Vector2(spacing.X * 2, 0), AtomTypes.repeat, isProduct);
            if (showExtra) {
                bool hovering = Bounds2.WithSize(corner - new Vector2(30 - spacing.X * 2, 30), new Vector2(61, 61)).Contains(InputManager.MousePos());
                if (hovering) {
                    TextureRenderer.RenderText(AtomTypes.repeat.defaultName, corner + new Vector2(spacing.X * 2, -40), Assets.fonts.crimson_9_75, UI.TextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
                }
            }
            return;
        }

        bool useVanillaList = currentPageBonds == 1;
        List<BondType> bonds = useVanillaList ? [
            BondTypes.prisma0,
            BondTypes.prisma1,
            BondTypes.prisma2,
        ] : QApi.ModBondTypes;


        Vector2 pos = corner;
        for (int x = 0; x < 3; x++) {
            int index = (useVanillaList ? 0 : 3 * (currentPageBonds - 2)) + x;
            if (index >= bonds.Count) {
                goto outer;
            }
            BondTypeSelector(pos, bonds[index]);
            if (showExtra) {
                bool hovering = Bounds2.WithSize(pos - new Vector2(30, 30), new Vector2(61, 61)).Contains(InputManager.MousePos());
                if (hovering) {
                    TextureRenderer.RenderText(bonds[index].Id, pos + new Vector2(0, -40), Assets.fonts.crimson_9_75, UI.TextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
                }
            }
            pos.X += spacing.X;
        }
    outer:
        // ...
        return;
    }
    private void BondTypeSelector(Vector2 pos, BondType bondType) {
        Vector2 vector = (pos - Assets.textures.molecule_editor.atom_outline.size.ToVector2() / 2).Rounded();
        TextureRenderer.Render((bondType == selectedBondType) ? Assets.textures.molecule_editor.atom_selected : Assets.textures.molecule_editor.atom_outline, vector);
        if (bondType == summedTriplexPlaceHolder) {
            TextureRenderer.Render(BondTypes.prisma2.bondTexture.texture, (pos - BondTypes.prisma2.bondTexture.texture.size.ToVector2() / 2).Rounded());
            TextureRenderer.Render(BondTypes.prisma1.bondTexture.texture, (pos - BondTypes.prisma1.bondTexture.texture.size.ToVector2() / 2).Rounded());
            TextureRenderer.Render(BondTypes.prisma0.bondTexture.texture, (pos - BondTypes.prisma0.bondTexture.texture.size.ToVector2() / 2).Rounded());
        } else {
            TextureRenderer.Render(bondType.bondTexture.texture, (pos - bondType.bondTexture.texture.size.ToVector2() / 2).Rounded());
        }
        if (Vector2.Distance(pos, InputManager.MousePos()) <= 37f) {
            Texture grid_circle_hover = Assets.textures.molecule_editor.grid_circle_hover;
            Vector2 vector2 = (pos - grid_circle_hover.size.ToVector2() / 2).Rounded();
            TextureRenderer.Render(grid_circle_hover, vector2);
            if (InputManager.IsClickPressed(MouseButtonType.LeftClick) && selectedBondType != bondType) {
                selectedBondType = bondType;
                Assets.sounds.click_button.PlaySound(1f);
            }
        }
    }
    private bool OnAtomClick(HexIndex index) {
        if (selectedBondType == null) return false;
        if (selectedBondType.validDistances.Any(pos => pos.Q > 1 || pos.R > 1)) {
            if (selectedBondIndex == null) {
                selectedBondIndex = index;
            } else if (selectedBondIndex == index) {
                selectedBondIndex = null;
            } else if (selectedBondType.validDistances.Contains(selectedBondIndex.Value - index)) {
                if (SafeContainsBond(molecule.GetBondAt(selectedBondIndex.Value, index), selectedBondType)) {
                    SafeRemoveBond(molecule, selectedBondType, selectedBondIndex.Value, index);
                } else
                    SafeAddBond(molecule, selectedBondType, selectedBondIndex.Value, index);
                selectedBondIndex = null;
            }
            return true;
        }
        return false;
    }
    private void OnAddBond(HexIndex primary, HexIndex secondary) {
        if (selectedBondType.validDistances.Contains(secondary - primary)) {
            bool fullRemove = InputManager.IsModifierKeyHeld(ModifierKeyType.Shift);
            if (fullRemove) {
                molecule.RemoveBond(primary, secondary);
                SafeAddBond(molecule, selectedBondType, primary, secondary);
            } else {
                if (SafeContainsBond(molecule.GetBondAt(primary, secondary), selectedBondType)) {
                    SafeRemoveBond(molecule, selectedBondType, primary, secondary);
                } else
                    SafeAddBond(molecule, selectedBondType, primary, secondary);
            }
        }
    }
    private void OnRemoveBond(HexIndex primary, HexIndex secondary) {
        bool fullRemove = InputManager.IsModifierKeyHeld(ModifierKeyType.Shift);
        if (fullRemove) {
            molecule.RemoveBond(primary, secondary);
        } else if (SafeContainsBond(molecule.GetBondAt(primary, secondary), selectedBondType)) {
            SafeRemoveBond(molecule, selectedBondType, primary, secondary);
        }
    }

    private void SafeAddBond(Molecule molecule, BondType bondType, HexIndex pos1, HexIndex pos2) {
        if (bondType == summedTriplexPlaceHolder) {
            molecule.AddBond(BondTypes.prisma0, pos1, pos2);
            molecule.AddBond(BondTypes.prisma1, pos1, pos2);
            molecule.AddBond(BondTypes.prisma2, pos1, pos2);
        } else
            molecule.AddBond(selectedBondType, pos1, pos2);
    }
    private void SafeRemoveBond(Molecule molecule, BondType bondType, HexIndex pos1, HexIndex pos2) {
        if (bondType == summedTriplexPlaceHolder) {
            molecule.RemoveBond(BondTypes.prisma0, pos1, pos2);
            molecule.RemoveBond(BondTypes.prisma1, pos1, pos2);
            molecule.RemoveBond(BondTypes.prisma2, pos1, pos2);
        } else
            molecule.RemoveBond(selectedBondType, pos1, pos2);
    }
    private bool SafeContainsBond(Maybe<Bond> bond, BondType bondType) {
        if (!bond.HasValue()) return false;
        if (bondType == summedTriplexPlaceHolder) {
            var types = bond.GetValue().GetBondTypes();
            return types.Contains(BondTypes.prisma0) && types.Contains(BondTypes.prisma1) && types.Contains(BondTypes.prisma2);
        }
        return bond.GetValue().GetBondTypes().Contains(selectedBondType);
    }
    private void RenderMoleculeHighlights(Vector2 offset) {
        if (selectedBondIndex == null) return;
        var atoms = molecule.GetAtoms();
        Texture highlight = Assets.textures.molecule_editor.grid_circle_hover;
        Vector2 size = new Vector2(1.65f, 1.65f) * highlight.size.ToVector2();
        Texture highlight2 = Assets.textures.molecule_editor.atom_selected;
        Vector2 size2 = new Vector2(0.85f, 0.85f) * highlight2.size.ToVector2();
        if (atoms.ContainsKey(selectedBondIndex.Value)) {
            Vector2 atomPos = HexGrid.standardGrid.ToPixelCoords(selectedBondIndex.Value, offset);
            TextureRenderer.Render(highlight, Color.White, (atomPos - size / 2).Rounded(), size);
            TextureRenderer.Render(highlight2, Color.White.WithAlpha(0.7f), (atomPos - size2 / 2).Rounded(), size2);
        }
        foreach (var distance in selectedBondType.validDistances) {
            if (atoms.ContainsKey(selectedBondIndex.Value + distance)) {
                Vector2 atomPos = HexGrid.standardGrid.ToPixelCoords(selectedBondIndex.Value + distance, offset);
                TextureRenderer.Render(highlight, Color.White, (atomPos - size / 2).Rounded(), size);
                TextureRenderer.Render(highlight2, Color.White.WithAlpha(0.7f), (atomPos - size2 / 2).Rounded(), size2);
            }
        }
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

        MethodDefinition to = method.DeclaringType.Methods.First(m => m.Name.Equals("DrawAtoms"));
        MethodDefinition to2 = method.DeclaringType.Methods.First(m => m.Name.Equals("DrawBonds"));
        MethodDefinition onAtomClick = method.DeclaringType.Methods.First(m => m.Name.Equals("OnAtomClick"));
        MethodDefinition onAddBond = method.DeclaringType.Methods.First(m => m.Name.Equals("OnAddBond"));
        MethodDefinition onRemoveBond = method.DeclaringType.Methods.First(m => m.Name.Equals("OnRemoveBond"));
        MethodDefinition renderHighlights = method.DeclaringType.Methods.First(m => m.Name.Equals("RenderMoleculeHighlights"));

        cursor.Goto(start);
        cursor.RemoveRange(end - start); // Go bye with you
        cursor.Emit(OpCodes.Ldarg_0); // this
        cursor.Emit(OpCodes.Ldloc, 9);
        cursor.Emit(OpCodes.Ldloc, 8);
        cursor.Emit(OpCodes.Callvirt, to);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdsfld("Assets", "textures"));
        cursor.Index--;
        start = cursor.Index;
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt("MoleculeEditorScreen", "AtomTypeSelector"));
        end = cursor.Index;

        cursor.Goto(start);
        cursor.RemoveRange(end - start); // Go bye with you
        cursor.Emit(OpCodes.Ldloc, 8);
        cursor.Emit(OpCodes.Callvirt, to2);

        cursor.GotoPrev(MoveType.Before, instr => instr.MatchCallvirt("Molecule", "ReplaceAtom"));
        var endInstr = cursor.Next.Next.Operand as Instruction;
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.EmitLdarg0();
        cursor.EmitLdloc(14);
        cursor.EmitCallvirt(onAtomClick);
        cursor.Emit(OpCodes.Brtrue_S, endInstr);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchCallvirt("Molecule", "RemoveBond"));
        start = cursor.Index - 1;
        cursor.Index -= 3;
        cursor.Remove();
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt("Molecule", "AddBond"));
        cursor.Index++;
        end = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(end - start); // Go bye with you
        cursor.EmitCall(onAddBond);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchCallvirt("Molecule", "RemoveBond"));
        start = cursor.Index - 1;
        cursor.Index -= 3;
        cursor.Remove();
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt("Molecule", "RemoveBond"));
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt("Molecule", "RemoveBond"));
        cursor.Index++;
        end = cursor.Index;
        cursor.Goto(start);
        cursor.RemoveRange(end - start); // Go bye with you
        cursor.EmitCall(onRemoveBond);

        cursor.GotoPrev(MoveType.After, instr => instr.MatchCallvirt("MoleculeEditorScreen", "RenderMolecule"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        cursor.EmitLdarg0();
        cursor.EmitLdloc(5);
        cursor.EmitCall(renderHighlights);
    }
}