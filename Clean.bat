@ECHO OFF

FOR /R /D %%i in (obj\*) DO @RMDIR /S /Q %%i
FOR /R %%i in (obj\*) DO @DEL /Q %%i

FOR /R /D %%i in (IVV.VENUS.*\bin\*) DO @RMDIR /S /Q %%i
FOR /R %%i in (IVV.VENUS.*\bin\*) DO @DEL /Q %%i

@ECHO ON