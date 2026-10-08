#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeModifiers

using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using Quintessential;
using Quintessential.Internal;
using System;
using System.Linq;

class patch_PuzzleEditorScreen {

	private Scrollbar scrollbar; // initializer is not merged

	[MonoModIgnore]
	private readonly PuzzleEditorScreenType type;
	[MonoModIgnore]
	private Maybe<Puzzle> selectedPuzzle;

	[MonoModIgnore]
    private static extern void RenderPermissionToggle(bool isAvailable, Puzzle puzzle, Vector2 pos, string name, PuzzlePermissions permissionFlag, bool isEnabled);

	[MonoModReplace]
	private void RenderEditor(bool isPersonal, Vector2 pos, Bounds2 bounds, Puzzle puzzle) {
		scrollbar ??= new();

		// reimplement this section
		Vector2 size = new(1516f, 922f);
		Bounds2 panelSize = Bounds2.WithSize(pos + new Vector2(0, 88 + 5), size + new Vector2(-152f + 78, -158f - 40 - 10));
		Bounds2 coverBounds = panelSize.Translated(new(80, 0));

		// add scrollbar/scroll region
		using (var _ = scrollbar.RenderScrollbar(panelSize.Min, panelSize.Size.CeilingToInt(), 0, -30)) {
			// clear scroll zone
			class_226.method_600(Color.Transparent);

			var nCorner = new Vector2(-12, scrollbar.scrollOffset - 95);

			//// CustomPermissions may have just not been set? TODO: find a better place for the "canonical" setter
			var conv = puzzle;
			conv.CustomPermissions ??= [];

            VanillaEditorRender(isPersonal, nCorner, bounds, puzzle);

			Vector2 ruleSize = new(236, -37);
			Vector2 partsCorner = nCorner + new Vector2(494f, 219f);

			// instructions selection
			Vector2 instructionsCorner = new(nCorner.X + 489, partsCorner.Y + ruleSize.Y * 4);
			UIUtils.RenderScreenTitle(Translations.Translate("Instructions"), instructionsCorner, 900, false, true);

			InstructionType[] types = InstructionTypes.instructions;
			var i = 0;
			foreach (var type in types) {
				var basePos = instructionsCorner + new Vector2(50 + 60 * i, -60);
				var istructionPos = basePos;
				if (type.permissionCategory == PuzzlePermissions.None)
					continue;
				bool enabled = puzzle.permissionFlags.HasFlag(type.permissionCategory);

				Texture @base;
				if (enabled)
					@base = Assets.textures.solution_editor.program_panel.instruction;
				else {
					@base = Assets.textures.solution_editor.program_panel.instruction_disabled;
					istructionPos += new Vector2(3, -3);
				}

				TextureRenderer.Render(@base, basePos);
				TextureRenderer.Render(type.enabledTexture, istructionPos + new Vector2(1, 2));

				if (isPersonal && Bounds2.WithSize(basePos, @base.size.ToVector2()).Contains(InputManager.MousePos())) {
                    Texture highlight = Assets.textures.solution_editor.program_panel.instruction_highlight;
                    TextureRenderer.Render(highlight, istructionPos + new Vector2(2, 4));

                    if (InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
                        puzzle.permissionFlags ^= type.permissionCategory;
                        puzzle.SaveToFile(GameLogic.instance.workshopManager.CustomPuzzlePath(puzzle));
                    }
                }
				i++;
			}

            // quintessential rules
            var cursor = instructionsCorner + new Vector2(0, ruleSize.Y * 3.5f);
            //UIUtils.RenderScreenTitle(QuintessentialUI.Instance.Translate("editor.sections.quint"), rulesCorner - new Vector2(0, ruleSize.Y * .5f), 900, false, true);
            //if (UI.DrawCheckbox(rulesCorner + new Vector2(ruleSize.X * 0 + 5, ruleSize.Y * 1), QuintessentialUI.Instance.Translate("editor.sections.quint.toggle"), conv.IsModdedPuzzle)) {
            //    //conv.ConvertFormat(!conv.IsModdedPuzzle);
            //}
            //if (conv.IsModdedPuzzle) {
            //    TextureRenderer.RenderText(QuintessentialUI.Instance.Translate("editor.sections.quint.warning"), rulesCorner + new Vector2(5, ruleSize.Y * 1.5f), Assets.fonts.crimson_13, Color.Red, TextAlignment.Left, 1, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
            //}
            //Vector2 cursor = rulesCorner + new Vector2(0, ruleSize.Y * 2.5f);

            // modded categories, if enabled
            foreach (var category in QApi.PuzzleOptions.GroupBy(k => k.SectionName)) {
                UIUtils.RenderScreenTitle(category.Key, cursor, 900, false, true);

                var idx = 0;
                foreach (var option in category) {
                    // ReSharper disable once PossibleLossOfFraction
                    Vector2 selectorPos = cursor + new Vector2(ruleSize.X / 2f * (idx % 8) + 5, ruleSize.Y * (idx / 8 + 1.5f));
                    // TODO: other option types
                    if (option.Type == PuzzleOptionType.Boolean) {
                        bool enabled = conv.CustomPermissions.Contains(option.ID);
                        if (UI.DrawCheckbox(selectorPos, option.Name, enabled) && isPersonal) {
                            if (enabled)
                                conv.CustomPermissions.Remove(option.ID);
                            else
                                conv.CustomPermissions.Add(option.ID);
                            puzzle.SaveToFile(GameLogic.instance.workshopManager.CustomPuzzlePath(puzzle));
                        }
                        if (option.length == 0) idx += 2;
                        else idx += option.length;
                    } else if (option.Type == PuzzleOptionType.Atom) {
                        var currentChoice = option.AtomIn(puzzle);
                        if (DrawAtomSelector(selectorPos, option.Name, currentChoice ?? AtomTypes.repeat))
                            UI.OpenScreen(new AtomSelectScreen(QuintessentialUI.Instance.Translate("editor.select_option") + " " + option.Name, type => {
                                option.SetAtomIn(puzzle, type);
                                puzzle.SaveToFile(GameLogic.instance.workshopManager.CustomPuzzlePath(puzzle));
                            }, currentChoice));
                        idx++;
                    }
                }
                var rows = (int)Math.Ceiling(idx / 8f);
                cursor += new Vector2(0, ruleSize.Y * (rows + 2));
            }

            // expand the scroll area to cover the entire displayed area
            // we're off by one row
            scrollbar.SetHeightAndClamp(nCorner.Y - cursor.Y + panelSize.Height - ruleSize.Y + 24);
		}
	}

