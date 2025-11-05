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
        <div>
          <div className="left-align row">
            <button className="max left-align small-round small-padding large-text">
              <i>attach_file</i>
              <span>Arquivo</span>
              <input type="file" />
            </button>
          </div>
          <div className="row">
            <button className="max border small-round primary-border small-padding">
              <i className="absolute large left left-margin">file_save</i>
              <h6 className="no-margin">Salvar arquivo</h6>
            </button>
            <button className="max border small-round primary-border small-padding">
              <i className="absolute large left left-margin">file_open</i>
              <h6 className="no-margin">Ler arquivo salvo</h6>
            </button>
          </div>
        </div>
      </article>
    </>
  );
}

export default HomePage;
