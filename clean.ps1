﻿<#
.Synopsis
   Cleans the solution: removes all bin and obj folders under src, deletes Dependencies, cache.dat and all mappings.bin files from App_Data.
   Just run the script where it is, don't move or run elsewhere.

.EXAMPLE
	PS> .\CleanSolution.ps1

#>

# Deleting bin and obj folders.
$currentPath = (Get-Item -Path ".\").FullName
# Add relative file paths here what you want to keep.
$whiteList = @()
$whiteListFolders = @()

Get-ChildItem -Path ($currentPath + "\.\") -Recurse | 
Where-Object { $PSItem.PSIsContainer -and ( $PSItem.Name -eq "bin" -or $PSItem.Name -eq "obj") } | 
ForEach-Object {
	if($whiteListFolders.Contains($PSItem.FullName.Substring($currentPath.Length)))
	{
		Get-ChildItem -Path $PSItem.FullName -Recurse -File |
		ForEach-Object {
			if(!$whiteList.Contains($PSItem.FullName.Substring($currentPath.Length)))
			{
				Remove-Item $PSItem.FullName -Force
			}
		}
	}
	else
	{
		Remove-Item $PSItem.FullName -Recurse -Force
	}
}