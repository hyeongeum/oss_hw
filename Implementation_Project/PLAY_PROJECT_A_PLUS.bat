@echo off
setlocal
set "GAME=%~dp0outputs\ProjectAPlus_Windows\Project A+.exe"

if not exist "%GAME%" (
  echo Project A+ build was not found.
  echo Open this folder in Unity 2022.3.62f3 and run:
  echo Tools ^> Project A+ ^> Build Windows Game
  pause
  exit /b 1
)

start "" "%GAME%"
endlocal
