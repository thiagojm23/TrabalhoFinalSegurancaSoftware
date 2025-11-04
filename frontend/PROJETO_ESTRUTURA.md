# Estrutura do Projeto React

Este projeto segue as melhores práticas da comunidade React para organização de código e estrutura de pastas.

## 📁 Estrutura de Pastas

```
src/
├── components/          # Componentes reutilizáveis
│   └── ProtectedRoute.tsx
├── contexts/           # Context API para estado global
│   └── AuthContext.tsx
├── pages/             # Páginas/Rotas da aplicação
│   ├── LoginPage.tsx
│   ├── LoginPage.css
│   ├── HomePage.tsx
│   └── HomePage.css
├── App.tsx            # Configuração de rotas
├── main.tsx          # Ponto de entrada da aplicação
└── index.css         # Estilos globais
```

## 🎯 Padrões Implementados

### 1. **Roteamento (React Router v6)**

- Uso do `BrowserRouter` para navegação
- Rotas protegidas com `ProtectedRoute`
- Redirecionamento automático baseado em autenticação

### 2. **Context API**

- `AuthContext` para gerenciamento de estado de autenticação
- Hook personalizado `useAuth()` para fácil acesso
- Persistência de login com localStorage

### 3. **Estrutura de Components**

```tsx
// Padrão de importação
import { useState } from "react";
import type { FormEvent } from "react";

// Padrão de componente funcional
function ComponentName() {
  // Estados
  const [state, setState] = useState();

  // Hooks
  const navigate = useNavigate();
  const { user } = useAuth();

  // Handlers
  const handleSubmit = () => {};

  // Render
  return <div>...</div>;
}

export default ComponentName;
```

### 4. **TypeScript**

- Tipos explícitos para props e estados
- Interfaces para estruturas de dados
- Type-only imports quando necessário

### 5. **Organização de CSS**

- CSS modularizado por página/componente
- Arquivo CSS ao lado do componente
- Classes semânticas e reutilizáveis

## 🔐 Sistema de Autenticação

### AuthContext

Provê:

- `user`: Dados do usuário logado
- `login()`: Função para autenticar
- `logout()`: Função para deslogar
- `isAuthenticated`: Boolean do estado de autenticação

### Uso:

```tsx
import { useAuth } from "../contexts/AuthContext";

function MyComponent() {
  const { user, login, logout, isAuthenticated } = useAuth();

  // Use as funções e estados
}
```

## 🛣️ Rotas

- `/` → Redireciona para `/login`
- `/login` → Página de login
- `/home` → Dashboard (requer autenticação)
- Rotas inválidas → Redireciona para `/login`

## 🔒 Proteção de Rotas

```tsx
<Route
  path="/home"
  element={
    <ProtectedRoute>
      <HomePage />
    </ProtectedRoute>
  }
/>
```

## 🚀 Como Executar

```bash
npm install
npm run dev
```

## 📝 Próximos Passos Sugeridos

1. Integrar com API real (substituir autenticação mock)
2. Adicionar validação de formulários (React Hook Form + Zod)
3. Implementar gerenciamento de estado global (Zustand/Redux)
4. Adicionar testes (Vitest + React Testing Library)
5. Implementar lazy loading de rotas
6. Adicionar tratamento de erros global
7. Implementar refresh token

## 🎨 Convenções de Código

- **Componentes**: PascalCase (`LoginPage.tsx`)
- **Funções/variáveis**: camelCase (`handleSubmit`)
- **CSS Classes**: kebab-case (`login-container`)
- **Constantes**: UPPER_SNAKE_CASE (`API_URL`)
- **Interfaces**: PascalCase com "I" ou sem (`User`, `IUser`)

## 📚 Referências

- [React Router Documentation](https://reactrouter.com/)
- [React Context API](https://react.dev/reference/react/useContext)
- [React Best Practices](https://react.dev/learn/thinking-in-react)
