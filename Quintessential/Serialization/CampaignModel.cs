using System.Collections.Generic;

namespace Quintessential.Serialization {

	public class CampaignModel {

		public string Name { get; set; }

		public string Title { get; set; }

		public IList<ChapterModel> Chapters { get; set; }

		[YamlDotNet.Serialization.YamlIgnore]
		public string Path = "";
	}

	public class ChapterModel {

		public string Title { get; set; }

		public string Subtitle { get; set; }

		public string Place { get; set; }

		public string Background { get; set; }

		// TODO: wheel icons

		public IList<EntryModel> Entries { get; set; }
	}

	public class EntryModel {

		// TODO: multiple requirements, solitaires and documents, tutorials

		public string ID { get; set; }

		public string Title { get; set; }

		public string Puzzle { get; set; }

		public string Requires { get; set; }
	}
}
