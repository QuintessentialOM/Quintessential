using Quintessential.Serialization;
using System;
using System.Text;
using System.Text.Json.Serialization;

namespace Quintessential;

[JsonConverter(typeof(VersionRangeJsonConverter))]
public class VersionRange {
    public bool InclusiveMin;
    public Version VersionMin;
    public bool InclusiveMax;
    public Version VersionMax;

    public bool Contains(Version version) =>
        (VersionMin == null || VersionMin < version || (InclusiveMin && VersionMin == version)) &&
        (VersionMax == null || VersionMax > version || (InclusiveMax && VersionMax == version));

    public static VersionRange Parse(string str) {
        VersionRange toReturn = new();
        string[] versionRange = str.Split(',');
        if (versionRange.Length == 1) {
            toReturn.InclusiveMin = true;
            toReturn.InclusiveMax = false;
            toReturn.VersionMax = null;
            try {
                toReturn.VersionMin = versionRange[0] == "" ? null : Version.Parse(versionRange[0]);
            } catch (Exception e) { throw new Exception("Faliled to parse minimum version in range", e); }
            return toReturn;
        }
        if (versionRange.Length == 2) {

            char minInclusive = versionRange[0][0];
            if (minInclusive == '[') toReturn.InclusiveMin = true;
            else if (minInclusive == '(') toReturn.InclusiveMin = false;
            else throw new Exception("Version range must begin with '[' for Min-Inclusive or '(' for Min-Exclusive.");
            versionRange[0] = versionRange[0][1..];

            char maxInclusive = versionRange[1][^1];
            if (maxInclusive == ']') toReturn.InclusiveMax = true;
            else if (maxInclusive == ')') toReturn.InclusiveMax = false;
            else throw new Exception("Version range must end with ']' for Max-Inclusive or ')' for Max-Exclusive.");
            versionRange[1] = versionRange[1][..^1];

            try {
                toReturn.VersionMin = versionRange[0] == "" ? null : Version.Parse(versionRange[0]);
            } catch (Exception e) { throw new Exception("Faliled to parse minimum version in range", e); }
            try {
                toReturn.VersionMax = versionRange[1] == "" ? null : Version.Parse(versionRange[1]);
            } catch (Exception e) { throw new Exception("Faliled to parse maximum version in range", e); }

            return toReturn;
        }
        throw new Exception("Invalid number of ',' characters in the VersionRange, multiple range sets are not supported.");
    }
    public override string ToString() {
        StringBuilder builder = new();
        builder.Append(InclusiveMin ? '[' : '(');
        if (VersionMin != null) builder.Append(VersionMin.ToString());
        builder.Append(',');
        if (VersionMax != null) builder.Append(VersionMax.ToString());
        builder.Append(InclusiveMax ? ']' : ')');
        return builder.ToString();
    }
}
