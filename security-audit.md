# Security Audit

## OWASP top 10 web vulnerabilities research

### 1. Broken Access Control
Exista doar un singru rol, iar site-ul nu are useri. Toate resursele sunt accesibile.

### 2. Cryptographic Failures
Nu exista criptografie incorporata, decat probail incorporata in .NET.

### 3. Injection
Riscuri de tipul XSS nu exista (cu o singura exceptie). Neexistand useri nu se poate salva content care sa fie afisat
pe pagina altui user.

Exceptia o face doar elementele afisate din baza de date. Daca un user ar avea acces la contentul din baza de date atunci
ar putea injecta cod javascript, dar si acest factor e minigat de mecanismele de protectie din .NET.

.NET are mecanisme de protectie integrate si pentru SQLi, insa se poate recurge si la un audit automat cu tool-ul `sqlmap`.

Exista un singur punct in care userul poate accesa baza de date: /Product

Am rulat comanda:

```sh
sqlmap -u "https://localhost:7285/Product" --data="quantity=test&query=test&unit=test&exactItemName=test" --level=5 --risk=3
```

Nu am obtinut cum ca unul din parametri ar fi vulnerabil.

Rezultat:
```
[00:02:59] [CRITICAL] all tested parameters do not appear to be injectable. If you suspect that there is some kind of protection mechanism involved (e.g. WAF) maybe you could try to use option '--tamper' (e.g. '--tamper=space2comment') and/or switch '--random-agent'
```

### 4. Insecure Design
O vulnerabilitate de tip Denial of Service poate aparea din design-ul aplicatiei. In backend aplicatia porneste trei procese la 
fiecare search al unui user. Daca mai multi useri ar face search-uri, memoria serverului poate cadea.

### 5. Security Misconfiguration
X

### 6. Vulnerable and Outdate Components
.NET 6 nu mai beneficiaza de suport din 12 Noiembrie 2024. Un upgrade la cea mai noua versiune este necesar.

### 7. Identification and Authentication problems
Aplicatia nu are vreun mecanism de identificare sau autentificare.

### 8. Software and Data Integrity Failures
Dependintele proiectului sunt:

Frontend:
- bootstrap 5.3.0 (CDN)
- Google Maps API

Backend:
- Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore 6.0.22
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 6.0.22
- Microsoft.AspNetCore.Identity.UI 6.0.22
- Microsoft.EntityFrameworkCore 6.0.29
- Microsoft.EntityFrameworkCore.Design 6.0.29
- Microsoft.EntityFrameworkCore.Relational 6.0.29
- Microsoft.EntityFrameworkCore.SqlServer 6.0.22
- Microsoft.EntityFrameworkCore.Tools 6.0.22
- Microsoft SQL Server 2022 16.0.4175.1 (X64)

### 9. Security Logging and Monitoring Failures
Nu exista loguri de securitate sau monitorizare.

### 10. Server-Side Request Forgery
Serverul face cateva requesturi la url-uri din afara retelei locale, dar userul nu are acces la modificarea
acestor parametri.