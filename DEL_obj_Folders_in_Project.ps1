$rootPath = $PSScriptRoot  # Gets the script's execution directory

Write-Host "This script will delete all 'obj' folders inside: $rootPath"
$confirmation = Read-Host "Are you sure you want to proceed? (yes/no)"

if ($confirmation -eq "yes") {
    # Get all directories named exactly "obj" (case-sensitive)
    $foldersToDelete = Get-ChildItem -Path $rootPath -Directory -Recurse | Where-Object { $_.Name -ceq "obj" }

    if ($foldersToDelete.Count -eq 0) {
        Write-Host "No 'obj' folders found."
        exit
    }

    Write-Host "The following 'obj' folders will be deleted:"
    foreach ($folder in $foldersToDelete) {
        $relativePath = $folder.FullName.Replace($rootPath, "").Trim("\")  # Compute relative path
        Write-Host "- $relativePath ($($folder.FullName))"
    }

    # Final confirmation
    $finalConfirmation = Read-Host "Do you confirm deleting all these folders? (yes/no)"

    if ($finalConfirmation -eq "yes") {
        foreach ($folder in $foldersToDelete) {
            Remove-Item -Path $folder.FullName -Recurse -Force
            Write-Host "Deleted: $($folder.FullName)"
        }
        Write-Host "Deletion complete."
    } else {
        Write-Host "Operation canceled."
    }
} else {
    Write-Host "Operation canceled."
}