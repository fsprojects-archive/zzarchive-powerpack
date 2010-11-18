@if "%_echo%"=="" echo off
setlocal

if "%TOOLS%" == "" (
    set TOOLS=c:\fsharp\rc1\tools
)
if "%WIX_HOME%" == "" (
    set WIX_HOME=%TOOLS%\win86\wix-ms\3.0.4116.0\bin
)
if "%WIX_UI_PATH%" == "" (
    set WIX_UI_PATH=%TOOLS%\win86\wix\WixUI_en-us.wxl
)
if "%CYGWIN%" == "" (
    set CYGWIN=%TOOLS%\win86\cygwin
)
set CANDLE=%WIX_HOME%\candle.exe
set LIGHT=%WIX_HOME%\light.exe
set FSI_TOOL=fsi.exe
set RM=%CYGWIN%\rm.exe
set SED=%CYGWIN%\sed.exe
set CP=%CYGWIN%\cp.exe
set PKZIP=%TOOLS%\win86\cygwin\zip.exe
if "%MSIDUMP%" == "" (
    set MSIDUMP=%TOOLS%\win86\msidump\x86\msidump.exe
)
set tree=FSharpPowerPack-2.0.0.0

%FSI_TOOL% --quiet generateWixFileEntries.fsx  %tree%
pushd %tree%
%CANDLE%  -ext WixNetFxExtension ..\files.wxs ..\product.wxs
%LIGHT%  -out ..\InstallFSharpPowerPack.msi -ext WixVsExtension -ext WixNetFxExtension  -ext WixUIExtension -loc %WIX_UI_PATH% product.wixobj files.wixobj
popd

REM %CHMOD% u+rx LICENSE-fsharp.rtf
%MSIDUMP% -list -format:f InstallFSharpPowerPack.msi > InstallFSharpPowerPack.msi.files

REM # Build the zip at the same time, from the same files (note, no need to sign the zip%.
del  FSharpPowerPack.zip
%PKZIP% -@ FSharpPowerPack.zip < zip.args

@echo off
REM Process to sign the MSI
REM 
REM %MKDIR% -p %deployment_cpx_tree%\unsigned-msi
REM %RM% -rf %deployment_cpx_tree%\signed-msi
REM %CP% %tree%\InstallFSharpPowerPack.msi %deployment_cpx_tree%\unsigned-msi
REM %CHMOD% a+rwx %deployment_cpx_tree%\unsigned-msi\InstallFSharpPowerPack.msi
REM %SUBMITTER% %NOSIGN% -src %deployment_cpx_tree%\unsigned-msi -dst %deployment_cpx_tree%\signed-msi -description "Microsoft Research F# %ILX_VERSION% Installer" %APPROVER_OPTIONS%
REM %CP% %deployment_cpx_tree%\signed-msi\InstallFSharpPowerPack.msi $@
REM %MSIDUMP% -list -format:pfv InstallFSharpPowerPack.msi | sort > %tree%\InstallFSharpPowerPack.msi.files.txt
REM %DIFF% %tree%\InstallFSharpPowerPack.msi.files.txt src\wix\regression.msi.files.txt
REM %MKDIR% -p %release_config_drop%
REM %CP% %deployment_cpx_tree%\signed-msi\InstallFSharpPowerPack.msi %release_config_drop%\InstallFSharpPowerPack.msi
REM %CP% %tree%\fsharp.zip                %release_config_drop%\fsharp.zip
REM %DATE% > %release_config_drop%\timestamp.txt
REM %FCIV% %release_config_drop% > %release_config_drop%\hashes.txt

endlocal
