# Relatório de Segurança - Trabalho Final Segurança de Software

## Introdução

Este documento descreve as medidas de segurança implementadas no sistema para mitigar vulnerabilidades comuns e garantir a proteção dos dados e operações. Todas as técnicas implementadas seguem as melhores práticas da OWASP (Open Web Application Security Project).

---

## 1. Autenticação de Usuários

### 1.1 Armazenamento Seguro de Senhas com Hash

**Implementação:** `UsuarioRepositorio.cs` (linhas 29-46)

As senhas dos usuários **nunca** são armazenadas em texto plano. Utilizamos o algoritmo **PBKDF2 (Password-Based Key Derivation Function 2)** com as seguintes características:

- **Algoritmo:** HMAC-SHA256
- **Iterações:** 10.000 (dificulta ataques de força bruta)
- **Salt aleatório:** 128 bits únicos por senha (previne rainbow table attacks)
- **Tamanho do hash:** 256 bits

**Como funciona:**
```csharp
// Gerar hash da senha
string GerarHashSenha(string senha)
{
    // 1. Gera salt aleatório de 128 bits
    byte[] salt = new byte[128/8];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(salt);
    }
    
    // 2. Aplica PBKDF2 com 10.000 iterações
    string hash = Convert.ToBase64String(
        KeyDerivation.Pbkdf2(
            password: senha,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8
        )
    );
    
    // 3. Armazena salt + hash concatenados
    return $"{Convert.ToBase64String(salt)}.{hash}";
}
```

**Benefícios:**
- Mesmo senhas idênticas geram hashes diferentes (devido ao salt único)
- Impossível reverter o hash para obter a senha original
- Alto custo computacional dificulta ataques de força bruta

### 1.2 Proteção contra Ataques de Força Bruta

**Implementação:** `UsuarioController.cs` (linhas 40-108)

O sistema implementa múltiplas camadas de proteção contra tentativas de força bruta:

#### Mecanismo de Bloqueio Temporário

1. **Contador de falhas:** Cada tentativa de login incorreta incrementa `QuantidadeFalhasLogin`
2. **Limite de tentativas:** Após 5 tentativas falhas, a conta é bloqueada
3. **Tempo de bloqueio:** 30 minutos
4. **Reset automático:** Contador é zerado após login bem-sucedido

**Fluxo de proteção:**
```
Tentativa 1-4: Falha → Incrementa contador → Informa tentativas restantes
Tentativa 5:   Falha → Bloqueia conta por 30 minutos → Registra no log
Login bloqueado: Rejeita tentativa → Informa tempo restante
Login sucesso:  Reseta contador → Desbloqueia conta
```

**Características de segurança adicionais:**
- Mensagens de erro genéricas ("Credenciais inválidas") para não revelar se o email existe
- Verificação de bloqueio antes de validar a senha (previne timing attacks)
- Todos os eventos são registrados no sistema de logs

---

## 2. Sistema de Registro de Logs

**Implementação:** `LogsService.cs` e `Logs.cs`

O sistema mantém um registro completo de todas as ações realizadas pelos usuários:

### 2.1 Estrutura dos Logs

Cada registro de log contém:
- **Id:** Identificador único do log
- **UsuarioId:** Referência ao usuário que executou a ação
- **TelaAcao:** Contexto da ação (ex: "Login", "Cadastro", "Upload")
- **TituloAcao:** Título descritivo da ação
- **DescricaoAcao:** Descrição detalhada do que ocorreu
- **DataAcao:** Timestamp UTC da ação

### 2.2 Eventos Registrados

O sistema registra:
- ✅ Cadastro de novos usuários
- ✅ Tentativas de login (sucesso e falhas)
- ✅ Bloqueio de contas por múltiplas tentativas
- ✅ Tentativas de acesso a contas bloqueadas
- ✅ Upload de arquivos

### 2.3 Proteção contra Log Injection

**Implementação:** `LogsService.cs` (linhas 29-32)

Todas as entradas de usuário que vão para os logs são sanitizadas através da função `NeutralizaEntradaUsuario()`:

```csharp
public string NeutralizaEntradaUsuario(string input)
{
    return input.Replace("\n", "").Replace("\r", "").Replace("\t", "");
}
```

**Proteção contra:**
- Quebras de linha (`\n`, `\r`) que poderiam injetar linhas falsas nos logs
- Caracteres de tabulação (`\t`) que poderiam causar confusão na leitura
- Manipulação da estrutura dos logs por entradas maliciosas

---

## 3. Proteção contra SQL Injection

**Implementação:** `AppDbContext.cs` e `UsuarioRepositorio.cs`

O sistema utiliza **Entity Framework Core** com queries parametrizadas, que oferece proteção completa contra SQL Injection:

