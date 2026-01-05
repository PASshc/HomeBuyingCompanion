## Version 7.4.3

### ✨ Improvements

- **Microsoft Store Compliance**: Added full silent install/uninstall support for Microsoft Store publishing requirements
  - Supports `/SILENT`, `/VERYSILENT`, and `/SUPPRESSMSGBOXES` command-line switches
  - Auto-closes running application during updates

---

## Version 7.4.2

### 🐛 Bug Fixes

- **Fixed Backup Freeze**: Resolved issue where the application would freeze when creating a backup. Backup now runs asynchronously with a wait cursor indicator.

---

## Version 7.4.1

### 🐛 Bug Fixes

- **Fixed Duplicate Attachments**: Resolved issue where journal attachments were duplicated when saving an entry
- **Improved Markdown Processing**: Enhanced real-time Markdown formatting reliability in journal content editor

---

## Version 7.4.0

### 🎉 New Features

- **Markdown Formatting in Journal**: Write journal entries using familiar Markdown syntax
  - Type `**text**` for **bold** formatting
  - Type `*text*` for *italic* formatting
  - Type `~~text~~` for ~~strikethrough~~ formatting
  - Type `` `text` `` for `code` formatting
  - Formatting is applied automatically when you press Space or Enter
  - Helpful syntax hint displayed above the content editor

### 🐛 Bug Fixes

- **Fixed Journal Content Not Saving**: Resolved issue where text entered in journal content field wasn't being stored
- **Fixed Journal Attachments**: Attachments now save and open correctly (was using wrong storage folder in dev mode)
- **Fixed Property Images**: Images now save to correct folder in development mode
- **Fixed Save Error**: Resolved "Object reference not set" error when saving existing journal entries

---

## Version 7.3.0

### 🔧 Improvements

- **Smart Category Detection**: Existing tags are now automatically assigned categories based on keywords
  - Tags containing "Kitchen", "Island", "Cabinet" → Kitchen category
  - Tags containing "Yard", "Backyard", "Pool", "Landscape" → Yard category
  - Tags containing "View", "Street", "Lake", "Location" → Location category
  - Tags containing "Flooring", "Tile", "Carpet", "Hardwood" → Flooring category
  - And many more keyword-to-category mappings
  - Works automatically on app startup - no manual updates needed

---

## Version 7.2.0

### 🎉 New Features

- **Category Prefixes for Tags**: Organize PROs and CONs with category prefixes for better sorting
  - Select from predefined categories: Kitchen, Bathroom, Bedroom, Roof, HVAC, Location, and more
  - Tags display as "Category: Name" format (e.g., "Kitchen: Remodeled", "Roof: New")
  - Tags automatically sort alphabetically by category, then by name
  - Category is optional - legacy tags without categories continue to work
  - Helps keep tags organized when comparing multiple properties

---

## Version 7.1.0

### 🎉 New Features

- **Journal Attachments**: Attach documents directly to journal entries
  - Perfect for storing pre-approval letters, loan documents, and receipts
  - Add unlimited attachments per journal entry (PDFs, images, documents)
  - View attachment count (📎) indicator in the entry list
  - Click to open attachments in default application
  - Add descriptions to each attachment for easy reference

### 🐛 Bug Fixes

- **Fixed PROs/CONs Tags**: Resolved issue where tags weren't displaying or saving after recent refactoring
  - Tag dropdowns now populate correctly
  - Creating new tags works as expected
  - All existing tags are preserved

---

## Version 7.0.0

### 🎉 New Features

- **Property Tags Refactoring**: Improved internal architecture for better maintainability
- Performance improvements and code cleanup

---

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
