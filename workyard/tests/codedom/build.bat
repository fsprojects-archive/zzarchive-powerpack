@if "%_echo%"=="" echo off

setlocal
if EXIST build.ok DEL /f /q build.ok

call %~d0%~p0..\..\..\config.bat

REM NOTE that this test does not call FSC.
REM PEVERIFY not needed


REM build C# projects
del /s /q cdtsuite.exe > NUL 2>&1
del /s /q test.out > NUL 2>&1
del /s /q test.err > NUL 2>&1

REM ==
REM == Invoke resgen to create .cs files out of .resx
REM ==
%RESGEN% tests\Properties\Resources.resx    /str:cs,tests.Properties,Resources,tests\Properties\Resources.Designer.cs
%RESGEN% CodeDomTest\Properties\Resources.resx /str:cs,CodeDomTest.Properties,Resources,CodeDomTest\Properties\Resources.Designer.cs

%MSBUILDTOOLSPATH%\msbuild.exe CodeDomTest\CodeDOM.TestCore.csproj
%MSBUILDTOOLSPATH%\msbuild.exe tests\CodeDOM.Tests.csproj
%MSBUILDTOOLSPATH%\msbuild.exe CodeDOM.TestSuite.csproj

REM Run the tests

if not exist bin\Debug\CdtSuite.exe goto Error

bin\Debug\CdtSuite.exe /testcaselib:bin\Debug\tests.dll /codedomproviderlib:%FSCBinPath%\fsharp.compiler.codedom.dll /codedomprovider:Microsoft.FSharp.Compiler.CodeDom.FSharpCodeProvider > test.out
REM Do Not test ERRORLEVEL here, as the failures might be known.

if not exist test.out goto Error
type test.out

type test.out | find /C "TEST FAILED" > test.err
for /f %%c IN (test.err) do (if NOT "%%c"=="0" (
   echo Error: CodeDom TEST FAILURES DETECTED IN TESTS PREVIOUSLY KNOWN TO PASS!
   type test.out | find "TEST FAILED"
   set NonexistentErrorLevel 2> nul
   goto Error)
)

echo Ran fsharp CodeDom tests OK
:Ok
echo. > build.ok
endlocal
exit /b 0

:Error
call %SCRIPT_ROOT%\ChompErr.bat %ERRORLEVEL% %~f0
endlocal
exit /b %ERRORLEVEL%