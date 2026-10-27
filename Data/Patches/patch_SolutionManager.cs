using MonoMod;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class patch_SolutionManager {
    [MonoModIgnore]
    public static extern List<Solution> orig_LoadAllSolutions();

    public static List<Solution> LoadAllSolutions() {
        var orig = orig_LoadAllSolutions();

        string[] ExtraFileExtensions = {
            ".json",
            ".jsonc"
        };
        foreach (var extension in ExtraFileExtensions) {
            IEnumerable<string> enumerable = Directory.EnumerateFiles(PlatformBridge.savePath, "*" + Solution.fileExtension + extension).OrderBy(path => File.GetCreationTimeUtc(path));
            foreach (string text in enumerable) {
                Maybe<Solution> solutionAt = Solution.GetSolutionAt(text);
                if (solutionAt.HasValue()) {
                    orig.Add(solutionAt.GetValue());
                }
            }
        }
        return [.. orig.OrderBy(solution => solution.creationTime)];
    }
}
