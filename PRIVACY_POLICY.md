# Privacy Policy - Home Buying Companion

**Effective Date:** December 27, 2025  
**Version:** 1.0  
**Application Version:** 5.2.0

---

## Overview

Home Buying Companion is a **privacy-first desktop application** designed to help you track and manage your home buying journey. We are deeply committed to protecting your privacy and ensuring you maintain complete control over your personal data.

---

## Core Privacy Principles

### 🔒 **Your Data Stays on Your Device**
All data is stored **exclusively on your local computer**. We do not transmit, upload, or sync any of your information to external servers, cloud services, or third parties.

### 🚫 **No Online Accounts Required**
The application does not require registration, login, or any form of user account. There are no user credentials, passwords, or authentication mechanisms.

### 🙈 **No Tracking or Analytics**
We do not collect, track, or analyze your usage patterns. There is no telemetry, crash reporting, or behavioral analytics of any kind.

### 💾 **Complete Data Ownership**
You own all data created within the application. You can export, backup, delete, or migrate your data at any time without restriction.

---

## Data Collection and Storage

### What Data is Stored Locally

The application stores the following information **exclusively on your computer**:

#### Property Information
- Property addresses, cities, states, zip codes
- Listing details (MLS numbers, URLs, prices)
- Property features and characteristics
- Your personal notes, comments, and ratings
- Property status and tracking flags
- Viewing appointments and important dates

#### Financial Data
- Mortgage calculation parameters (purchase price, down payment, interest rate)
- Calculated payment scenarios and amortization schedules
- Property tax estimates and HOA fees

#### Media Files
- Property images (up to 4 per property)
- File attachments (documents, PDFs, etc.)

#### Organizational Data
- Custom tags (PROs/CONs)
- Quick notes and status markers
- Archive settings

### Where Data is Stored

All application data is stored locally in:

- **Database Location:**  
  `%LocalAppData%\HomeBuyingApp\homebuying.db`  
  (e.g., `C:\Users\YourName\AppData\Local\HomeBuyingApp\homebuying.db`)

- **Images Folder:**  
  `%LocalAppData%\HomeBuyingApp\Images\`

- **Attachments Folder:**  
  `%LocalAppData%\HomeBuyingApp\Attachments\`

**Important:** In development mode, data is stored in `HomeBuyingApp_Dev` to isolate test data from production.

---

## Data Access and Sharing

### Who Has Access to Your Data

- **Only You:** Your data is accessible only on your local computer, protected by your Windows user account and system security.
- **Not Us:** We (the developers) have **zero access** to any data you create, store, or manage within the application.
- **No Third Parties:** The application does not communicate with any external services, APIs, or third-party providers.

### Data Transmission

The application **NEVER**:
- Sends your property data, notes, or personal information over the internet
- Uploads data to cloud servers
- Syncs across devices
- Shares data with third parties
- Reports usage statistics or crash data

### Limited Internet Access

The application makes **one optional internet connection**:

**Version Check (Auto-Update):**
- On application launch, checks GitHub API for the latest version
- Only sends: One HTTP GET request to `https://api.github.com/repos/PASshc/HomeBuyingCompanion/releases/latest`
- No user data, identifiers, or usage information is transmitted
- Only receives: Version number and release information
- Can be blocked by firewall without affecting app functionality
- Failure to check for updates does not impact application use

**Manual Links:**
- Clicking GitHub links in the User Guide opens your default browser
- These links are clearly marked and require your explicit action

---

## Data Security

### Local Protection

Your data security relies on:

1. **Windows Security:** Standard Windows file system permissions protect your data folder
2. **User Account Control:** Only users logged into your Windows account can access the data
3. **Physical Security:** Protecting your computer with passwords, encryption (BitLocker), and physical access controls

### Backup and Recovery

- **Backups Are Your Responsibility:** We provide built-in backup/restore features, but you are responsible for creating and securing backups
- **Backup Security:** Backup files (`.zip` format) contain your complete database and all attachments. Store them securely
- **No Cloud Backups:** The application does not automatically back up to cloud services

### Recommendations

