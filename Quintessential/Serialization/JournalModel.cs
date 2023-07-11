using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Quintessential.Serialization;

using Texture = class_256;

public class JournalModel {

	public string Title { get; set; }
	
	public string PuzzleBackgroundLarge { get; set; }
	public string PuzzleBackgroundSmall { get; set; }

	public List<JournalChapterModel> Chapters = new();

	[YamlIgnore]
	public string Path = "";

	[YamlIgnore]
	public Texture PuzzleBackgroundSmallTex, PuzzleBackgroundLargeTex;
}

public class JournalChapterModel {

	public string Title { get; set; }

	public string Description { get; set; }

	public List<string> Puzzles = new();
}
