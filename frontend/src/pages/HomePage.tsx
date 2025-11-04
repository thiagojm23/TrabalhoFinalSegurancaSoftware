import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

function HomePage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <>
      <article className="surface-container-low padding border absolute center middle w50 text-center">
        <h5>Insira um arquivo</h5>
        <div className="large-space"></div>
        <div className="left-align row">
          <div>
            <div>
              <label className="radio">
                <input type="radio" name="ler" />
                <span>Ler arquivo já salvo</span>
              </label>
            </div>
            <div>
              <label className="radio">
                <input type="radio" name="salvar" />
                <span>Salvar novo arquivo</span>
              </label>
            </div>
          </div>
          <button className="max left-align small-round small-padding large-text">
            <i>attach_file</i>
            <span>Arquivo</span>
            <input type="file" />
          </button>
        </div>
      </article>
    </>
  );
}

export default HomePage;
