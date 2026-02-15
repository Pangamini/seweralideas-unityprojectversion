#nullable enable
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SeweralIdeas.ProjectVersion
{
    /// <summary>
    /// Runtime component that can display the current application version.
    /// The version is read from Application.version which is set from PlayerSettings.bundleVersion.
    /// </summary>
    public class VersionReader : MonoBehaviour
    {
        [FormerlySerializedAs("updateOnStart")]
        [Header("Version Display")]
        [Tooltip("Automatically update the version text on Start")]
        [SerializeField] private bool _updateOnStart = true;

        [SerializeField]
        private UnityEvent<string> _onUpdate = new();
        
        protected void Start()
        {
            if (_updateOnStart)
                UpdateVersionText();
        }

        /// <summary>
        /// Updates the version text components with the current application version
        /// </summary>
        public void UpdateVersionText()
        {
            string versionText = GetVersionText();
            _onUpdate.Invoke(versionText);
        }

        /// <summary>
        /// Gets the formatted version text
        /// </summary>
        private string GetVersionText()
        {
            string version = Application.version;
            return version;
        }
    }
}
