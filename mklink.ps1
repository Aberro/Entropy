$mod_folder = Split-Path -Leaf (Get-Location)
cd ..
Remove-Item .\$mod_folder -Force -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path "$mod_folder" -Target "..\..\..\..\..\workshop\content\544550\3469850624"