setlocal
if EXIST build.ok DEL /f /q build.ok

call %~d0%~p0..\..\config.bat
@if ERRORLEVEL 1 goto Error

if NOT "%FSC:NOTAVAIL=X%" == "%FSC%" ( 
  REM Skipping test for FSI.EXE
  goto Skip
)


REM UNICODE test1-unicode

REM Regression test for FSB 1885
"%FSLEX%" repro1885.fsl
@if ERRORLEVEL 1 goto Error

"%FSLEX%" --light-off -o test1lex.fs test1lex.mll
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --light-off --module TestParser -o test1.fs test1.mly
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% -g -o:test1%ILX_SUFFIX%.exe tree.ml test1.fsi test1.fs test1lex.fs main.ml
@if ERRORLEVEL 1 goto Error

"%PEVERIFY%" test1%ILX_SUFFIX%.exe 
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --light-off --module TestParser -o test2.fs test2.mly
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% -g -o:test2%ILX_SUFFIX%.exe tree.ml test2.fsi test2.fs test1lex.fs main.ml
@if ERRORLEVEL 1 goto Error

"%PEVERIFY%" test2%ILX_SUFFIX%.exe
@if ERRORLEVEL 1 goto Error

"%FSLEX%" --light-off --unicode -o test1-unicode-lex.fs test1-unicode-lex.mll
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --light-off --module TestParser -o test1-unicode.fs test1-unicode.mly
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% -g -o:test1-unicode%ILX_SUFFIX%.exe tree.ml test1-unicode.fsi test1-unicode.fs test1-unicode-lex.fs main-unicode.ml
@if ERRORLEVEL 1 goto Error

"%PEVERIFY%" test1-unicode%ILX_SUFFIX%.exe 
@if ERRORLEVEL 1 goto Error



"%FSLEX%" -o test1lex.ml test1lex.mll
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test1.ml test1.mly
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% -g -o:test1%ILX_SUFFIX%.exe tree.ml test1.mli test1.ml test1lex.ml main.ml
@if ERRORLEVEL 1 goto Error

"%PEVERIFY%" test1%ILX_SUFFIX%.exe 
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test1compat.ml --ml-compatibility test1.mly
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% -g -o:test1compat%ILX_SUFFIX%.exe tree.ml test1compat.mli test1compat.ml test1lex.ml main.ml
@if ERRORLEVEL 1 goto Error

"%PEVERIFY%" test1compat%ILX_SUFFIX%.exe
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test2.ml test2.mly
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% -g -o:test2%ILX_SUFFIX%.exe tree.ml test2.mli test2.ml test1lex.ml main.ml
@if ERRORLEVEL 1 goto Error

"%PEVERIFY%" test2%ILX_SUFFIX%.exe
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test2compat.ml --ml-compatibility test2.mly
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% -g -o:test2compat%ILX_SUFFIX%.exe tree.ml test2compat.mli test2compat.ml test1lex.ml main.ml
@if ERRORLEVEL 1 goto Error

"%PEVERIFY%" test2compat%ILX_SUFFIX%.exe
@if ERRORLEVEL 1 goto Error

"%FSLEX%" --unicode -o test1-unicode-lex.ml test1-unicode-lex.mll
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test1-unicode.ml test1-unicode.mly
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% -g -o:test1-unicode%ILX_SUFFIX%.exe tree.ml test1-unicode.mli test1-unicode.ml test1-unicode-lex.ml main-unicode.ml
@if ERRORLEVEL 1 goto Error

"%PEVERIFY%" test1-unicode%ILX_SUFFIX%.exe 
@if ERRORLEVEL 1 goto Error



:Ok
echo Passed fsharp %~f0 ok.
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
