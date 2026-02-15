#nullable enable
using NUnit.Framework;

namespace SeweralIdeas.ProjectVersion.Tests
{
    /// <summary>
    /// Test fixture for Version struct validation
    /// </summary>
    [TestFixture]
    public class VersionTests
    {
        #region Parse Tests

        [Test]
        public void Parse_ValidVersionWithoutPrefix_ReturnsCorrectVersion()
        {
            var version = Version.Parse("1.2.3");

            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.IsNull(version.RevisionHash);
        }

        [Test]
        public void Parse_ValidVersionWithPrefix_ReturnsCorrectVersion()
        {
            var version = Version.Parse("v1.2.3");

            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.IsNull(version.RevisionHash);
        }

        [Test]
        public void Parse_ValidVersionWithHash_ReturnsCorrectVersion()
        {
            var version = Version.Parse("1.2.3+abc123");

            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.AreEqual("abc123", version.RevisionHash);
        }

        [Test]
        public void Parse_ValidVersionWithPrefixAndHash_ReturnsCorrectVersion()
        {
            var version = Version.Parse("v1.2.3+abc123");

            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.AreEqual("abc123", version.RevisionHash);
        }

        [Test]
        public void Parse_LargeVersionNumbers_ReturnsCorrectVersion()
        {
            var version = Version.Parse("v10.20.30+g1a2b3c4");

            Assert.AreEqual(10, version.Major);
            Assert.AreEqual(20, version.Minor);
            Assert.AreEqual(30, version.Patch);
            Assert.AreEqual("g1a2b3c4", version.RevisionHash);
        }

        [Test]
        public void Parse_ZeroVersion_ReturnsCorrectVersion()
        {
            var version = Version.Parse("0.0.0");

            Assert.AreEqual(0, version.Major);
            Assert.AreEqual(0, version.Minor);
            Assert.AreEqual(0, version.Patch);
        }

        [Test]
        public void Parse_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<System.ArgumentException>(() => Version.Parse(""));
        }

        [Test]
        public void Parse_NullString_ThrowsArgumentException()
        {
            Assert.Throws<System.ArgumentException>(() => Version.Parse(null!));
        }

        [Test]
        public void Parse_InvalidFormat_ThrowsFormatException()
        {
            Assert.Throws<System.FormatException>(() => Version.Parse("1.2"));
            Assert.Throws<System.FormatException>(() => Version.Parse("1"));
            Assert.Throws<System.FormatException>(() => Version.Parse("invalid"));
            Assert.Throws<System.FormatException>(() => Version.Parse("release-1.2.3"));
        }

        [Test]
        public void Parse_NonNumericVersion_ThrowsFormatException()
        {
            Assert.Throws<System.FormatException>(() => Version.Parse("a.b.c"));
        }

        #endregion

        #region TryParse Tests

        [Test]
        public void TryParse_ValidVersion_ReturnsTrue()
        {
            bool result = Version.TryParse("v1.2.3", out var version);

            Assert.IsTrue(result);
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
        }

        [Test]
        public void TryParse_InvalidVersion_ReturnsFalse()
        {
            bool result = Version.TryParse("invalid", out var version);

            Assert.IsFalse(result);
            Assert.AreEqual(default(Version), version);
        }

        [Test]
        public void TryParse_EmptyString_ReturnsFalse()
        {
            bool result = Version.TryParse("", out var version);

            Assert.IsFalse(result);
        }

        #endregion

        #region ToString Tests

        [Test]
        public void ToString_VersionWithoutHash_ReturnsCorrectFormat()
        {
            var version = new Version(1, 2, 3);
            Assert.AreEqual("1.2.3", version.ToString());
        }

        [Test]
        public void ToString_VersionWithHash_ReturnsCorrectFormat()
        {
            var version = new Version(1, 2, 3, "abc123");
            Assert.AreEqual("1.2.3+abc123", version.ToString());
        }

        [Test]
        public void ToString_ZeroVersion_ReturnsCorrectFormat()
        {
            var version = new Version(0, 0, 0);
            Assert.AreEqual("0.0.0", version.ToString());
        }

        [Test]
        public void ToStringWithPrefix_VersionWithoutHash_ReturnsCorrectFormat()
        {
            var version = new Version(1, 2, 3);
            Assert.AreEqual("v1.2.3", version.ToStringWithPrefix());
        }

        [Test]
        public void ToStringWithPrefix_VersionWithHash_ReturnsCorrectFormat()
        {
            var version = new Version(1, 2, 3, "abc123");
            Assert.AreEqual("v1.2.3+abc123", version.ToStringWithPrefix());
        }

