using MonoMod;
using System;

public class patch_SolutionEditorScreen {

    [MonoModReplace]
    public void RenderAtBoardScale(Action doRender) {
        Vector2 screenSize = InputManager.screenSize;
        Index2 index = (InputManager.screenSize * ((SolutionEditorScreen)(object)this).boardScale).FlooredToInt();
        var stackEntry = RenderTargetStack.stack.Peek();
        // § Added:
        TextureRenderer.CompleteRender();
        RenderTargetStack.stack.Push(new RenderTargetStackEntry(stackEntry.canvas, stackEntry.size, stackEntry.projectionMatrix, stackEntry.viewMatrix
            * Matrix4.GetScale(new Vector3(1f / ((SolutionEditorScreen)(object)this).boardScale, 1f / ((SolutionEditorScreen)(object)this).boardScale, 1f))));
        // § Changed:
        Renderer.SetPerCameraConstants(stackEntry.projectionMatrix, RenderTargetStack.stack.Peek().viewMatrix);
        // # ---------
        InputManager.screenSize = index.ToVector2();
        doRender();
        TextureRenderer.CompleteRender();
        // § Added:
        RenderTargetStack.stack.Pop();
        // # ---------
        Renderer.SetPerCameraConstants(stackEntry.projectionMatrix, stackEntry.viewMatrix);
        InputManager.screenSize = screenSize;
    }
}