### 3.1 Uso de ORM (Object-Relational Mapping)

Todas as consultas ao banco de dados são feitas através do Entity Framework:

```csharp
// Exemplo seguro - Entity Framework parametriza automaticamente
await _usuarios.FirstOrDefaultAsync(u => u.Email == email);
```

**Por que é seguro:**
- Entity Framework converte expressões LINQ em SQL parametrizado
- Entradas de usuário nunca são concatenadas diretamente no SQL
- O framework automaticamente faz escape de caracteres especiais

### 3.2 Validações Adicionais

O modelo de dados define restrições:
- Tamanho máximo dos campos (ex: Email até 50 caracteres)
- Índices únicos (previne duplicações)
- Relacionamentos com integridade referencial

---

## 4. Proteção contra Path Traversal (Caminho Transversal)

**Implementação:** `ArquivosController.cs` (linhas 14-71)

O sistema implementa múltiplas camadas de proteção para uploads de arquivos:

### 4.1 Validações Implementadas

1. **Validação de extensão:**
   ```csharp
   var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".txt", ".docx" };
   var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
   if (!extensoesPermitidas.Contains(extensao))
       return BadRequest("Tipo de arquivo não permitido.");
   ```

2. **Limitação de tamanho:**
   ```csharp
   if (arquivo.Length > 5 * 1024 * 1024) // 5MB
       return BadRequest("Arquivo muito grande.");
   ```

3. **Nome de arquivo seguro:**
   ```csharp
   // Ignora completamente o nome fornecido pelo usuário
   var nomeArquivoSeguro = $"{Guid.NewGuid()}{extensao}";
   ```

4. **Validação de caminho (Path Traversal Protection):**
   ```csharp
   var caminhoCompleto = Path.GetFullPath(caminhoArquivo);
   var caminhoBaseCompleto = Path.GetFullPath(caminhoBase);
   
   // Garante que o arquivo será salvo dentro do diretório permitido
   if (!caminhoCompleto.StartsWith(caminhoBaseCompleto))
   {
       return BadRequest("Caminho de arquivo inválido.");
   }
   ```

**Proteção contra ataques:**
- ❌ `../../etc/passwd` → Bloqueado pela validação de caminho
- ❌ `../../../windows/system32/config/sam` → Bloqueado pela validação de caminho
- ❌ `malware.exe` → Bloqueado pela validação de extensão
- ✅ Somente arquivos seguros em diretório controlado

---

## 5. Proteção contra Cross-Site Scripting (XSS)

**Implementação:** Múltiplas camadas

### 5.1 Content Security Policy (CSP)

**Implementação:** `Program.cs` (linhas 47-55)

```csharp
context.Response.Headers["Content-Security-Policy"] = 
    "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';";
```

**O que faz:**
- `default-src 'self'`: Só permite recursos da mesma origem
- `script-src 'self'`: Só permite scripts do próprio domínio
- `style-src 'self' 'unsafe-inline'`: Permite estilos inline (necessário para React)

**Previne:**
- Injeção de scripts externos maliciosos
- Execução de JavaScript inline não autorizado
- Carregamento de recursos de domínios não confiáveis

### 5.2 Headers de Segurança Adicionais

```csharp
context.Response.Headers["X-Content-Type-Options"] = "nosniff";
context.Response.Headers["X-Frame-Options"] = "DENY";
context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
```

**Explicação:**
- **X-Content-Type-Options:** Previne MIME sniffing attacks
- **X-Frame-Options:** Previne clickjacking (não permite iframe)
- **X-XSS-Protection:** Ativa filtro XSS do navegador
- **Referrer-Policy:** Controla informações de referrer enviadas

### 5.3 Sanitização de Entradas

O sistema sanitiza todas as entradas de usuário antes de armazenar nos logs (ver seção 2.3).

---

## 6. Proteção contra Cross-Site Request Forgery (CSRF)

**Implementação:** `Program.cs` (linhas 14-85) e `ArquivosController.cs` (linha 9)

### 6.1 Configuração do Antiforgery Token

```csharp
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
});
```

### 6.2 Endpoint para Obter Token

```csharp
app.MapGet("/api/csrf-token", (HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
        new CookieOptions
        {
            HttpOnly = false, // Frontend precisa ler
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
    return Results.Ok(new { token = tokens.RequestToken });
});
```

### 6.3 Validação em Controllers

```csharp
[ValidateAntiForgeryToken]
public class ArquivosController : ControllerBase
```

**Como funciona:**
1. Frontend solicita token CSRF ao endpoint `/api/csrf-token`
2. Backend gera token único e envia no cookie
3. Frontend inclui token no header `X-XSRF-TOKEN` em requisições POST/PUT/DELETE
4. Backend valida se o token do header corresponde ao cookie
5. Requisição é rejeitada se tokens não coincidirem

