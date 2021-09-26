class patch_WorkshopManager {

	public void method_2230() {
		// without the object cast, it's illegal
		((WorkshopManager)(object)this).method_2234();
		((WorkshopManager)(object)this).method_2235();
	}
}