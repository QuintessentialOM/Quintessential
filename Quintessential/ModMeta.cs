using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Quintessential;

public class ModMeta {

	public string Name { get; set; }

	public string Title { get; set; }

	public string Icon { get; set; }

	public string Desc { get; set; }

	public string DLL { get; set; }

	public string Mappings { get; set; }

	public Dependency[] Dependencies { get; set; } = new Dependency[0];

	public Dependency[] OptionalDependencies { get; set; } = new Dependency[0];

	[YamlIgnore]
	public string PathArchive { get; set; }

	[YamlIgnore]
	public string PathDirectory { get; set; }

	[YamlIgnore]
	public Version Version { get; set; } = new Version(1, 0);
	private string _VersionString;

	[YamlMember(Alias = "Version")]
	public string VersionString {
		get {
			return _VersionString;
		}
		set {
			_VersionString = value;
			int versionSplitIndex = value.IndexOf('-');
			if(versionSplitIndex == -1)
				Version = new Version(value);
			else
				Version = new Version(value.Substring(0, versionSplitIndex));
		}
	}

	public override string ToString() {
		return Name + " " + Version;
	}

	public void PostParse() {
		if(!string.IsNullOrEmpty(DLL) && !string.IsNullOrEmpty(PathDirectory) && !File.Exists(DLL))
			DLL = Path.Combine(PathDirectory, DLL.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
	}

	public class Dependency {

		public string Name { get; set; }

		[YamlIgnore]
		public Version Version { get; set; } = new Version(1, 0);
		private string _VersionString;

		[YamlMember(Alias = "Version")]
		public string VersionString {
			get {
				return _VersionString;
			}
			set {
				_VersionString = value;
				int versionSplitIndex = value.IndexOf('-');
				if(versionSplitIndex == -1)
					Version = new Version(value);
				else
					Version = new Version(value.Substring(0, versionSplitIndex));
			}
		}
	}
}
