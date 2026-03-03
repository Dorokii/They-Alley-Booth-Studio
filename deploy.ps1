param(
  [string]$Message = "chore: update site"
)

$ErrorActionPreference = "Stop"

function Run-Git {
  param([string[]]$GitArgs)
  & git @GitArgs
  if ($LASTEXITCODE -ne 0) {
    throw "git $($GitArgs -join ' ') failed."
  }
}

Run-Git -GitArgs @("rev-parse", "--is-inside-work-tree") | Out-Null

# Stage and commit if there are changes.
Run-Git -GitArgs @("add", "-A")
$status = git status --porcelain
if ($status) {
  Run-Git -GitArgs @("commit", "-m", $Message)
}
else {
  Write-Host "No local changes to commit."
}

Run-Git -GitArgs @("push", "public", "main")

$privateUrl = git remote get-url private 2>$null
if ($LASTEXITCODE -eq 0 -and $privateUrl) {
  try {
    Run-Git -GitArgs @("push", "private", "main")
  }
  catch {
    Write-Warning "Push to 'private' failed. Create/access the private repo first: $privateUrl"
  }
}
else {
  Write-Warning "Remote 'private' is not configured."
}

Write-Host "Deploy flow complete."
