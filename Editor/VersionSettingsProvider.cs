#nullable enable
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SeweralIdeas.ProjectVersion.Editor
{
    /// <summary>
    /// Provides a UI in Unity's Project Settings for configuring automatic version.
    /// </summary>
    public class VersionSettingsProvider : SettingsProvider
    {
        private const string SettingsPath = "Project/SeweralIdeas.ProjectVersion";
        private SerializedObject? _serializedSettings;

        // Cached git information to avoid running git commands every frame
        private bool _isGitRepository;
        private bool _hasGitVersion;
        private Version _cachedVersion;
        private string _errorMessage = string.Empty;
        private bool _cacheInitialized;

        public VersionSettingsProvider(string path, SettingsScope scopes)
            : base(path, scopes)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // ScriptableSingleton automatically creates the instance
            _serializedSettings = new SerializedObject(VersionSettings.instance);

            // Fetch git information once when settings window is opened
            RefreshGitInformation();
        }

        public override void OnGUI(string searchContext)
        {
            if (_serializedSettings == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Automatic Version Injection", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Automatically inject version from git tags into PlayerSettings during builds.\n" +
                "Requires git tags in format 'v1.2.3'.\n\n" +
                "The version will be automatically reverted after the build completes, fails, or is cancelled.",
                MessageType.Info);

            EditorGUILayout.Space();

            _serializedSettings.Update();

            EditorGUI.BeginChangeCheck();

            // Enable toggle
            var enableProp = _serializedSettings.FindProperty("_enableAutomaticProjectVersion");
            EditorGUILayout.PropertyField(enableProp, new GUIContent("Enable Automatic ProjectVersion"));

            // Additional settings (only show if enabled)
            if (enableProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(
                    _serializedSettings.FindProperty("_verboseLogging"),
                    new GUIContent("Verbose Logging", "Log version injection and reversion to console"));

                EditorGUILayout.PropertyField(
                    _serializedSettings.FindProperty("_failBuildOnError"),
                    new GUIContent("Fail Build On Error", "Fail the build if version cannot be retrieved from git"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                _serializedSettings.ApplyModifiedProperties();
                VersionSettings.instance.Save();
            }

            EditorGUILayout.Space();

            // Header with refresh button
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Current Git Version", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                {
                    RefreshGitInformation();
                }
            }

            // Show cached git version (doesn't run git commands every frame)
            if (!_cacheInitialized)
            {
                EditorGUILayout.HelpBox("Loading git information...", MessageType.Info);
            }
            else if (!_isGitRepository)
            {
                EditorGUILayout.HelpBox("Not a git repository", MessageType.Warning);
            }
            else if (!_hasGitVersion)
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrEmpty(_errorMessage)
                        ? "Could not retrieve version from git. Ensure you have at least one tag in format 'v1.2.3'"
                        : _errorMessage,
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("Latest Tag:", _cachedVersion.ToStringWithPrefix());
                EditorGUILayout.LabelField("Commit Hash:", _cachedVersion.RevisionHash ?? "N/A");
                EditorGUILayout.LabelField("Full Version:", _cachedVersion.ToString());
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current PlayerSettings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Bundle Version:", PlayerSettings.bundleVersion);

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                EditorGUILayout.LabelField("Bundle Version Code:", PlayerSettings.Android.bundleVersionCode.ToString());
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                EditorGUILayout.LabelField("Build Number:", PlayerSettings.iOS.buildNumber);
            }
        }

        /// <summary>
        /// Refreshes git information by executing git commands.
        /// Called once on activate and when user clicks refresh button.
        /// </summary>
        private void RefreshGitInformation()
        {
            _cacheInitialized = false;
            _errorMessage = string.Empty;

            try
            {
                _isGitRepository = GitVersionProvider.IsGitRepository();

                if (_isGitRepository)
                {
                    _hasGitVersion = GitVersionProvider.TryGetVersion(out _cachedVersion);
                }
                else
                {
                    _hasGitVersion = false;
                    _cachedVersion = default;
                }
            }
            catch (System.Exception ex)
            {
                _hasGitVersion = false;
                _cachedVersion = default;
                _errorMessage = $"Error retrieving git information: {ex.Message}";
                Debug.LogWarning($"[VersionSettings] {_errorMessage}");
            }
            finally
            {
                _cacheInitialized = true;
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateVersionSettingsProvider()
        {
            var provider = new VersionSettingsProvider(SettingsPath, SettingsScope.Project)
            {
                keywords = new[] { "version", "git", "build", "automatic", "tag" }
            };
            return provider;
        }
    }
}
