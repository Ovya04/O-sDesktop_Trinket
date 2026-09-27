; Trinket installer script (Inno Setup)
; AppId is a fixed GUID identifying this application across versions -
; never change it once released, since Windows uses it to recognize
; upgrades/uninstalls of the same app.
#define MyAppId "{{8F2C1A4E-3B7D-4E9A-9C5F-1D2E3F4A5B6C}"
#define MyAppName "Trinket"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Independent"
#define MyAppExeName "Trinket.exe"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=output
OutputBaseFilename=TrinketSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; No admin rights required - installs per-user under the user's own
; Program Files-equivalent folder ({autopf} resolves appropriately).
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
; Pulls in everything from the published output folder - the single
; self-contained Trinket.exe plus any small supporting files alongside it.
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Offers to launch the app immediately after a successful install.
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName} now"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Intentionally does NOT delete %LOCALAPPDATA%\Trinket - that's the user's
; own saved settings, Trinket presets, and imported charm images. Removing
; user data on uninstall would be surprising and destructive; if the user
; genuinely wants a clean slate, they can delete that folder themselves.