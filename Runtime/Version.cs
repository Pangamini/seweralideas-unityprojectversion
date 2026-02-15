#nullable enable
using System;
using System.Text.RegularExpressions;

namespace SeweralIdeas.ProjectVersion
{
    /// <summary>
    /// Represents a semantic version with optional git revision hash.
    /// Format: "major.minor.patch" or "major.minor.patch+hash"
    /// </summary>
    [Serializable]
    public struct Version : IEquatable<Version>
    {
        public int     Major;
        public int     Minor;
        public int     Patch;
        public string? RevisionHash;

        public Version(int major, int minor, int patch, string? revisionHash = null)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            RevisionHash = revisionHash;
        }

        /// <summary>
        /// Parses a version string in the format "major.minor.patch" or "major.minor.patch+hash"
        /// Also supports optional "v" prefix (e.g., "v1.2.3")
        /// </summary>
        public static Version Parse(string versionString)
        {
            if (string.IsNullOrWhiteSpace(versionString))
                throw new ArgumentException("Version string cannot be null or empty", nameof(versionString));

            // Remove optional 'v' prefix
            if (versionString.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                versionString = versionString.Substring(1);

            // Pattern: major.minor.patch optionally followed by +hash
            var pattern = @"^(\d+)\.(\d+)\.(\d+)(?:\+(.+))?$";
            var match = Regex.Match(versionString, pattern);

            if (!match.Success)
                throw new FormatException($"Invalid version format: '{versionString}'. Expected format: 'major.minor.patch' or 'major.minor.patch+hash'");

            return new Version(
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                match.Groups[4].Success ? match.Groups[4].Value : null
            );
        }

        /// <summary>
        /// Tries to parse a version string. Returns false if parsing fails.
        /// </summary>
        public static bool TryParse(string versionString, out Version version)
        {
            try
            {
                version = Parse(versionString);
                return true;
            }
            catch
            {
                version = default;
                return false;
            }
        }

        /// <summary>
        /// Converts the version to a string in the format "major.minor.patch" or "major.minor.patch+hash"
        /// This is the reverse of Parse() - Parse(ToString()) should return the same version.
        /// </summary>
        public override string ToString()
        {
            var baseVersion = $"{Major}.{Minor}.{Patch}";
            return string.IsNullOrEmpty(RevisionHash)
                ? baseVersion
                : $"{baseVersion}+{RevisionHash}";
        }

        /// <summary>
        /// Returns only the version string without build metadata (e.g., "1.2.3")
        /// Use this for PlayerSettings.bundleVersion as stores don't accept build metadata.
        /// </summary>
        public string ToVersionString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }

        /// <summary>
        /// Returns the version string with "v" prefix (e.g., "v1.2.3" or "v1.2.3+hash")
        /// </summary>
        public string ToStringWithPrefix()
        {
            return "v" + ToString();
        }

        public bool Equals(Version other)
        {
            return Major == other.Major
                && Minor == other.Minor
                && Patch == other.Patch
                && RevisionHash == other.RevisionHash;
        }

        public override bool Equals(object? obj)
        {
            return obj is Version other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Major;
                hash = hash * 31 + Minor;
                hash = hash * 31 + Patch;
                hash = hash * 31 + (RevisionHash?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static bool operator ==(Version left, Version right) => left.Equals(right);
        public static bool operator !=(Version left, Version right) => !left.Equals(right);
    }
}
