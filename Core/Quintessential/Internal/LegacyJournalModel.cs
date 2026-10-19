using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Quintessential.Internal;

public class LegacyJournalModel {

	public string Title { get; set; }

    public string PuzzleBackgroundLarge { get; set; }
	public string PuzzleBackgroundSmall { get; set; }

	public List<JournalChapterModel> Chapters = [];

	[YamlIgnore]
	public string Path = "";

	[YamlIgnore]
	public Texture PuzzleBackgroundSmallTex, PuzzleBackgroundLargeTex;
}

public class JournalChapterModel {

	public string Title { get; set; }

    public string Description { get; set; }

    public List<string> Puzzles = [];
}
