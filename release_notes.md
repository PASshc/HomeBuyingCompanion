## Version 6.0.0

### 🎉 New Features

- **Other Features as Chips**: Other Features field now uses tag-based chips (teal colored) instead of single-line text
  - Add multiple feature tags with the Add button
  - Remove individual tags with the ✕ button
  - Existing text automatically migrates to chip format

- **Click-to-Enlarge Images**: Property images can now be clicked to view in a larger popup window
  - Click any property image thumbnail to open enlarged view
  - Press Escape or click to close the popup

- **Rich Text Comments**: Comments section now includes a full formatting toolbar
  - **Bold** (B), *Italic* (I), Underline (U)
  - Bullet lists and numbered lists
  - Yellow and green text highlighting
  - Clear formatting button

### 🔧 Improvements

- Enhanced property detail editing experience
- Better visual organization of property features

---

## Version 5.7.0

### 🎉 New Features

- **Auto-Status on Archive**: Archiving a property now automatically sets its status to "Not Interested"
- This streamlines workflow when ruling out properties - just archive and the status updates automatically

---

## Version 5.6.0

### 🎉 New Features

- **Street # Filter**: New filter in the property list to search by street number (leading digits of address)
- **Enhanced Filtering**: Filter row now includes Street #, City, and State filters for quick property searching

### 🔧 Improvements

- Reorganized filter row layout with cleaner labels
- Clear Filters button now resets all three filter fields

---

## 🎉 New Features (v5.5.0)

- **Menu Bar**: Added File and Help menus for better navigation
- **Auto-Update System**: Automatically checks for updates on launch
- **Update Notifications**: Non-intrusive banner when new version is available
- **Manual Update Check**: Help → Check for Updates option
- **User Guide Tab**: Comprehensive in-app documentation with all features explained
- **About Dialog**: Version information and GitHub links
- **Menu Integration**: Backup/Restore database accessible from File menu

## 📚 Documentation

- **Privacy Policy**: Added PRIVACY_POLICY.md with full transparency about data handling
- **Auto-Update Disclosure**: Privacy policy updated to disclose version check API call
- **GitHub Links**: User Guide includes links to repository and documentation

## 🔧 Technical Improvements

- Implemented UpdateService for GitHub API integration
- Non-blocking async update checks
- Version comparison logic
- Clickable hyperlinks in User Guide
- Improved MainWindow architecture with menu system

## 📥 Installation

Download and run **HomeBuyingAppSetup_v5.5.0.exe** to install or update.

## 🔒 Privacy

All your data stays local. The only internet connection is a single API call to check for updates (can be blocked by firewall without affecting functionality).

**Full Changelog**: https://github.com/PASshc/HomeBuyingCompanion/compare/v5.4.0...v5.5.0
