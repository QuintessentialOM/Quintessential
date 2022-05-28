using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Quintessential.Serialization;

public class JournalModel {

	public string Title { get; set; }

	public List<JournalChapterModel> Chapters = new();

	[YamlIgnore]
	public string Path = "";
}

public class JournalChapterModel {

	public string Title { get; set; }

	public string Description { get; set; }

	public List<string> Puzzles = new();
}
