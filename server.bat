@echo off

start cmd /c "mode con: cols=100 lines=30 & MemoryStore\\bin\\Release\\net5.0\\MemoryStore.exe"
start cmd /c "mode con: cols=100 lines=30 & ServiceLocator\\bin\\Release\\net5.0\\ServiceLocator.exe"