We recommend:
- Using Windows BitLocker or similar disk encryption
- Regularly backing up your data using the built-in backup feature
- Storing backups in secure locations (encrypted external drives, secure cloud storage of your choice)
- Using strong Windows account passwords

---

## Your Data Rights and Control

### Full Control

You have complete control over your data:

- **Access:** View all your data at any time within the application
- **Export:** Use CSV export to extract property data in a portable format
- **Backup:** Create complete backups including database, images, and attachments
- **Modify:** Edit or delete any property, note, tag, or attachment
- **Migrate:** Copy your data folder to another computer or reinstall the application
- **Delete:** Permanently delete individual properties or the entire database

### Data Deletion

To completely remove all data:

1. **Uninstall Application:** Use Windows "Add or Remove Programs"
2. **Delete Data Folder:** Manually delete:
   - `%LocalAppData%\HomeBuyingApp\`
   - Any backup files you created
3. **Verify:** Check that no residual files remain

---

## Third-Party Services

### No Third-Party Integration

The application **does not integrate** with:
- Cloud storage services
- Analytics platforms
- Advertising networks
- Social media platforms
- Data brokers
- Marketing services

### External Links

The application includes clickable links to:
- **GitHub Repository:** https://github.com/PASshc/HomeBuyingCompanion (documentation)
- **Listing Websites:** User-entered URLs (Zillow, Redfin, etc.)

These links open in your default web browser. Standard web privacy policies apply when visiting external sites.

---

## Open Source Transparency

### Verify Our Claims

Home Buying Companion is **open source software**:

- **Source Code:** Publicly available at https://github.com/PASshc/HomeBuyingCompanion
- **Transparency:** Anyone can review the code to verify our privacy claims
- **Community Review:** The open-source community can audit for security or privacy issues
- **No Hidden Features:** All functionality is visible in the source code

---

## Children's Privacy

The application is designed for adults engaged in home buying. We do not knowingly collect or target information from individuals under 18 years of age. As all data is stored locally and not transmitted, no personally identifiable information is collected by us.

---

## Data Retention

- **Indefinite Local Storage:** Data remains on your computer until you manually delete it
- **No Automatic Deletion:** The application does not automatically delete or expire data
- **Archive Feature:** Properties can be archived (hidden) without deletion
- **Your Choice:** You control when and how to delete data

---

## Changes to This Privacy Policy

We may update this privacy policy to reflect:
- Application feature changes
- Legal requirements
- Best practice updates

**Notification:** Updated policies will be included with new application releases and posted on our GitHub repository. The effective date and version number will be updated accordingly.

---

## Contact and Support

### Questions or Concerns

For privacy-related questions:

- **GitHub Issues:** https://github.com/PASshc/HomeBuyingCompanion/issues
- **Documentation:** Review the README.md and ARCHITECTURE.md files in the repository

### Security Issues

If you discover a security vulnerability:
1. **DO NOT** publicly disclose the issue
2. Report it privately via GitHub Security Advisories
3. We will respond promptly and work to address the concern

---

## Legal Information

### Jurisdiction

This application is provided "as-is" without warranty. As it operates entirely locally on your device, standard software license terms apply (MIT License).

### GDPR Compliance

While the application is not subject to GDPR (no data processing by us), it is designed to support GDPR principles:
- **Data Minimization:** Only collects data you explicitly enter
- **Right to Access:** Full access to all your data
- **Right to Erasure:** Complete deletion control
- **Data Portability:** CSV export and backup features
- **Privacy by Design:** Local-only architecture ensures privacy

---

## Summary

**In Plain English:**

✅ **All your data stays on your computer**  
✅ **We never see, access, or collect your information**  
✅ **No internet connection required (offline capable)**  
✅ **No accounts, no tracking, no ads**  
✅ **You own and control everything**  
✅ **Open source for transparency**

This is privacy-first software built for people who value data ownership and control.

---

**Last Updated:** December 27, 2025  
**GitHub Repository:** https://github.com/PASshc/HomeBuyingCompanion  

---

*This privacy policy applies to Home Buying Companion version 5.2.0 and subsequent versions unless superseded by an updated policy.*
