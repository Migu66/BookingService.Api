# Generate RSA Key Pair for JWT RS256 Authentication
# This script generates a 2048-bit RSA key pair for secure JWT signing

Write-Host "Generating RSA Key Pair for JWT Authentication..." -ForegroundColor Green

# Generate private key
$privateKeyPath = "jwt-private.pem"
$publicKeyPath = "jwt-public.pem"

# Use OpenSSL if available, otherwise use .NET
try {
    # Try OpenSSL first (more standard)
    $opensslAvailable = Get-Command openssl -ErrorAction SilentlyContinue
    
    if ($opensslAvailable) {
        Write-Host "Using OpenSSL to generate keys..." -ForegroundColor Yellow
        
        # Generate 2048-bit RSA private key
        openssl genrsa -out $privateKeyPath 2048
        
        # Extract public key from private key
        openssl rsa -in $privateKeyPath -pubout -out $publicKeyPath
        
        Write-Host "`nKeys generated successfully!" -ForegroundColor Green
    }
    else {
        Write-Host "OpenSSL not found. Using .NET to generate keys..." -ForegroundColor Yellow
        
        # Use .NET RSA
        $rsa = [System.Security.Cryptography.RSA]::Create(2048)
        
        # Export private key in PEM format
        $privateKeyBytes = $rsa.ExportRSAPrivateKey()
        $privateKeyBase64 = [Convert]::ToBase64String($privateKeyBytes)
        $privateKeyPem = "-----BEGIN RSA PRIVATE KEY-----`n"
        for ($i = 0; $i -lt $privateKeyBase64.Length; $i += 64) {
            $length = [Math]::Min(64, $privateKeyBase64.Length - $i)
            $privateKeyPem += $privateKeyBase64.Substring($i, $length) + "`n"
        }
        $privateKeyPem += "-----END RSA PRIVATE KEY-----"
        
        # Export public key in PEM format
        $publicKeyBytes = $rsa.ExportSubjectPublicKeyInfo()
        $publicKeyBase64 = [Convert]::ToBase64String($publicKeyBytes)
        $publicKeyPem = "-----BEGIN PUBLIC KEY-----`n"
        for ($i = 0; $i -lt $publicKeyBase64.Length; $i += 64) {
            $length = [Math]::Min(64, $publicKeyBase64.Length - $i)
            $publicKeyPem += $publicKeyBase64.Substring($i, $length) + "`n"
        }
        $publicKeyPem += "-----END PUBLIC KEY-----"
        
        # Save to files
        $privateKeyPem | Out-File -FilePath $privateKeyPath -Encoding ascii -NoNewline
        $publicKeyPem | Out-File -FilePath $publicKeyPath -Encoding ascii -NoNewline
        
        Write-Host "`nKeys generated successfully!" -ForegroundColor Green
    }
    
    Write-Host "`nPrivate Key saved to: $privateKeyPath" -ForegroundColor Cyan
    Write-Host "Public Key saved to: $publicKeyPath" -ForegroundColor Cyan
    
    Write-Host "`n" -NoNewline
    Write-Host "IMPORTANT SECURITY NOTES:" -ForegroundColor Red
    Write-Host "1. NEVER commit the private key to source control!" -ForegroundColor Yellow
    Write-Host "2. Store the private key securely (Azure Key Vault, AWS Secrets Manager, etc.)" -ForegroundColor Yellow
    Write-Host "3. The public key can be shared and used for token validation" -ForegroundColor Yellow
    
    Write-Host "`nTo add to appsettings.json or User Secrets:" -ForegroundColor Green
    Write-Host "1. Replace newlines with \n in the PEM content" -ForegroundColor White
    Write-Host "2. Or use environment variables/secrets manager" -ForegroundColor White
    
    # Show how to format for JSON
    Write-Host "`nExample for User Secrets (use 'dotnet user-secrets set'):" -ForegroundColor Green
    $privateKeyContent = Get-Content $privateKeyPath -Raw
    $privateKeyEscaped = $privateKeyContent -replace "`r`n", "\n" -replace "`n", "\n"
    Write-Host "dotnet user-secrets set `"JwtSettings:PrivateKey`" `"$privateKeyEscaped`"" -ForegroundColor Gray
}
catch {
    Write-Host "Error generating keys: $_" -ForegroundColor Red
    exit 1
}
