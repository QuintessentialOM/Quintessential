#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeModifiers

using System;
using System.Linq;
using MonoMod;
using MonoMod.Utils;
using Quintessential;

using Scrollbar = class_262;
using InstructionTypes = class_169;
using Permissions = enum_149;
using AtomTypes = class_175;

class patch_PuzzleEditorScreen{
	private Scrollbar scrollbar; // initializer is not merged

	[MonoModIgnore]
	[PatchPuzzleEditorScreen]
	public extern void method_50(float param);

	// name is used in MonoModRules
	private void DisplayEditorPanelScreen(){
		scrollbar ??= new();
		
		// reimplement this section
		DynamicData self = new(this);
		Vector2 size = new(1516f, 922f);
		Vector2 corner = (class_115.field_1433 / 2 - size / 2 + new Vector2(-2, -11)).Rounded();
		Bounds2 panelSize = Bounds2.WithSize(corner + new Vector2(0, 88 + 5), size + new Vector2(-152f + 78, -158f - 40 - 10));

		// add scrollbar/scroll region
		using(var _ = scrollbar.method_709(panelSize.Min, panelSize.Size.CeilingToInt(), 0, -30)){
			// clear scroll zone
			class_226.method_600(Color.Transparent);
			// draw headers
			var nCorner = new Vector2(-10, scrollbar.field_2078 - 100);

			class_140.method_317(class_134.method_253("Products", "FULL LENGTH"), nCorner + new Vector2(489, 774), 900, false, true);
			class_140.method_317(class_134.method_253("Reagents", ""), nCorner + new Vector2(489, 506), 900, false, true);
			class_140.method_317(class_134.method_253("Mechanisms and Glyphs", ""), nCorner + new Vector2(489, 237), 900, false, true);

			Puzzle myPuzzle = self.Get<Maybe<Puzzle>>("field_2789").method_1087();
			// CustomPermissions may have just not been set? TODO: find a better place for the "canonical" setter
			var conv = (patch_Puzzle)(object)myPuzzle;
			conv.CustomPermissions ??= new();

			// draw inputs/outputs
			bool b = self.Get<enum_138>("field_2788") == 0;
			for(var row = 0; row < 2; row++){
				PuzzleInputOutput[] puzzleIOs = row == 0 ? myPuzzle.field_2771 : myPuzzle.field_2770;
				for(var column = 0; column < 4; ++column){
					Bounds2 puzzleIoBox = Bounds2.WithSize(nCorner + new Vector2(495f, 576f) + new Vector2(column * 235, row == 0 ? -28f : -297f), new Vector2(226f, 226f));
					if(puzzleIOs.Length > column){
						class_135.method_272(b ? class_238.field_1989.field_94.field_805 : class_238.field_1989.field_94.field_808, puzzleIoBox.Min);
						var isHover = false;
						if(b){
							Bounds2 deleteBounds = Bounds2.WithSize(puzzleIoBox.Min + new Vector2(176f, 175f), class_238.field_1989.field_94.field_806.field_2056.ToVector2());
							bool isDelete = deleteBounds.Contains(Input.MousePos());
							// open editor if clicked
							if(!isDelete && puzzleIoBox.Contains(Input.MousePos())){
								isHover = true;
								if(Input.IsLeftClickPressed()){
									int columnTemp = column; // otherwise it's modified after(?)
									var moleculeEditorScreen = new MoleculeEditorScreen(puzzleIOs[column].field_2813, row == 0, value => {
										puzzleIOs[columnTemp].field_2813 = value;
										GameLogic.field_2434.field_2460.method_2241(myPuzzle);
									});
									((patch_MoleculeEditorScreen)(object)moleculeEditorScreen).editing = conv;
									GameLogic.field_2434.method_946(moleculeEditorScreen);
									class_238.field_1991.field_1821.method_28(1f);
								}
							}

							class_135.method_272(class_238.field_1989.field_94.field_806, deleteBounds.Min);
							// open "are you sure" menu if X is clicked
							if(isDelete){
								class_135.method_272(class_238.field_1989.field_94.field_807, deleteBounds.Min);
								if(Input.IsLeftClickPressed()){
									int rowTemp = row;
									int columnTemp = column;
									GameLogic.field_2434.method_946(MessageBoxScreen.method_1095(panelSize.Translated(new(80, 0)), true, row == 0 ? class_134.method_253("Do you really want to delete this product?", string.Empty) : class_134.method_253("Do you really want to delete this reagent?", string.Empty), struct_18.field_1431, row == 0 ? class_134.method_253("Delete Product", string.Empty) : class_134.method_253("Delete Reagent", string.Empty), class_134.method_253("Cancel", string.Empty),
										() => {
											if(rowTemp == 0)
												myPuzzle.field_2771 = myPuzzle.field_2771.Take(columnTemp).Concat(myPuzzle.field_2771.Skip(columnTemp + 1)).ToArray();
											else
												myPuzzle.field_2770 = myPuzzle.field_2770.Take(columnTemp).Concat(myPuzzle.field_2770.Skip(columnTemp + 1)).ToArray();
											GameLogic.field_2434.field_2460.method_2241(myPuzzle);
										}, /* cancel is no-op */ () => { }));
									class_238.field_1991.field_1821.method_28(1f);
								}
							}
						}

						class_256 moleculeSprite = Editor.method_928(puzzleIOs[column].field_2813, (uint)row > 0U, isHover, new Vector2(156f, 146f), false, struct_18.field_1431).method_1351().field_937;
						Vector2 centre = moleculeSprite.field_2056.ToVector2() / 2;
						Vector2 halfSize = centre.Rounded();
						centre = puzzleIoBox.Center;
						class_135.method_272(moleculeSprite, centre.Rounded() - halfSize + new Vector2(-8f, -10f));
					}else if(b){
						Vector2 offset = new(-2f, -3f);
						class_135.method_272(class_238.field_1989.field_94.field_802, puzzleIoBox.Min + offset);
						class_135.method_290(row == 0 ? class_134.method_253("Create New Product", string.Empty).method_1060() : class_134.method_253("Create New Reagent", string.Empty).method_1060(), puzzleIoBox.Center + new Vector2(-6f, -8f), class_238.field_1990.field_2143, class_181.field_1718, (enum_0)1, 1f, 0.6f, 120f, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);

						if(!puzzleIoBox.Contains(Input.MousePos())) continue;
						class_135.method_272(class_238.field_1989.field_94.field_803, puzzleIoBox.Min + offset);

						if(!class_115.method_206((enum_142)1)) continue;
						int rowTemp = row; // otherwise it's modified after
						var moleculeEditorScreen = new MoleculeEditorScreen(new Molecule(), row == 0, value => {
							if(rowTemp == 0)
								myPuzzle.field_2771 = myPuzzle.field_2771.method_451(new PuzzleInputOutput(value)).ToArray();
							else
								myPuzzle.field_2770 = myPuzzle.field_2770.method_451(new PuzzleInputOutput(value)).ToArray();
							GameLogic.field_2434.field_2460.method_2241(myPuzzle);
						});
						((patch_MoleculeEditorScreen)(object)moleculeEditorScreen).editing = conv;
						GameLogic.field_2434.method_946(moleculeEditorScreen);
						class_238.field_1991.field_1821.method_28(1f);
					}
				}
			}

			// draw vanilla rule checkboxes
			Vector2 ruleSize = new(236, -37);
			Vector2 partsCorner = nCorner + new Vector2(494, 184);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 0, ruleSize.Y * 0), (string)class_191.field_1772.field_1529, enum_149.Bonder, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 1, ruleSize.Y * 0), (string)class_191.field_1774.field_1529, enum_149.SpeedBonder, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 2, ruleSize.Y * 0), (string)class_191.field_1775.field_1529, enum_149.PrismaBonder, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 3, ruleSize.Y * 0), (string)class_191.field_1773.field_1529, enum_149.Unbonder, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 0, ruleSize.Y * 1), (string)class_191.field_1776.field_1529, enum_149.Calcification, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 1, ruleSize.Y * 1), (string)class_191.field_1777.field_1529, enum_149.Duplication, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 2, ruleSize.Y * 1), (string)class_191.field_1771.field_1529, enum_149.BaronWheel, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 3, ruleSize.Y * 1), (string)class_191.field_1780.field_1529, enum_149.LifeAndDeath, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 0, ruleSize.Y * 2), (string)class_191.field_1778.field_1529, enum_149.Projection, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 1, ruleSize.Y * 2), (string)class_191.field_1779.field_1529, enum_149.Purification, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 2, ruleSize.Y * 2), (string)class_191.field_1781.field_1529, enum_149.Disposal, myPuzzle);
			self.Invoke("method_1261", partsCorner + new Vector2(ruleSize.X * 3, ruleSize.Y * 2), (string)class_134.method_253("Glyphs of Quintessence", string.Empty), enum_149.Quintessence, myPuzzle);

			// instructions selection
			Vector2 instructionsCorner = new(nCorner.X + 489, partsCorner.Y + ruleSize.Y * 3);
			class_140.method_317(class_134.method_253("Instructions", ""), instructionsCorner, 900, false, true);

			InstructionType[] types = InstructionTypes.field_1667;
			var i = 0;
			foreach(var type in types){
				var basePos = instructionsCorner + new Vector2(80 + 60 * i, -60);
				var pos = basePos;
				if(type.field_2550 == Permissions.None)
					continue;
				bool enabled = myPuzzle.field_2773.HasFlag(type.field_2550);

				class_256 @base;
				if(enabled)
					@base = class_238.field_1989.field_99.field_706.field_716;
				else{
					@base = class_238.field_1989.field_99.field_706.field_717;
					pos += new Vector2(3, -3);
				}

				bool hovered = Bounds2.WithSize(basePos, @base.field_2056.ToVector2()).Contains(Input.MousePos());
				class_256 highlight = class_238.field_1989.field_99.field_706.field_720;
				
				UI.DrawTexture(@base, basePos);
				UI.DrawTexture(type.field_2546, pos + new Vector2(1, 2));
				if(hovered)
					UI.DrawTexture(highlight, pos + new Vector2(2, 4));

				if(hovered && Input.IsLeftClickPressed()){
					myPuzzle.field_2773 ^= type.field_2550;
					GameLogic.field_2434.field_2460.method_2241(myPuzzle);
				}
				
				i++;
			}

			// quintessential rules
			var rulesCorner = instructionsCorner + new Vector2(0, ruleSize.Y * 3.5f);
			class_140.method_317(class_134.method_253("Quintessential Rules", ""), rulesCorner - new Vector2(0, ruleSize.Y * .5f), 900, false, true);
			if(UI.DrawCheckbox(rulesCorner + new Vector2(ruleSize.X * 0 + 5, ruleSize.Y * 1), "Enable Modded Content", conv.IsModdedPuzzle))
				conv.ConvertFormat(!conv.IsModdedPuzzle);
			// TODO: will probably move to a separate mod
			//UI.DrawCheckbox(rulesCorner + new Vector2(ruleSize.X * 1 + 5, ruleSize.Y * 1), "Allow Overlap", false);
			
			// modded categories, if enabled
			Vector2 cursor = rulesCorner + new Vector2(0, ruleSize.Y * 2.5f);
			if(conv.IsModdedPuzzle)
				foreach(var category in QApi.PuzzleOptions.GroupBy(k => k.SectionName)){
					class_140.method_317(category.Key, cursor, 900, false, true);

					var idx = 0;
					foreach(var option in category){
						// ReSharper disable once PossibleLossOfFraction
						Vector2 pos = cursor + new Vector2(ruleSize.X * (idx % 4) + 5, ruleSize.Y * (idx / 4 + 1.5f));
						// TODO: other option types
						if(option.Type == PuzzleOptionType.Boolean){
							bool enabled = conv.CustomPermissions.Contains(option.ID);
							if(UI.DrawCheckbox(pos, option.Name, enabled)){
								if(enabled)
									conv.CustomPermissions.Remove(option.ID);
								else
									conv.CustomPermissions.Add(option.ID);
								GameLogic.field_2434.field_2460.method_2241(myPuzzle);
							}
						}else if(option.Type == PuzzleOptionType.Atom){
							var currentChoice = option.AtomIn(myPuzzle);
							if(DrawAtomSelector(pos, option.Name, currentChoice ?? AtomTypes.field_1689))
								UI.OpenScreen(new AtomSelectScreen("Select: " + option.Name, type => {
									option.SetAtomIn(myPuzzle, type);
									GameLogic.field_2434.field_2460.method_2241(myPuzzle);
								}, currentChoice));
						}

						idx++;
					}

					var rows = (int)Math.Ceiling(idx / 4f);
					cursor += new Vector2(0, ruleSize.Y * (rows + 2));
				}

			// expand the scroll area to cover the entire displayed area
			// we're off by one row
			scrollbar.method_707(nCorner.Y - cursor.Y + panelSize.Height - ruleSize.Y + 24);
		}
	}

	// TODO: generalize?
	private static bool DrawAtomSelector(Vector2 pos, string label, AtomType atom){
		Bounds2 labelBounds = UI.DrawText(label, pos + new Vector2(45f, 13f), UI.SubTitle, UI.TextColor, TextAlignment.Left);
		Vector2 atomPos = pos + new Vector2(17, 16);
		const float scale = 0.7f;
		Editor.method_927(atom, atomPos, scale, 1, 1, 1, -21, 0, null, null, false);

		if(Vector2.Distance(atomPos, Input.MousePos()) < (37 * scale) || labelBounds.Contains(Input.MousePos())){
			Vector2 outlinePos = (atomPos - class_238.field_1989.field_89.field_124.field_2056.ToVector2() * scale / 2).Rounded();
			var tex = class_238.field_1989.field_89.field_124;
			class_135.method_263(tex, Color.White, outlinePos, tex.field_2056.ToVector2() * 0.7f);
			if(Input.IsLeftClickPressed()){
				class_238.field_1991.field_1821.method_28(1);
				return true;
			}
		}
		return false;
	}
}