@echo off
rem Run rename-project.ps1 from cmd.exe, e.g.:  rename-project TestKube   or   rename-project TestKube -All
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0rename-project.ps1" %*
