using MonoMod;
using System;

public class patch_SolutionEditorScreen {

    [MonoModReplace]
    public void method_2463(Action param_8705) {
        Vector2 screenSize = InputManager.screenSize;
        Index2 index = (InputManager.screenSize * ((SolutionEditorScreen)(object)this).boardScale).FlooredToInt();
        var class_ = RenderTargetStack.stack.Peek();
        // § Added:
        TextureRenderer.CompleteRender();
        RenderTargetStack.stack.Push(new RenderTargetStackEntry(class_.canvas, class_.size, class_.projectionMatrix, class_.viewMatrix
            * Matrix4.GetScale(new Vector3(1f / ((SolutionEditorScreen)(object)this).boardScale, 1f / ((SolutionEditorScreen)(object)this).boardScale, 1f))));
        // § Changed:
        Renderer.SetPerCameraConstants(class_.projectionMatrix, RenderTargetStack.stack.Peek().viewMatrix);
        // # ---------
        InputManager.screenSize = index.ToVector2();
        param_8705();
        TextureRenderer.CompleteRender();
        // § Added:
        RenderTargetStack.stack.Pop();
        // # ---------
        Renderer.SetPerCameraConstants(class_.projectionMatrix, class_.viewMatrix);
        InputManager.screenSize = screenSize;
    }
}
