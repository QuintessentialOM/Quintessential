using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Quintessential.Serialization;

public class CampaignModel {

	public string Name, Title;

	public IList<ChapterModel> Chapters { get; set; }

	[YamlIgnore]
	public string Path = "";
}

public class ChapterModel {

	public string Title, Subtitle, Place, Background;

	// TODO: wheel icons

	public IList<EntryModel> Entries;
}

public class EntryModel {

	// TODO: multiple requirements, solitaires and documents, tutorials

	public string ID, Title, Song, Fanfare, Puzzle, Requires;
	public bool Cutscene;
}
