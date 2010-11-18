@if "%_echo%"=="" echo off
setlocal
if EXIST build.ok DEL /f /q build.ok

call %~d0%~p0..\..\..\config.bat

del /s /q namespaces.html > NUL 2>&1
del /s /q FSharp.Core > NUL 2>&1
del /s /q Test.PowerPack > NUL 2>&1
del /s /q Test.PowerPack.Linq > NUL 2>&1
del /s /q dummy > NUL 2>&1

REM devenv /debugexe 
REM Check the generation of basic team docs
fshtmldoc.exe --locallinks %FSCBinPath%\FSharp.Core.dll %FSCBinPath%\Test.PowerPack.dll %FSCBinPath%\Test.PowerPack.Linq.dll %FSCBinPath%\Test.PowerPack.Metadata.dll
if ERRORLEVEL 1 goto Error

REM Check all the command line options
mkdir dummy
fshtmldoc.exe --outdir dummy --cssfile dummy.css --namespacefile nsp.html %FSCBinPath%\Test.PowerPack.Metadata.dll
if ERRORLEVEL 1 goto Error

REM devenv /debugexe 
REM Check the generation of docs against a local DLL
fsc.exe -a test.fs
if ERRORLEVEL 1 goto Error

fshtmldoc.exe test.dll
if ERRORLEVEL 1 goto Error


:Ok
echo Built fsharp %~f0 ok.
echo. > build.ok
endlocal
exit /b 0

:Skip
echo Skipped %~f0
endlocal
exit /b 0


:Error
call %SCRIPT_ROOT%\ChompErr.bat %ERRORLEVEL% %~f0
endlocal
exit /b %ERRORLEVEL%

