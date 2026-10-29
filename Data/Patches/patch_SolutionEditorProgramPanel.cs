using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using Quintessential;
using System;
using System.Linq;

internal class patch_SolutionEditorProgramPanel : SolutionEditorProgramPanel {
    [MonoModIgnore] public patch_SolutionEditorProgramPanel(SolutionEditorScreen solEditScrn) : base(solEditScrn) { }
    [MonoModIgnore] private extern bool IsLimitReached(InstructionType instructionType);
    [MonoModIgnore] [MonoModRemove] private extern void RenderInstructionInTray(InstructionType instructionType, Vector2 pos, Maybe<InstructionType> selectedInstructionType);

    private (bool, bool) RenderInstructionInTray(InstructionType instructionType, Vector2 pos, Maybe<InstructionType> selectedInstructionType, bool isSameLayer) {
        if (instructionType == null || !solEditScrn.GetSolution().GetPuzzle().HasPermissionForInstr(instructionType)) {
            if (isSameLayer) TextureRenderer.Render(Assets.textures.solution_editor.program_panel.instruction_disabled, pos + new Vector2(-2f, 3f));
            return (false, false);
        }
        if (isSameLayer) {
            TextureRenderer.Render(Assets.textures.UI.letter_shadow, Color.White, pos + new Vector2(8f, -17f));
            TextureRenderer.RenderText(SolutionEditorScreen.GetCharacterFromKey(instructionType.GetHotkey()).FormatBold(true), pos + new Vector2(18f, -8f), Assets.fonts.crimson_9_75, Color.FromHex(14474460), TextAlignment.Center, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default(Color), null, int.MaxValue, false, true);
        }
        if (IsLimitReached(instructionType)) {
            if (isSameLayer) {
                TextureRenderer.Render(Assets.textures.solution_editor.program_panel.instruction_disabled, pos + new Vector2(-2f, 3f));
                TextureRenderer.Render(instructionType.disabledTexture, Color.White, pos + new Vector2(1f, 2f));
            }
            return (false, false);
        }
        bool highlight = false;
        bool selected = false;
        if (solEditScrn.GetSimPlayState() == SimPlayState.Stopped) {
            Bounds2 bounds = Bounds2.WithSize(pos, instructionType.enabledTexture.size.ToVector2());
            if (bounds.Contains(InputManager.MousePos()) && !solEditScrn.IsPartEditMode() && isSameLayer) {
                highlight = true;
                solEditScrn.SetTooltip(instructionType.name, instructionType.description, instructionType.GetHotkey());
                if (InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
                    selected = true;
                    ClearPartSelection();
                    solEditScrn.currentInputMode = InstructionInsertInputMode.StartInputMode(instructionType, pos);
                    Assets.sounds.instruction_pickup.PlaySound(1f);
                }
            }
            if (selectedInstructionType.HasValue() && selectedInstructionType.GetValue() == instructionType && !solEditScrn.IsPartEditMode()) {
                bool quickHotkeys = GameLogic.instance.settingsData.quickHotkeys.Get();
                highlight = !quickHotkeys;
                bool flag4 = !quickHotkeys && InputManager.IsClickPressed(MouseButtonType.LeftClick);
                bool flag5 = quickHotkeys && InputManager.IsKeyPressed(instructionType.GetHotkey());
                if ((flag4 || flag5) && ProgramAreaBounds().Contains(InputManager.MousePos())) {
                    Maybe<InstructionReference> instructionAt = GetInstructionAt(InputManager.MousePos());
                    if (instructionAt.HasValue() && !instructionAt.GetValue().part.GetIsFixed()) {
                        selected = true;
                        ClearPartSelection();
                        instructionAt.GetValue().part.program.SetInstruction(instructionAt.GetValue().instructionIndex, instructionType);
                        Assets.sounds.instruction_place.PlaySound(1f);
                    }
                }
            }
        }
        if (isSameLayer) {
            TextureRenderer.Render(Assets.textures.solution_editor.program_panel.instruction, pos);
            TextureRenderer.Render(instructionType.enabledTexture, Color.White, pos + new Vector2(1f, 2f));
            if (highlight) {
                TextureRenderer.Render(Assets.textures.solution_editor.program_panel.instruction_highlight, pos + new Vector2(2f, 5f));
            }
        }
        return (highlight, selected);
    }

