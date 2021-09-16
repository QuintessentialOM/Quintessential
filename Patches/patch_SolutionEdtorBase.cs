using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
abstract class patch_SolutionEditorBase : SolutionEditorBase {

	// renders parts
	// adds support for custom part renderers

	public extern void orig_method_1996(Part part, Vector2 pos);
	public void method_1996(Part part, Vector2 pos) {
		orig_method_1996(part, pos);
		class_236 class195 = method_1989(part, pos);
		class_195 renderer = new class_195(class195.field_1984, class195.field_1985, Editor.method_922());
		foreach(var r in QApi.PartRenderers)
			if(r.Left(part))
				r.Right(part, pos, this, renderer);
	}
}