#nullable enable
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.ProjectVersion.Editor
{
    /// <summary>
    /// Settings for automatic version injection during builds.
    /// Uses ScriptableSingleton pattern for automatic serialization and persistence.
    /// </summary>
    [FilePath("ProjectSettings/ProjectVersionSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class VersionSettings : ScriptableSingleton<VersionSettings>
    {
        [SerializeField]
        [Tooltip("Enable automatic version injection from git during builds")]
        private bool _enableAutomaticProjectVersion = false; // Safe default for new projects

        [SerializeField]
        [Tooltip("Log version changes to console")]
        private bool _verboseLogging = true;

        [SerializeField]
        [Tooltip("Fail build if git version cannot be retrieved")]
        private bool _failBuildOnError = false;

        /// <summary>
        /// Enable automatic version injection from git during builds
        /// </summary>
        public bool EnableAutomaticprojectVersion
        {
            get => _enableAutomaticProjectVersion;
            set
            {
                _enableAutomaticProjectVersion = value;
                Save();
            }
        }

        /// <summary>
        /// Log version changes to console
        /// </summary>
        public bool VerboseLogging
        {
            get => _verboseLogging;
            set
            {
                _verboseLogging = value;
                Save();
            }
        }

        /// <summary>
        /// Fail build if git version cannot be retrieved
        /// </summary>
        public bool FailBuildOnError
        {
            get => _failBuildOnError;
            set
            {
                _failBuildOnError = value;
                Save();
            }
        }

        /// <summary>
        /// Checks if automatic versioning is enabled in project settings
        /// </summary>
        public static bool IsEnabled => instance.EnableAutomaticprojectVersion;

        /// <summary>
        /// Saves the settings to disk
        /// </summary>
        public void Save()
        {
            Save(true);
        }
    }
}
