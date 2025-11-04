import { useState, useEffect } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

function LoginPage() {
  const [email, setEmail] = useState("");
  const [senha, setsenha] = useState("");
  const [ehTelaLogin, setEhTelaLogin] = useState(true);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { login, isAuthenticated } = useAuth();

  useEffect(() => {
    if (isAuthenticated) {
      navigate("/home", { replace: true });
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError("");
    setIsLoading(true);

    try {
      const success = await login(email, senha);
      if (success) {
        navigate("/home", { replace: true });
      } else {
        setError("Email ou senha inválidos");
      }
    } catch (err) {
      setError("Erro ao fazer login. Tente novamente.");
    } finally {
      setIsLoading(false);
    }
  };

  function logar() {
    navigate("/home", { replace: true });
  }

  function cadastrar() {
    navigate("/home", { replace: true });
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
          <input type="text" />
          <label>Email</label>
        </div>
        <div className="space"></div>
        <div className="field label border no-margin">
          <input type="text" />
          <label>Senha</label>
        </div>
        <button
          onClick={logar}
          className="w50 small-round vertical-margin tiny-padding large-text"
        >
          ENTRAR
        </button>
        <button
          onClick={irParaCadastro}
          className="w50 border small-round tiny-padding large-text"
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
          <input type="text" />
          <label>Email</label>
        </div>
        <div className="space"></div>
        <div className="field label border no-margin">
          <input type="text" />
          <label>Senha</label>
        </div>
        <div className="space"></div>
        <div className="field label border no-margin">
          <input type="text" />
          <label>Confirmar senha</label>
        </div>
        <div className="space"></div>
        <button
          onClick={cadastrar}
          className="w50 small-round vertical-margin tiny-padding large-text"
        >
          CADASTRAR
        </button>
        <button
          onClick={irParaLogin}
          className="w50 border small-round tiny-padding large-text"
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
