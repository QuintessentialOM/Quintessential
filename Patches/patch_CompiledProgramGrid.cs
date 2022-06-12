#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_CompiledProgramGrid
{
	public extern int orig_method_853(int param_4510);
	public int method_853(int param_4510)
	{
		int n = orig_method_853(param_4510);
		return (n == 0) ? 1 : n;
	}
}