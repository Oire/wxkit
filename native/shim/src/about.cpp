#include "shim.h"

#include <wx/aboutdlg.h>

namespace {

void AddPeople(const wxk_StringArray* people, void (wxAboutDialogInfo::*add)(const wxString&), wxAboutDialogInfo& info) {
	if (people == nullptr) return;
	for (const wxString& person : *wxk::Unwrap(const_cast<wxk_StringArray*>(people))) (info.*add)(person);
}

}

WXK_FUNCTION(void) wxk_AboutBox_Show(const wxk_AboutInfo* about, wxk_Window* parent) noexcept {
	// The strings are taken first, so they are freed whatever wxWidgets does.
	const bool hasName = about->name != nullptr;
	const bool hasVersion = about->version != nullptr;
	const bool hasDescription = about->description != nullptr;
	const bool hasCopyright = about->copyright != nullptr;
	const bool hasLicense = about->license != nullptr;
	const bool hasWebSite = about->webSite != nullptr;
	const wxString name = wxk::TakeString(about->name);
	const wxString version = wxk::TakeString(about->version);
	const wxString longVersion = wxk::TakeString(about->longVersion);
	const wxString description = wxk::TakeString(about->description);
	const wxString copyright = wxk::TakeString(about->copyright);
	const wxString license = wxk::TakeString(about->license);
	const wxString webSite = wxk::TakeString(about->webSite);
	const wxString webSiteDescription = wxk::TakeString(about->webSiteDescription);
	wxk::Guard([&] {
		wxAboutDialogInfo info;
		if (hasName) info.SetName(name);
		if (hasVersion) info.SetVersion(version, longVersion);
		if (hasDescription) info.SetDescription(description);
		if (hasCopyright) info.SetCopyright(copyright);
		if (hasLicense) info.SetLicence(license);
		if (hasWebSite) info.SetWebSite(webSite, webSiteDescription);
		AddPeople(about->developers, &wxAboutDialogInfo::AddDeveloper, info);
		AddPeople(about->docWriters, &wxAboutDialogInfo::AddDocWriter, info);
		AddPeople(about->artists, &wxAboutDialogInfo::AddArtist, info);
		AddPeople(about->translators, &wxAboutDialogInfo::AddTranslator, info);
		if (about->icon != nullptr) info.SetIcon(*static_cast<const wxIcon*>(wxk::Unwrap(const_cast<wxk_Object*>(about->icon))));
		wxAboutBox(info, wxk::Unwrap(parent));
	});
}