	private void VanillaEditorRender(bool isPersonal, Vector2 pos, Bounds2 bounds, Puzzle puzzle) {

        PuzzleEditorScreen.PuzzleContainer puzzleCont = new() {
            field_4622 = puzzle
        };
        UIUtils.RenderScreenTitle(Translations.Translate("Products"), pos + new Vector2(489f, 774f), 904, false, true);
        UIUtils.RenderScreenTitle(Translations.Translate("Reagents"), pos + new Vector2(489f, 522f), 904, false, true);
        UIUtils.RenderScreenTitle(Translations.Translate("Mechanisms and Glyphs"), pos + new Vector2(489f, 270f), 904, false, true);

        bool screenOpened = false;
        for (int i = 0; i < 2; i++) {
            PuzzleInputOutput[] array = ((i == 0) ? puzzleCont.field_4622.outputs : puzzleCont.field_4622.inputs);
            string text = ((i == 0) ? Translations.Translate("Create New Product").ToUpper() : Translations.Translate("Create New Reagent").ToUpper());
            for (int j = 0; j < 4; j++) {
                Bounds2 bounds2 = Bounds2.WithSize(pos + new Vector2(495f, 588f) + new Vector2((float)(j * 236), (float)((i == 0) ? (-28) : (-281))), new Vector2(226f, 201f));
                if (array.Length > j) {
                    TextureRenderer.Render(isPersonal ? Assets.textures.puzzle_editor.product : Assets.textures.puzzle_editor.product_no_close, bounds2.Min);
                    bool flag = false;
                    if (isPersonal) {
                        Bounds2 bounds3 = Bounds2.WithSize(bounds2.Min + new Vector2(176f, 165f), Assets.textures.puzzle_editor.product_close.size.ToVector2());
                        bool flag2 = bounds3.Contains(InputManager.MousePos());
                        if (!flag2 && bounds2.Contains(InputManager.MousePos())) {
                            flag = true;
                            if (InputManager.IsClickPressed((MouseButtonType)1)) {
                                int I = i;
                                int J = j;
                                var moleculeEditorScreen = new MoleculeEditorScreen(array[J].molecule, I == 0, new Action<Molecule>(molecule => {
                                    (I == 0 ? puzzleCont.field_4622.outputs : puzzleCont.field_4622.inputs)[J].molecule = molecule;
                                    puzzle.SaveToFile(GameLogic.instance.workshopManager.CustomPuzzlePath(puzzle));
                                }));
                                ((patch_MoleculeEditorScreen)(object)moleculeEditorScreen).editing = puzzle;
                                screenOpened = true;

                                GameLogic.instance.PushScreen(moleculeEditorScreen);
                                Assets.sounds.click_button.PlaySound(1f);
                            }
                        }
                        TextureRenderer.Render(Assets.textures.puzzle_editor.product_close, bounds3.Min);
                        if (flag2) {
                            TextureRenderer.Render(Assets.textures.puzzle_editor.product_close_hover, bounds3.Min);
                            if (InputManager.IsClickPressed((MouseButtonType)1)) {

                                int I = i;
                                int J = j;
                                GameLogic.instance.PushScreen(MessageBoxScreen.ConfirmBox(bounds, true, (i == 0) ? Translations.Translate("Do you really want to delete this product?") : Translations.Translate("Do you really want to delete this reagent?"), MaybeHelper.empty, (i == 0) ? Translations.Translate("Delete Product") : Translations.Translate("Delete Reagent"), Translations.Translate("Cancel"), () => {

                                    if (I == 0) {
                                        puzzleCont.field_4622.outputs = [.. puzzleCont.field_4622.outputs.Take(J).Concat(puzzleCont.field_4622.outputs.Skip(J + 1))];
                                    } else {
                                        puzzleCont.field_4622.inputs = [.. puzzleCont.field_4622.inputs.Take(J).Concat(puzzleCont.field_4622.inputs.Skip(J + 1))];
                                    }
                                    puzzle.SaveToFile(GameLogic.instance.workshopManager.CustomPuzzlePath(puzzle));
                                }, () => { }));
                                Assets.sounds.click_button.PlaySound(1f);
                            }
                        }

                        Vector2 namePos = bounds2.BottomLeft + new Vector2(bounds2.Width / 2f - 7, -17);
                        var isElement = array[j].molecule.GetAtoms().Count == 1;
                        var fallbackPvw = isElement ? ("_(" + array[j].molecule.GetAtoms().Values.First().atomType.elementalName + ")_") : QuintessentialUI.Instance.ModId + ".editor.unnamed_molecule";
                        Bounds2 textArea = TextureRenderer.RenderText(array[j].molecule.displayName.GetOrDefault(Translations.Translate(fallbackPvw)), namePos, Assets.fonts.crimson_13, UI.TextColor, TextAlignment.Center, 1f, 0.6f, 236, 206, 0, new Color(), null, int.MaxValue, true, true);
                        if (textArea.Contains(InputManager.MousePos()) && InputManager.IsClickPressed(MouseButtonType.LeftClick) && !screenOpened) {
                            screenOpened = true;
                            int J = j;
                            GameLogic.instance.PushScreen(
                                MessageBoxScreenEx.Textbox(
                                    bounds,
                                    QuintessentialUI.Instance.Translate("editor.renaming." + (i == 0 ? "product" : "reagent")),
                                    array[j].molecule.displayName.HasValue() ? array[j].molecule.displayName.GetValue() : (isElement ? array[j].molecule.GetAtoms().Values.First().atomType.elementalName : ""),
                                    QuintessentialUI.Instance.Translate("editor.renaming.confirm_" + (i == 0 ? "product" : "reagent")),
                                    s => {
                                        array[J].molecule.displayName = Translations.Translate(s);
                                        puzzle.SaveToFile(GameLogic.instance.workshopManager.CustomPuzzlePath(puzzle));
                                    }
                                )
                            );
                            Assets.sounds.click_button.PlaySound(1f);
                        }
                    }
                    Texture renderedTexture = Editor.RenderMoleculeForDisplay(array[j].molecule, i != 0, flag, new Vector2(156f, 146f), false, MaybeHelper.empty).GetTarget().renderedTexture;
                    Vector2 vector = (renderedTexture.size.ToVector2() / 2).Rounded();
                    Vector2 vector2 = bounds2.Center.Rounded() - vector + new Vector2(-8f, 0f);
                    TextureRenderer.Render(renderedTexture, vector2);
                } else if (isPersonal) {
                    Vector2 vector3 = new(-2f, -3f);
                    TextureRenderer.Render(Assets.textures.puzzle_editor.new_product, bounds2.Min + vector3);
                    TextureRenderer.RenderText(text, bounds2.Center + new Vector2(-6f, 0f), Assets.fonts.crimson_13, UIConsts.lightTextColor, TextAlignment.Center, 1f, 0.6f, 120f, float.MaxValue, 0, default, null, int.MaxValue, false, true);
                    if (bounds2.Contains(InputManager.MousePos())) {
                        TextureRenderer.Render(Assets.textures.puzzle_editor.new_product_hover, bounds2.Min + vector3);
                        if (InputManager.IsClickPressed(MouseButtonType.LeftClick)) {

                            int I = i;
                            var moleculeEditorScreen =new MoleculeEditorScreen(new Molecule(), i == 0, new Action<Molecule>(molecule => {
                                if (I == 0) {
                                    puzzleCont.field_4622.outputs = [.. puzzleCont.field_4622.outputs, new PuzzleInputOutput(molecule)];
                                } else {
                                    puzzleCont.field_4622.inputs = [.. puzzleCont.field_4622.inputs, new PuzzleInputOutput(molecule)];
                                }
                                puzzle.SaveToFile(GameLogic.instance.workshopManager.CustomPuzzlePath(puzzle));
                            }));
                            ((patch_MoleculeEditorScreen)(object)moleculeEditorScreen).editing = puzzle;

							GameLogic.instance.PushScreen(moleculeEditorScreen);
                            Assets.sounds.click_button.PlaySound(1f);
                        }
                    }
                }
            }
        }
        bool flag3 = isPersonal && !PuzzleSelectScreen.HasDRM();
        Vector2 vector4 = pos + new Vector2(494f, 219f);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(0, 0), Translations.Translate("Glyph of Bonding###Shorter"), PuzzlePermissions.Bonder, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(1, 0), Translations.Translate("Glyph of Multi-bonding###Shorter"), PuzzlePermissions.SpeedBonder, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(2, 0), Translations.Translate("Glyph of Triplex-bonding###Shorter"), PuzzlePermissions.PrismaBonder, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(3, 0), Translations.Translate("Glyph of Unbonding###Shorter"), PuzzlePermissions.Unbonder, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(0, 1), Translations.Translate("Glyph of Calcification###Shorter"), PuzzlePermissions.Calcification, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(1, 1), Translations.Translate("Glyph of Duplication###Shorter"), PuzzlePermissions.Duplication, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(2, 1), PartTypes.berlosWheel.name, PuzzlePermissions.BaronWheel, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(3, 1), Translations.Translate("Glyph of Animismus###Shorter"), PuzzlePermissions.LifeAndDeath, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(0, 2), Translations.Translate("Glyph of Projection###Shorter"), PuzzlePermissions.Projection, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(1, 2), Translations.Translate("Glyph of Purification###Shorter"), PuzzlePermissions.Purification, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(2, 2), Translations.Translate("Glyph of Disposal###Shorter"), PuzzlePermissions.Disposal, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(3, 2), Translations.Translate("Glyphs of Quintessence"), PuzzlePermissions.Quintessence, false);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(0, 3), Translations.Translate("Glyph of Rejection###Shorter"), PuzzlePermissions.Rejection, flag3);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(1, 3), Translations.Translate("Glyph of Division###Shorter"), PuzzlePermissions.Division, flag3);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(2, 3), Translations.Translate("Glyph of Proliferation###Shorter"), PuzzlePermissions.Proliferation, flag3);
        RenderPermissionToggle(isPersonal, puzzleCont.field_4622, vector4 + GetPermissionTogglePos(3, 3), PartTypes.ravarisWheel.name, PuzzlePermissions.RavariWheel, flag3);
    }
    internal static Vector2 GetPermissionTogglePos(int x, int y) {
        return new Vector2((float)(236 * x), (float)(-37 * y));
    }

    // TODO: generalize?
    private static bool DrawAtomSelector(Vector2 pos, string label, AtomType atom){
        Bounds2 labelBounds = TextureRenderer.RenderText(label, pos + new Vector2(45f, 13f), Assets.fonts.crimson_13, UI.TextColor, TextAlignment.Left, 1, 0.6f,float.MaxValue, float.MaxValue,0,new Color(),null,int.MaxValue,true, true);
        Vector2 atomPos = pos + new Vector2(17, 16);
		const float scale = 0.7f;
		Editor.RenderAtom(atom, atomPos, scale, 1, 1, 1, -21, 0, null, null, false);

		if(Vector2.Distance(atomPos, InputManager.MousePos()) < (37 * scale) || labelBounds.Contains(InputManager.MousePos())){
			Vector2 outlinePos = (atomPos - Assets.textures.molecule_editor.grid_circle_hover.size.ToVector2() * scale / 2).Rounded();
			var tex = Assets.textures.molecule_editor.grid_circle_hover;
			TextureRenderer.Render(tex, Color.White, outlinePos, tex.size.ToVector2() * 0.7f);
			if(InputManager.IsClickPressed(MouseButtonType.LeftClick)){
				Assets.sounds.click_button.PlaySound(1);
				return true;
			}
		}
		return false;
	}

	private static bool DrawPuzzleButton(Puzzle p, Vector2 param_3552, int param_3025, bool param_3553, bool param_3554, bool param_4458, bool param_4459) {
        bool shift = InputManager.IsModifierKeyHeld(ModifierKeyType.Shift);
        string name = shift ? "ID: " + p.puzzleId.Replace("_", "\\_") : p.puzzleName;

		// draw the button
		ButtonDrawingLogic bdl = UIUtils.PuzzleButton(name, param_3552, param_3025, param_3553, param_3554);
        bool flag = bdl.RenderAndCheckIfPressed(param_4458, param_4459);
		if (shift)
		{
			// draw hash line
			TextureRenderer.RenderText("HASH: " + p.fileHash.ToString(), bdl.getBounds().Center + new Vector2(-15f, -24f), Assets.fonts.crimson_13, UI.TextColor, TextAlignment.Center, 1f, 0.6f, float.MaxValue, param_3025 - 75, 0, default, null, int.MaxValue, false, true);
		}
        return flag;
    }

    [MonoModILInject("RenderFrame")]
    public static void PatchPuzzleEditorScreen(MethodDefinition method, CustomAttribute attrib) {
        MonoModRule.Modder.Log("Patching puzzle editor screen");
        if (!method.HasBody) {
            Console.WriteLine("Failed to modify puzzle editor screen (no body)!");
            throw new Exception();
        }

        ILCursor cursor = new(new ILContext(method));

        if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdfld(out FieldReference fr) && fr.Name == "puzzleName",
            instr => instr.MatchCall(out MethodReference mr) && mr.Name == "op_Implicit",
            instr => instr.MatchLdloc(13)
            )) {
            Console.WriteLine("Failed to modify puzzle editor screen (no puzzle name found)");
            throw new Exception();
        }
        cursor.RemoveRange(2);

        if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.OpCode == OpCodes.Call,
            instr => instr.MatchStloc(18),
            instr => instr.MatchLdloca(18))) {
            Console.WriteLine("Failed to modify puzzle editor screen (no ButtonDrawLogic instantiation)");
            throw new Exception();
        }

        cursor.RemoveRange(3);

        if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchCall(out MethodReference mr) && mr.Name == "RenderAndCheckIfPressed",
            instr => instr.OpCode == OpCodes.Brfalse_S,
            instr => instr.MatchLdloc(16))) {
            Console.WriteLine("Failed to modify puzzle editor screen (no ButtonDrawLogic call)");
            throw new Exception();
        }

        TypeDefinition holder = MonoModRule.Modder.FindType("PuzzleEditorScreen").Resolve();
        MethodDefinition getName = holder.Methods.First(m => m.Name.Equals("DrawPuzzleButton")); // TODO fix this line
        cursor.Remove();
        cursor.Emit(OpCodes.Call, getName);
    }
}