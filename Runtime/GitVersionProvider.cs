#nullable enable
using System;
using System.Diagnostics;

namespace SeweralIdeas.ProjectVersion
{
    /// <summary>
    /// Provides version information from git repository.
    /// This utility can be used both in runtime and editor contexts.
    /// </summary>
    public static class GitVersionProvider
    {
        /// <summary>
        /// Gets the latest git tag on the current branch in the format "v1.2.3"
        /// </summary>
        public static string GetLatestTag()
        {
            return ExecuteGitCommand("describe --tags --abbrev=0");
        }

        /// <summary>
        /// Gets the current git commit hash (short format)
        /// </summary>
        public static string GetCommitHash(bool shortFormat = true)
        {
            string args = shortFormat ? "rev-parse --short HEAD" : "rev-parse HEAD";
            return ExecuteGitCommand(args);
        }

        /// <summary>
        /// Gets the full version from git including tag and commit hash
        /// Format: "v1.2.3" or "v1.2.3-5-g1a2b3c4" (if commits ahead of tag)
        /// </summary>
        public static string GetDescribe()
        {
            return ExecuteGitCommand("describe --tags --always --long");
        }

        /// <summary>
        /// Extracts version from git tag and adds current commit hash
        /// Returns a Version struct with major.minor.patch and optional revision hash
        /// </summary>
        public static Version GetVersion()
        {
            try
            {
                string tag = GetLatestTag();
                string hash = GetCommitHash(shortFormat: true);

                // Parse the tag (e.g., "v1.2.3")
                Version version = Version.Parse(tag);

                // Add the commit hash as revision
                version.RevisionHash = hash;

                return version;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to get version from git. Ensure you're in a git repository with at least one tag in format 'v1.2.3'",
                    ex);
            }
        }

        /// <summary>
        /// Tries to get version from git. Returns false if git is not available or no tags exist.
        /// </summary>
        public static bool TryGetVersion(out Version version)
        {
            try
            {
                version = GetVersion();
                return true;
            }
            catch
            {
                version = default;
                return false;
            }
        }

        /// <summary>
        /// Checks if the working directory is a git repository
        /// </summary>
        public static bool IsGitRepository()
        {
            try
            {
                ExecuteGitCommand("rev-parse --git-dir");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes a git command and returns the output
        /// </summary>
        private static string ExecuteGitCommand(string arguments)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = UnityEngine.Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "")
                };

                using Process process = Process.Start(processInfo)!;
                string output = process.StandardOutput.ReadToEnd().Trim();
                string error = process.StandardError.ReadToEnd().Trim();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Git command failed: {error}");
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute git command: {arguments}", ex);
            }
        }
    }
}