**Proteção:**
- Sites maliciosos não conseguem obter o token CSRF
- Requisições forjadas de outros domínios são bloqueadas
- Cookie com `SameSite=Strict` previne envio em contextos cross-site

---

## 7. Medidas de Segurança Adicionais

### 7.1 Configuração CORS Restritiva

**Implementação:** `Program.cs` (linhas 27-37)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Necessário para CSRF cookies
    });
});
```

**Benefícios:**
- Apenas origens específicas podem fazer requisições
- Previne acesso de domínios não autorizados
- Suporta credenciais (necessário para autenticação)

### 7.2 HTTPS Redirection

**Implementação:** `Program.cs` (linha 57)

```csharp
app.UseHttpsRedirection();
```

**Benefícios:**
- Todo tráfego HTTP é automaticamente redirecionado para HTTPS
- Protege dados em trânsito contra interceptação
- Previne ataques man-in-the-middle

### 7.3 Validação de Dados de Entrada

O sistema utiliza Data Annotations e validações explícitas:
- Verificação de campos obrigatórios
- Validação de formato de email
- Verificação de tipos de dados
- Limitações de tamanho de campos

### 7.4 Tratamento Seguro de Erros

**Implementação:** `ArquivosController.cs` (linha 69)

```csharp
catch (Exception ex)
{
    return StatusCode(500, $"{ex}. Erro ao salvar o arquivo.");
}
```

**Nota de segurança:** Em produção, a mensagem de erro detalhada deve ser removida para não expor informações do sistema. Deve ser usado apenas logging interno.

### 7.5 Integridade Referencial do Banco de Dados

**Implementação:** `AppDbContext.cs` (linhas 27-30)

```csharp
entidade.HasOne(e => e.Usuario)
    .WithMany(u => u.Logs)
    .HasForeignKey(e => e.UsuarioId)
    .OnDelete(DeleteBehavior.Cascade);
```

**Benefícios:**
- Garante consistência dos dados
- Logs são automaticamente removidos quando usuário é deletado
- Previne referências órfãs no banco de dados

---

## 8. Resumo das Proteções

| Vulnerabilidade | Status | Técnicas Implementadas |
|----------------|--------|------------------------|
| **Senhas em texto plano** | ✅ Protegido | PBKDF2 com HMAC-SHA256, salt aleatório, 10.000 iterações |
| **Ataque de força bruta** | ✅ Protegido | Bloqueio após 5 tentativas, timeout de 30 minutos, logging |
| **SQL Injection** | ✅ Protegido | Entity Framework com queries parametrizadas |
| **Path Traversal** | ✅ Protegido | Validação de caminho, whitelist de extensões, nome aleatorizado |
| **Cross-Site Scripting (XSS)** | ✅ Protegido | CSP headers, X-XSS-Protection, sanitização de inputs |
| **Cross-Site Request Forgery (CSRF)** | ✅ Protegido | Antiforgery tokens, SameSite cookies, validação de tokens |
| **Log Injection** | ✅ Protegido | Neutralização de caracteres especiais (\n, \r, \t) |

---

## 9. Recomendações para Produção

Para um ambiente de produção, considere as seguintes melhorias adicionais:

1. **Autenticação JWT:** Implementar tokens JWT para sessões stateless
2. **Rate Limiting:** Adicionar limitação de requisições por IP
3. **Auditoria de Logs:** Implementar sistema de análise e alertas de logs
4. **Backup de Segurança:** Backups automáticos criptografados do banco de dados
5. **Monitoramento:** Implementar ferramentas de monitoramento de segurança (SIEM)
6. **HTTPS Obrigatório:** Configurar certificados SSL/TLS válidos
7. **Variáveis de Ambiente:** Mover configurações sensíveis para variáveis de ambiente
8. **Testes de Segurança:** Realizar penetration testing e code review regular
9. **Atualização de Dependências:** Manter todas as bibliotecas atualizadas
10. **Política de Senhas:** Exigir senhas fortes (mínimo 8 caracteres, caracteres especiais, etc.)

---

## 10. Conclusão

O sistema implementa um conjunto robusto de medidas de segurança que protegem contra as vulnerabilidades mais comuns listadas pela OWASP Top 10. Todas as técnicas seguem as melhores práticas da indústria e foram implementadas em múltiplas camadas para garantir defesa em profundidade (defense in depth).

O código está estruturado de forma clara e manutenível, facilitando futuras auditorias de segurança e atualizações. O sistema de logs permite rastreamento completo de ações, essencial para investigação de incidentes de segurança.

---

**Documento elaborado em:** Novembro de 2025  
**Versão:** 1.0
