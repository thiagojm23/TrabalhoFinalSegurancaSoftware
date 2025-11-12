import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import axios, { isAxiosError } from "../lib/axios";

function LoginPage() {
  const [email, setEmail] = useState("");
  const [senha, setsenha] = useState("");
  const [ehTelaLogin, setEhTelaLogin] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { login, isAuthenticated } = useAuth();

  useEffect(() => {
    if (isAuthenticated) {
      navigate("/home", { replace: true });
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async () => {
    if (!email || !senha) {
      window.alert("Por favor, preencha todos os campos");
      return;
    }

    setIsLoading(true);
    try {
      const success = await login(email, senha);
      if (success) {
        navigate("/home", { replace: true });
      } else {
        window.alert("Email ou senha inválidos");
      }
    } catch (err) {
      window.alert("Erro ao fazer login. Tente novamente.");
    } finally {
      setIsLoading(false);
    }
  };

  async function cadastrar() {
    if (!email || !senha) {
      window.alert("Por favor, preencha todos os campos");
      return;
    }

    setIsLoading(true);
    try {
      const response = await axios.post(
        "/api/TrabalhoSF/Usuario/CadastrarUsuario",
        {
          email,
          senha,
        }
      );

      console.log("Cadastro realizado:", response.data);
      setEhTelaLogin(true);
      setsenha("");
      window.alert(
        "Cadastro realizado com sucesso! Faça login para continuar."
      );
    } catch (error) {
      console.error("Erro ao cadastrar:", error);
      if (isAxiosError(error)) {
        if (error.response?.status === 400) {
          window.alert("Email já cadastrado ou dados inválidos");
        } else if (error.response?.status === 500) {
          window.alert("Erro no servidor. Tente novamente mais tarde.");
        } else {
          window.alert("Erro ao cadastrar. Tente novamente.");
        }
      } else {
        window.alert("Erro ao cadastrar. Tente novamente.");
      }
    } finally {
      setIsLoading(false);
    }
  }

  function irParaCadastro() {
    setEhTelaLogin(false);
  }

  function irParaLogin() {
    setEhTelaLogin(true);
  }

  function telaLogin() {
    return (
      <>
        <h5>Bem-vindo</h5>
        <div className="vertical-padding large-text">
          Insira suas credenciais para acessar sua conta.
        </div>
        <div className="field label border no-margin">
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            disabled={isLoading}
          />
          <label>Email</label>
        </div>
        <div className="space"></div>
        <div className="field label border no-margin">
          <input
            type="password"
            value={senha}
            onChange={(e) => setsenha(e.target.value)}
            disabled={isLoading}
          />
          <label>Senha</label>
        </div>
        <button
          onClick={handleSubmit}
          className="w50 small-round vertical-margin tiny-padding large-text"
          disabled={isLoading}
        >
          {isLoading ? "ENTRANDO..." : "ENTRAR"}
        </button>
        <button
          onClick={irParaCadastro}
          className="w50 border small-round tiny-padding large-text"
          disabled={isLoading}
        >
          CADASTRAR-SE
        </button>
      </>
    );
  }
  function telaCadastro() {
    return (
      <>
        <h5 className="vertical-padding">
          Insira suas credenciais para criar sua conta.
        </h5>
        <div className="space"></div>
        <div className="field label border no-margin">
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            disabled={isLoading}
          />
          <label>Email</label>
        </div>
        <div className="space"></div>
        <div className="field label border no-margin">
          <input
            type="password"
            value={senha}
            onChange={(e) => setsenha(e.target.value)}
            disabled={isLoading}
          />
          <label>Senha</label>
        </div>
        <div className="space"></div>
        <div className="field label border no-margin">
          <input type="password" disabled={isLoading} />
          <label>Confirmar senha</label>
        </div>
        <div className="space"></div>
        <button
          onClick={cadastrar}
          className="w50 small-round vertical-margin tiny-padding large-text"
          disabled={isLoading}
        >
          {isLoading ? "CADASTRANDO..." : "CADASTRAR"}
        </button>
        <button
          onClick={irParaLogin}
          className="w50 border small-round tiny-padding large-text"
          disabled={isLoading}
        >
          RETORNAR PARA LOGIN
        </button>
      </>
    );
  }

  return (
    <article className="surface-container-low padding border absolute center middle w50">
      {ehTelaLogin ? telaLogin() : telaCadastro()}
    </article>
  );
}

export default LoginPage;
