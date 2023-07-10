#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

public class patch_Molecule{

	private extern Molecule orig_method_1104();
	public Molecule method_1104(){
		Molecule ret = orig_method_1104();
		ret.field_2639 = ((Molecule)(object)this).field_2639;
		return ret;
	}
}