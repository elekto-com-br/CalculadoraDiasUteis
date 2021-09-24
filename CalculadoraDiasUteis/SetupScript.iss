; -- Example2.iss --
; Same as Example1.iss, but creates its icon in the Programs folder of the
; Start Menu instead of in a subfolder, and also creates a desktop icon.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

[Setup]
AppName=Calculadora de Dias Úteis
AppPublisher=Elekto Produtos Financeiros Ltda.
AppPublisherURL=https://elekto.com.br
AppCopyright=Copyright (C) 2021 Elekto Produtos Financeiros Ltda.
AppVersion=1.0.0.0
WizardStyle=modern
LicenseFile=bin\Release\license.txt
SetupIconFile=Assets\elekto_icon.ico
DefaultDirName={autopf}\Elekto\CalculadoraDiasUteis
; Since no icons will be created in "{group}", we don't need the wizard
; to ask for a Start Menu folder name:
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\CalculadoraDiasUteis.exe
Compression=lzma2
SolidCompression=yes
ChangesAssociations=yes
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=bin\Setups
OutputBaseFilename=CalculadoraDiasUteis1000

[Files]
Source: "bin\Release\CalculadoraDiasUteis.exe"; DestDir: "{app}"
Source: "bin\Release\CalculadoraDiasUteis.exe.config"; DestDir: "{app}"
Source: "bin\Release\CalculadoraDiasUteis.pdb"; DestDir: "{app}"
Source: "bin\Release\EPPlus.dll"; DestDir: "{app}"
Source: "bin\Release\EPPlus.xml"; DestDir: "{app}"
Source: "bin\Release\licença.html"; DestDir: "{app}"
Source: "bin\Release\license.txt"; DestDir: "{app}"
Source: "bin\Release\Calendars\0010.calendar.br-BC.txt"; DestDir: "{app}\Calendars"
Source: "bin\Release\Calendars\0020.calendar.br-SP.txt"; DestDir: "{app}\Calendars"
Source: "bin\Release\Calendars\0030.calendar.us-NYC.txt"; DestDir: "{app}\Calendars"
Source: "bin\Release\Calendars\0040.calendar.gb-LND.txt"; DestDir: "{app}\Calendars"
Source: "bin\Release\Calendars\LeiaMe.txt"; DestDir: "{app}\Calendars"

[Languages]
Name: "pt"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Icons]
Name: "{autoprograms}\Calculadora de Dias Úteis"; Filename: "{app}\CalculadoraDiasUteis.exe"
Name: "{autodesktop}\Calculadora de Dias Úteis"; Filename: "{app}\CalculadoraDiasUteis.exe"
