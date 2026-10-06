using MonoMod;
using System;

public class patch_SolutionEditorScreen {

    [MonoModReplace]
    public void method_2463(Action param_8705) {
        Vector2 screenSize = InputManager.screenSize;
        Index2 index = (InputManager.screenSize * ((SolutionEditorScreen)(object)this).field_4755).FlooredToInt();
        class_193 class_ = class_268.field_2097.Peek();
        // § Added:
        class_268.field_2097.Push(new class_193(class_.field_1787, class_.field_1788, class_.field_1789, class_.field_1790
            * Matrix4.GetScale(new Vector3(1f / ((SolutionEditorScreen)(object)this).field_4755, 1f / ((SolutionEditorScreen)(object)this).field_4755, 1f))));
        // § Changed:
        Renderer.SetPerCameraConstants(class_.field_1789, class_268.field_2097.Peek().field_1790);
        // # ---------
        InputManager.screenSize = index.ToVector2();
        param_8705();
        TextureRenderer.CompleteRender();
        // § Added:
        class_268.field_2097.Pop();
        // # ---------
        Renderer.SetPerCameraConstants(class_.field_1789, class_.field_1790);
        InputManager.screenSize = screenSize;
    }
}