    [MonoModILInject("RenderFrame")]
    static void PatchRenderInstructionSelectors(MethodDefinition method, CustomAttribute attribute) {
        ILCursor cursor = new(new ILContext(method));
        MethodDefinition render = method.DeclaringType.Resolve().Methods.First(f => f.Name.Equals("RenderInstructionSelectors"));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchCallvirt("SolutionEditorProgramPanel", "RenderInstructionInTray"));
        Instruction start = cursor.Next;
        cursor.Index = cursor.Instrs.Count;
        cursor.GotoPrev(MoveType.After, instr => instr.MatchCallvirt("SolutionEditorProgramPanel", "RenderInstructionInTray"));
        int endIndex = cursor.Index;
        cursor.Goto(start, MoveType.Before);
        cursor.RemoveRange(endIndex - cursor.Index);
        cursor.EmitCall(render);

        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0());
        start = cursor.Next;
        cursor.GotoNext(MoveType.After, instr => instr.MatchNewobj(out var _));
        endIndex = cursor.Index;
        cursor.Goto(start, MoveType.Before);
        cursor.RemoveRange(endIndex - cursor.Index);
        cursor.EmitLdarg0();
    }

    private int currentPage;
    private void RenderInstructionSelectors(Maybe<InstructionType> selectedInstructionType) {
        bool highlightLeft = false;
        bool highlightRight = false;
        int newPage = currentPage;
        for (int i = 0; i < QApi.InstructionTrays.Count; i++) {
            bool wasSelected = false;
            foreach (var item in QApi.InstructionTrays[i].Tray) {
                var returned = RenderInstructionInTray(item.Value, GetInstructionPosition((int)item.Key), selectedInstructionType, currentPage == i);
                wasSelected = wasSelected || returned.Item2;
                if (returned.Item1 && i < currentPage) highlightLeft = true;
                if (returned.Item1 && i > currentPage) highlightRight = true;
            }
            if (wasSelected) newPage = i;
        }
        currentPage = newPage;

        int nextAllowed = GetNextAllowedPage();
        if ( currentPage != 0 || nextAllowed >= 0) {
            Vector2 posLeft = new(285f, 225f);
            Vector2 posRight = new(285f, 186f);
            Texture boxLeft = currentPage != 0 ? InstructionsBox : InstructionsBoxDisabled;
            Texture boxRight = currentPage < QApi.InstructionTrays.Count - 1 ? InstructionsBox : InstructionsBoxDisabled;
            TextureRenderer.Render(boxLeft, posLeft - (boxLeft.size.ToVector2() / 2f).Rounded());
            TextureRenderer.Render(currentPage != 0 ? InstructionsLeft : InstructionsLeftDisabled, posLeft - (InstructionsLeft.size.ToVector2() / 2f).Rounded());
            TextureRenderer.Render(boxRight, posRight - (boxRight.size.ToVector2() / 2f).Rounded());
            TextureRenderer.Render(currentPage < QApi.InstructionTrays.Count - 1 ? InstructionsRight : InstructionsRightDisabled, posRight - (InstructionsRight.size.ToVector2() / 2f).Rounded());
            if (!solEditScrn.IsPartEditMode()) {
                Bounds2 boundsLeft = Bounds2.WithSize(posLeft - (boxLeft.size.ToVector2() / 2f).Rounded(), boxLeft.size.ToVector2());
                Bounds2 boundsRight = Bounds2.WithSize(posRight - (boxRight.size.ToVector2() / 2f).Rounded(), boxRight.size.ToVector2());
                if (boundsLeft.Contains(InputManager.MousePos()) && currentPage != 0) {
                    TextureRenderer.Render(InstructionsBoxHighlight, posLeft - (InstructionsBoxHighlight.size.ToVector2() / 2f).Rounded());
                    if (InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
                        currentPage = GetPrevAllowedPage();
                        Assets.sounds.click_button.PlaySound(1f);
                    }
                } else if (highlightLeft)
                    TextureRenderer.Render(InstructionsBoxHighlight, posLeft - (InstructionsBoxHighlight.size.ToVector2() / 2f).Rounded());
                if (boundsRight.Contains(InputManager.MousePos()) && nextAllowed >= 0) {
                    TextureRenderer.Render(InstructionsBoxHighlight, posRight - (InstructionsBoxHighlight.size.ToVector2() / 2f).Rounded());
                    if (InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
                        currentPage = nextAllowed;
                        Assets.sounds.click_button.PlaySound(1f);
                    }
                } else if (highlightRight)
                    TextureRenderer.Render(InstructionsBoxHighlight, posRight - (InstructionsBoxHighlight.size.ToVector2() / 2f).Rounded());
            }
        }
    }
    private static readonly Texture InstructionsBox = AssetLoaderHelper.LoadTexture("textures/quintessential/instructions_arrow_box");
    private static readonly Texture InstructionsBoxDisabled = AssetLoaderHelper.LoadTexture("textures/quintessential/instructions_arrow_box_disabled");
    private static readonly Texture InstructionsBoxHighlight = AssetLoaderHelper.LoadTexture("textures/quintessential/instructions_arrow_highlight");
    private static readonly Texture InstructionsLeft = AssetLoaderHelper.LoadTexture("textures/quintessential/instructions_go_left");
    private static readonly Texture InstructionsRight = AssetLoaderHelper.LoadTexture("textures/quintessential/instructions_go_right");
    private static readonly Texture InstructionsLeftDisabled = AssetLoaderHelper.LoadTexture("textures/quintessential/instructions_go_left_disabled");
    private static readonly Texture InstructionsRightDisabled = AssetLoaderHelper.LoadTexture("textures/quintessential/instructions_go_right_disabled");
    private static readonly float[] colPositions = [46f, 90f, 135f, 179f, 224f];
    private static readonly float[] rowPositions = [199f, 146f, 92f];
    private static Vector2 GetInstructionPosition(int i) {
        if (i < 0 || i > 14) throw new Exception("Index was outside the allowed range of 0 - 14");
        if (i == 10) throw new Exception("Index was dissallowed value 10");
        int col = i % 5;
        int row = i / 5;
        return new Vector2(colPositions[col], rowPositions[row]);
    }

    private int GetNextAllowedPage() {
        for (int i = currentPage + 1; i < QApi.InstructionTrays.Count; i++) {
            if (QApi.InstructionTrays[i].Tray.Any(instr => instr.Value != null && solEditScrn.GetSolution().GetPuzzle().HasPermissionForInstr(instr.Value)))
                return i;
        }
        return -1;
    }
    private int GetPrevAllowedPage() {
        for (int i = currentPage - 1; i > 0; i--) {
            if (QApi.InstructionTrays[i].Tray.Any(instr => instr.Value != null && solEditScrn.GetSolution().GetPuzzle().HasPermissionForInstr(instr.Value)))
                return i;
        }
        return 0;
    }
}