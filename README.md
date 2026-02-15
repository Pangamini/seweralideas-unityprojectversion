# Project Version - Automatic Git Versioning for Unity

A modular, loosely-coupled system for automatically injecting version information from git tags into Unity builds.

### 1. Create a Git Tag

```bash
git tag v1.2.3
```

Tags must follow the format `v{major}.{minor}.{patch}` (e.g., `v1.2.3`, `v0.1.0`, `v2.0.0`)

### 2. Enable Automatic Versioning

1. Open Unity
2. Go to **Edit → Project Settings → Versioning**
3. Check **Enable Automatic Versioning**

Or use the menu: **Tools → Versioning → Open Settings**

### 3. Build Your Project

The version from git will be automatically injected into `PlayerSettings.bundleVersion` before the build and reverted after.

## Usage

### Version Struct

```csharp
using SeweralIdeas.ProjectVersion;

// Parse from string
var version = Version.Parse("v1.2.3");
var versionWithHash = Version.Parse("1.2.3+abc123");

// Create manually
var version = new Version(1, 2, 3, "abc123");

// Convert to string (reversible)
string versionString = version.ToString(); // "1.2.3+abc123"
string withPrefix = version.ToStringWithPrefix(); // "v1.2.3+abc123"

// Parse and ToString are bidirectional
var parsed = Version.Parse("v1.2.3+abc123");
var serialized = parsed.ToString();
var reparsed = Version.Parse(serialized);
Assert(parsed == reparsed); // Always true
```

### Git Version Provider

```csharp
using SeweralIdeas.ProjectVersion;

// Check if in git repository
bool isGit = GitVersionProvider.IsGitRepository();

// Get version from git
Version version = GitVersionProvider.GetVersion();

// Safe version (returns false if git not available)
if (GitVersionProvider.TryGetVersion(out Version version))
{
    Debug.Log($"Version: {version}");
}
```

### Runtime Version Display

Attach the `VersionDisplay` component to a GameObject with TextMeshPro or UI.Text to automatically show the version in-game.

## Configuration

### Project Settings

**Edit → Project Settings → Versioning**

Options:
- **Enable Automatic Versioning**: Master toggle
- **Verbose Logging**: Log injection/reversion to console
- **Fail Build On Error**: Fail build if version cannot be retrieved

### Menu Items

**Tools → Versioning**:
- **Open Settings**: Jump to Project Settings
- **Show Current Version**: Display current git version
- **Copy Version to PlayerSettings**: Manually update PlayerSettings
- **Run Tests**: Open Unity Test Runner

## Testing

The package includes comprehensive NUnit tests covering:
- ✅ Parse tests (valid/invalid formats, edge cases)
- ✅ TryParse tests
- ✅ ToString tests
- ✅ Bidirectional reversibility tests
- ✅ Equality and hash code tests
- ✅ Constructor tests

### Running Tests

1. **Via Menu**: **Tools → Versioning → Run Tests**
2. **Via Test Runner**: **Window → General → Test Runner**
3. Select **EditMode** tab → **SeweralIdeas.ProjectVersion.Tests**

## Platform Support

### Android
- `PlayerSettings.bundleVersion` → `"1.2.3"`
- `PlayerSettings.Android.bundleVersionCode` → `10203`

### iOS
- `PlayerSettings.bundleVersion` → `"1.2.3"`
- `PlayerSettings.iOS.buildNumber` → `"1.2.3"`

### Other Platforms
- `PlayerSettings.bundleVersion` → `"1.2.3"`

## Architecture

```
Version (struct)              ← Core data structure (runtime)
    ↓ used by
GitVersionProvider (static)   ← Git integration utility
    ↓ used by
VersionBuildProcessor         ← Build hooks (editor-only)
    ↓ controlled by
VersionSettings               ← Project settings (editor-only)
    ↓ UI provided by
VersionSettingsProvider       ← Project Settings UI
```

## Modules

### Runtime
- **Version.cs** - Semantic version struct
- **GitVersionProvider.cs** - Git integration utility
- **VersionDisplay.cs** - Optional UI component

### Editor
- **VersionBuildProcessor.cs** - Build hooks (IPreprocessBuildWithReport, IPostprocessBuildWithReport)
- **VersionSettings.cs** - Project settings ScriptableObject
- **VersionSettingsProvider.cs** - Project Settings UI
- **VersionMenuItems.cs** - Editor menu items

### Tests
- **VersionTests.cs** - Comprehensive NUnit test fixture

## Requirements

- Unity 6000.0 or later (works with earlier versions, just update `package.json`)
- Git installed and available in PATH
- At least one git tag in format `v{major}.{minor}.{patch}`

## Troubleshooting

### "Could not retrieve version from git"
- Ensure you're in a git repository: `git status`
- Create at least one tag: `git tag v0.1.0`
- Verify git is in PATH

### Version not updating during build
- Check Project Settings → Versioning → Enable Automatic Versioning
- Verify git tags exist: `git tag -l`
- Enable verbose logging in settings

### Version not reverting after build
- Check console for errors
- EditorPrefs are used for state tracking
- Try manual revert via Tools → Versioning menu

## License

MIT License - See LICENSE file for details

## Contributing

Contributions welcome! Please open an issue or PR.
