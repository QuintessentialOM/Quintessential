namespace Quintessential {

	public abstract class QuintessentialMod {

		public ModMeta Meta;
		public object Settings = new object();

		public abstract void Load();

		public abstract void PostLoad();

		public abstract void Unload();

		public virtual void LoadPuzzleContent() {

		}

		public virtual void ApplySettings() {

		}
	}
}
