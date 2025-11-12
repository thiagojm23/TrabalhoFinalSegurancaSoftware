# Guia de Autenticação

## 🔐 Como funciona a autenticação

O sistema usa **cookies httpOnly** do backend para gerenciar a sessão. Não é necessário armazenar tokens no localStorage.

## ✅ O que foi implementado:

### 1. **AuthContext** (`src/contexts/AuthContext.tsx`)

- Função `login()` que chama `/api/TrabalhoSF/Usuario/Logar`
- Backend retorna cookie httpOnly automaticamente
- Não salva nada no localStorage

### 2. **Interceptor Axios** (`src/lib/axios.ts`)

- Intercepta todas as respostas da API
- Se receber **401 Unauthorized**, redireciona automaticamente para `/login`
- Limpa qualquer dado local

### 3. **LoginPage** (`src/pages/LoginPage.tsx`)

- Função `handleSubmit()` para login
- Função `cadastrar()` para registro
- Estados `isLoading` para desabilitar inputs durante requisições
- Usa o AuthContext para gerenciar autenticação

## 📖 Como usar nas páginas

### Importar o axios configurado:

```tsx
import axios from "../lib/axios";
```

### Fazer requisições protegidas:

```tsx
async function buscarDados() {
  try {
    const response = await axios.get("/api/TrabalhoSF/SeuEndpoint");
    console.log(response.data);
  } catch (error) {
    // Se for 401, o interceptor já redireciona para /login
    console.error("Erro:", error);
  }
}
```

### O interceptor cuida de:

- ✅ Redirecionar para login se receber 401
- ✅ Limpar dados locais
- ✅ Funciona em TODAS as requisições automaticamente

## 🚀 Fluxo completo:

1. **Login**:

   - Usuário preenche email/senha
   - `handleSubmit()` chama `login()` do AuthContext
   - Backend retorna cookie httpOnly
   - Redireciona para `/home`

2. **Navegação protegida**:

   - Todas as páginas usam `import axios from "../lib/axios"`
   - Requisições incluem automaticamente o cookie
   - Se sessão expirar (401), volta para login

3. **Logout**:
   - Chama `logout()` do AuthContext
   - Pode chamar endpoint de logout do backend se necessário
   - Redireciona para `/login`

## 🔧 Endpoints usados:

- `POST /api/TrabalhoSF/Usuario/Logar` - Login
- `POST /api/TrabalhoSF/Usuario/CadastrarUsuario` - Cadastro
