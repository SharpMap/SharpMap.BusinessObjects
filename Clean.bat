@ECHO OFF

FOR /R /D %%i in (obj\*) DO @RMDIR /S /Q %%i
FOR /R %%i in (obj\*) DO @DEL /Q %%i

FOR /R /D %%i in (bin\*) DO @RMDIR /S /Q %%i
FOR /R %%i in (bin\*) DO @DEL /Q %%i

@ECHO ON
