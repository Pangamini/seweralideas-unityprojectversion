#nullable enable
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.ProjectVersion.Editor
{
    /// <summary>
    /// Menu items for quick access to version utilities
    /// </summary>
    public static class VersionMenuItems
    {
        [MenuItem("Tools/ProjectVersion/Open Settings", priority = 1)]
        private static void OpenSettings()
        {
            SettingsService.OpenProjectSettings("Project/SeweralIdeas.ProjectVersion");
        }

        [MenuItem("Tools/ProjectVersion/Show Current Version", priority = 2)]
        private static void ShowCurrentVersion()
        {
            if (!GitVersionProvider.IsGitRepository())
            {
                EditorUtility.DisplayDialog("Version Info", "Not a git repository", "OK");
                return;
            }

            if (GitVersionProvider.TryGetVersion(out var version))
            {
                string message = $"Git Version: {version.ToStringWithPrefix()}\n" +
                                $"Major: {version.Major}\n" +
                                $"Minor: {version.Minor}\n" +
                                $"Patch: {version.Patch}\n" +
                                $"Revision Hash: {version.RevisionHash}\n\n" +
                                $"PlayerSettings.bundleVersion: {PlayerSettings.bundleVersion}";

                EditorUtility.DisplayDialog("Version Info", message, "OK");
                Debug.Log($"[Version] {version.ToStringWithPrefix()}");
            }
            else
            {
                EditorUtility.DisplayDialog("Version Info", "Could not retrieve version from git.\nEnsure you have at least one tag in format 'v1.2.3'", "OK");
            }
        }

        [MenuItem("Tools/ProjectVersion/Copy Version to PlayerSettings", priority = 3)]
        private static void CopyVersionToPlayerSettings()
        {
            if (!GitVersionProvider.IsGitRepository())
            {
                EditorUtility.DisplayDialog("Error", "Not a git repository", "OK");
                return;
            }

            if (GitVersionProvider.TryGetVersion(out var version))
            {
                // Use full version string with hash for better traceability
                PlayerSettings.bundleVersion = version.ToString();

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    int versionCode = version.Major * 10000 + version.Minor * 100 + version.Patch;
                    PlayerSettings.Android.bundleVersionCode = versionCode;
                }
                else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                {
                    PlayerSettings.iOS.buildNumber = version.ToVersionString();
                }

                Debug.Log($"[Version] Updated PlayerSettings to {version.ToStringWithPrefix()}");
                EditorUtility.DisplayDialog("Success", $"Updated PlayerSettings to {version.ToStringWithPrefix()}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Could not retrieve version from git.\nEnsure you have at least one tag in format 'v1.2.3'", "OK");
            }
        }

        [MenuItem("Tools/ProjectVersion/Run Tests", priority = 20)]
        private static void RunTests()
        {
            // Open the Test Runner window filtered to Version tests
            var testRunnerWindow = EditorWindow.GetWindow<UnityEditor.TestTools.TestRunner.TestRunnerWindow>();
            testRunnerWindow.Show();

            EditorUtility.DisplayDialog(
                "Version Tests",
                "Opening Test Runner window.\n\n" +
                "Look for 'SeweralIdeas.ProjectVersion.Tests' and run the VersionTests fixture.\n\n" +
                "Alternatively, use:\n" +
                "Window → General → Test Runner",
                "OK");
        }
    }
}
