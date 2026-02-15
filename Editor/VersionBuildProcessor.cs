#nullable enable
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SeweralIdeas.ProjectVersion.Editor
{
    /// <summary>
    /// Build processor that automatically injects version from git before build
    /// and reverts it after build (even if build fails or is canceled).
    ///
    /// This processor is controlled by VersionSettings in Project Settings.
    /// By default, it's disabled for projects using this as a library.
    /// </summary>
    public class VersionBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string BACKUP_KEY = "VersionBuildProcessor_OriginalVersion";
        private const string BACKUP_BUNDLE_KEY = "VersionBuildProcessor_OriginalBundleVersion";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Check if automatic versioning is enabled
            if (!VersionSettings.IsEnabled)
            {
                return; // Silently skip if disabled
            }

            var settings = VersionSettings.instance;
            bool verboseLogging = settings.VerboseLogging;
            bool failOnError = settings.FailBuildOnError;

            if (!GitVersionProvider.IsGitRepository())
            {
                string message = "[VersionBuildProcessor] Not a git repository. Skipping version injection.";
                if (failOnError)
                {
                    throw new BuildFailedException(message);
                }
                if (verboseLogging)
                {
                    Debug.LogWarning(message);
                }
                return;
            }

            if (!GitVersionProvider.TryGetVersion(out Version version))
            {
                string message = "[VersionBuildProcessor] Could not retrieve version from git. Ensure you have at least one tag in format 'v1.2.3'.";
                if (failOnError)
                {
                    throw new BuildFailedException(message);
                }
                if (verboseLogging)
                {
                    Debug.LogWarning(message);
                }
                return;
            }

            // Backup current version settings
            EditorPrefs.SetString(BACKUP_KEY, PlayerSettings.bundleVersion);

            // For platforms that use bundle version code (Android, iOS, etc.)
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                EditorPrefs.SetString(BACKUP_BUNDLE_KEY, PlayerSettings.Android.bundleVersionCode.ToString());
            }

            // Set the version from git with hash for better traceability
            // Note: We use ToString() which includes the hash (e.g., "1.2.3+abc123")
            // This is fine - stores only use versionCode/buildNumber for comparison
            PlayerSettings.bundleVersion = version.ToString();

            // Set bundle version code from patch number (you can customize this logic)
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // Common pattern: combine all version parts into an integer
                // e.g., 1.2.3 becomes 10203
                int versionCode = version.Major * 10000 + version.Minor * 100 + version.Patch;
                PlayerSettings.Android.bundleVersionCode = versionCode;
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                // iOS buildNumber can be just the version without hash, or include it
                // Using version-only string for cleaner App Store Connect display
                PlayerSettings.iOS.buildNumber = version.ToVersionString();
            }

            if (verboseLogging)
            {
                Debug.Log($"[VersionBuildProcessor] Injected version from git: {version.ToStringWithPrefix()} into PlayerSettings");
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            RevertVersion();
        }

        private static void RevertVersion()
        {
            var settings = VersionSettings.instance;
            bool verboseLogging = settings.VerboseLogging;

            if (!EditorPrefs.HasKey(BACKUP_KEY))
            {
                return;
            }

            // Restore original version
            string originalVersion = EditorPrefs.GetString(BACKUP_KEY);
            PlayerSettings.bundleVersion = originalVersion;
            EditorPrefs.DeleteKey(BACKUP_KEY);

            // Restore bundle version code if it was backed up
            if (EditorPrefs.HasKey(BACKUP_BUNDLE_KEY))
            {
                string originalBundleVersion = EditorPrefs.GetString(BACKUP_BUNDLE_KEY);

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    if (int.TryParse(originalBundleVersion, out int bundleCode))
                    {
                        PlayerSettings.Android.bundleVersionCode = bundleCode;
                    }
                }

                EditorPrefs.DeleteKey(BACKUP_BUNDLE_KEY);
            }

            if (verboseLogging)
            {
                Debug.Log($"[VersionBuildProcessor] Reverted version to original: {originalVersion}");
            }
        }

        /// <summary>
        /// Called when Unity editor is closed or scripts are reloaded during build.
        /// This ensures version is reverted even if build is cancelled.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            // Check if we have a backup but Unity was restarted/scripts reloaded during build
            if (EditorPrefs.HasKey(BACKUP_KEY) && !BuildPipeline.isBuildingPlayer)
            {
                Debug.LogWarning("[VersionBuildProcessor] Detected interrupted build. Reverting version...");
                RevertVersion();
            }
        }
    }
}
