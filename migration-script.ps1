# Script de migration automatique des controllers
# Ce script migre les patterns try/catch vers le middleware global

param(
    [string]$ControllerPath
)

Write-Host "Migration du controller: $ControllerPath"

# Lire le contenu du fichier
$content = Get-Content $ControllerPath -Raw

# Remplacer les types de réponse d'erreur
$content = $content -replace 'typeof\(object\)', 'typeof(ApiErrorResponse)'

# Supprimer les try blocks
$content = $content -replace '\s*try\s*\{\s*\n', "`n"

# Supprimer les catch blocks pour InvalidOperationException
$content = $content -replace '\s*\}\s*catch\s*\(InvalidOperationException\s+ex\)\s*\{\s*return\s+NotFound\(new\s*\{\s*message\s*=\s*ex\.Message\s*\}\);\s*\}', ''

# Supprimer les catch blocks pour Exception  
$content = $content -replace '\s*\}\s*catch\s*\(Exception\s+ex\)\s*\{\s*return\s+BadRequest\(new\s*\{\s*message\s*=\s*ex\.Message\s*\}\);\s*\}', ''

# Supprimer les catch blocks restants
$content = $content -replace '\s*catch\s*\([^)]+\)\s*\{[^}]*\}', ''

# Sauvegarder
Set-Content $ControllerPath $content -NoNewline

Write-Host "Migration terminée pour: $ControllerPath"
