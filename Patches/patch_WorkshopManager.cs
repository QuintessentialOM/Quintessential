// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

internal class patch_WorkshopManager{
	
	public void method_2230(){
		// without the object cast, it's illegal
		((WorkshopManager)(object)this).method_2234();
		((WorkshopManager)(object)this).method_2235();
	}

	public void method_2233(){
		// make the Browse button a no-op rather than crashing
	}
}