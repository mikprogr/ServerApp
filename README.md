# Apps
The repository contains ClientApp and ServerApp applications.

Przed uruchomieniem aplikacji należy wdrożyć certyfikat X.509.
Wdrożenie i generowanie certyfikatu należy wykonać z poziomu PowerShell z uprawnieniami administratora.
W tym celu należy wykonać polecenia:

$certName = "CN=TestCert"
$pfxPassword = ConvertTo-SecureString -String "TwojeHaslo123!" -Force -AsPlainText
$pfxPath = "C:\Certyfikaty\TestCert.pfx"

# Tworzenie certyfikatu samopodpisanego
$cert = New-SelfSignedCertificate `
    -Subject $certName `
    -CertStoreLocation "Cert:\CurrentUser\My" ` //Tutaj wskazujemy docelową ścieżkę - ścieżkę do folderu z projektem aplikacji ServerApp
    -KeyExportPolicy Exportable `
    -KeySpec Signature `
    -NotAfter (Get-Date).AddYears(5)

# Eksport certyfikatu do PFX
Export-PfxCertificate `
    -Cert $cert `
    -FilePath $pfxPath `
    -Password $pfxPassword

Możemy też dokonać eksportu tego certyfikatu stosując:

$pfxPassword = ConvertTo-SecureString -String "MocneHaslo123!" -Force -AsPlainText
$pfxPath = "C:\Certyfikaty\EksportowanyCert.pfx"

$cert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*CN=NazwaCertyfikatu*" }

Export-PfxCertificate `
    -Cert $cert `
    -FilePath $pfxPath `
    -Password $pfxPassword

Ważne jest, aby to robić na serwerze. Pomoże to uniknąć ewentualnych problemów w komunikacji między aplikacjami ServerApp a Client App.

W drugiej kolejności, na serwerze należy zaimplementować regułę wychodzącą. W tym celu należy podać numer portu (12345) oraz wskazać, że ruch ma 
się odbywać w obie strony.

Następnie uruchamiamy aplikacje. W pierwszej kolejności należy nacisnąć przycisk "Start" w aplikacji ServerApp w celu inicjalizacji połączenia.
Dalej wpisujemy dowolny tekst w aplikacji ClientApp w miejscu przeznaczonym do wysyłania wiadomości i naciskamy przycisk "Wyślij". Warto zwrócić
uwagę na statusy w poszczególnych aplikacjach. Jeśli wiadomość wysłana nie pojawi się w polu wiadomości odebranych w aplikacji ServerApp - należy
w dowolnym miejscu tej aplikacji kliknąć. Wiadomość powinna się pojawić. Tak też działa w drugą stronę.