        #endregion

        #region Bidirectional Parse/ToString Tests

        [TestCase("1.2.3")]
        [TestCase("v1.2.3")]
        [TestCase("0.1.0")]
        [TestCase("10.20.30")]
        [TestCase("1.2.3+abc123")]
        [TestCase("v1.2.3+abc123")]
        [TestCase("2.0.0+g1a2b3c4")]
        [TestCase("0.0.0")]
        [TestCase("999.999.999+hash")]
        public void ParseAndToString_AreReversible(string input)
        {
            // Parse -> ToString -> Parse should return the same version
            var parsed = Version.Parse(input);
            var serialized = parsed.ToString();
            var reparsed = Version.Parse(serialized);

            Assert.AreEqual(parsed, reparsed, $"Reversibility failed for input '{input}'");
            Assert.AreEqual(parsed.Major, reparsed.Major);
            Assert.AreEqual(parsed.Minor, reparsed.Minor);
            Assert.AreEqual(parsed.Patch, reparsed.Patch);
            Assert.AreEqual(parsed.RevisionHash, reparsed.RevisionHash);
        }

        [Test]
        public void ToStringThenParse_PreservesAllFields()
        {
            var original = new Version(5, 10, 15, "githash123");
            var serialized = original.ToString();
            var deserialized = Version.Parse(serialized);

            Assert.AreEqual(original, deserialized);
            Assert.AreEqual(original.Major, deserialized.Major);
            Assert.AreEqual(original.Minor, deserialized.Minor);
            Assert.AreEqual(original.Patch, deserialized.Patch);
            Assert.AreEqual(original.RevisionHash, deserialized.RevisionHash);
        }

        #endregion

        #region Equality Tests

        [Test]
        public void Equals_SameVersions_ReturnsTrue()
        {
            var v1 = new Version(1, 2, 3, "abc");
            var v2 = new Version(1, 2, 3, "abc");

            Assert.AreEqual(v1, v2);
            Assert.IsTrue(v1.Equals(v2));
            Assert.IsTrue(v1 == v2);
            Assert.IsFalse(v1 != v2);
        }

        [Test]
        public void Equals_DifferentMajor_ReturnsFalse()
        {
            var v1 = new Version(1, 2, 3);
            var v2 = new Version(2, 2, 3);

            Assert.AreNotEqual(v1, v2);
            Assert.IsFalse(v1 == v2);
            Assert.IsTrue(v1 != v2);
        }

        [Test]
        public void Equals_DifferentMinor_ReturnsFalse()
        {
            var v1 = new Version(1, 2, 3);
            var v2 = new Version(1, 3, 3);

            Assert.AreNotEqual(v1, v2);
        }

        [Test]
        public void Equals_DifferentPatch_ReturnsFalse()
        {
            var v1 = new Version(1, 2, 3);
            var v2 = new Version(1, 2, 4);

            Assert.AreNotEqual(v1, v2);
        }

        [Test]
        public void Equals_DifferentHash_ReturnsFalse()
        {
            var v1 = new Version(1, 2, 3, "abc");
            var v2 = new Version(1, 2, 3, "def");

            Assert.AreNotEqual(v1, v2);
        }

        [Test]
        public void Equals_OneHashNullOtherNot_ReturnsFalse()
        {
            var v1 = new Version(1, 2, 3, null);
            var v2 = new Version(1, 2, 3, "abc");

            Assert.AreNotEqual(v1, v2);
        }

        [Test]
        public void Equals_BothHashNull_ReturnsTrue()
        {
            var v1 = new Version(1, 2, 3, null);
            var v2 = new Version(1, 2, 3, null);

            Assert.AreEqual(v1, v2);
        }

        #endregion

        #region HashCode Tests

        [Test]
        public void GetHashCode_SameVersions_ReturnsSameHashCode()
        {
            var v1 = new Version(1, 2, 3, "abc");
            var v2 = new Version(1, 2, 3, "abc");

            Assert.AreEqual(v1.GetHashCode(), v2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentVersions_ReturnsDifferentHashCode()
        {
            var v1 = new Version(1, 2, 3);
            var v2 = new Version(1, 2, 4);

            Assert.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void Constructor_WithAllParameters_SetsCorrectly()
        {
            var version = new Version(1, 2, 3, "abc123");

            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.AreEqual("abc123", version.RevisionHash);
        }

        [Test]
        public void Constructor_WithoutHash_SetsHashToNull()
        {
            var version = new Version(1, 2, 3);

            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.IsNull(version.RevisionHash);
        }

        #endregion
    }
}
