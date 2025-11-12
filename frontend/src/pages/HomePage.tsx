import { useState } from "react";
import axios, { isAxiosError } from "../lib/axios";

function HomePage() {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [nomeArquivo, setNomeArquivo] = useState("");
  const [isUploading, setIsUploading] = useState(false);
  const [isDownloading, setIsDownloading] = useState(false);
  const [modoOperacao, setModoOperacao] = useState<"upload" | "download">(
    "upload"
  );

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setSelectedFile(file);
      console.log("Arquivo selecionado:", file.name);
    }
  };

  const handleUploadArquivo = async () => {
    if (!selectedFile) {
      window.alert("Por favor, selecione um arquivo primeiro");
      return;
    }

    setIsUploading(true);
    try {
      const formData = new FormData();
      formData.append("arquivo", selectedFile);

      const response = await axios.post(
        "/api/TrabalhoSF/Arquivos/UploadArquivo",
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      );

      console.log("Upload realizado:", response.data);
      window.alert("Arquivo enviado com sucesso!");
      setSelectedFile(null);

      // Limpar o input file
      const fileInput = document.querySelector(
        'input[type="file"]'
      ) as HTMLInputElement;
      if (fileInput) fileInput.value = "";
    } catch (error) {
      console.error("Erro ao fazer upload:", error);
      if (isAxiosError(error)) {
        const mensagemErro =
          error.response?.data?.mensagem || error.response?.data;
        if (error.response?.status === 401) {
          // O interceptor já vai redirecionar
          window.alert("Sessão expirada. Redirecionando para login...");
        } else {
          window.alert(
            typeof mensagemErro === "string"
              ? mensagemErro
              : "Erro ao enviar arquivo. Tente novamente."
          );
        }
      } else {
        window.alert("Erro ao enviar arquivo. Tente novamente.");
      }
    } finally {
      setIsUploading(false);
    }
  };

  const handleBaixarArquivo = async () => {
    if (!nomeArquivo.trim()) {
      window.alert("Por favor, insira o nome do arquivo");
      return;
    }

    setIsDownloading(true);
    try {
      const response = await axios.get(
        `/api/TrabalhoSF/Arquivos/BaixarArquivo/${encodeURIComponent(
          nomeArquivo
        )}`,
        {
          responseType: "blob", // Importante para download de arquivo
        }
      );

      // Verificar se realmente recebeu um arquivo (não HTML)
      const contentType = response.headers["content-type"] || "";
      if (contentType.includes("text/html")) {
        window.alert(
          "Erro: Backend não está respondendo ou arquivo não encontrado."
        );
        return;
      }

      // Criar URL do blob e fazer download
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", nomeArquivo);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);

      console.log("Download realizado:", nomeArquivo);
      window.alert("Arquivo baixado com sucesso!");
    } catch (error) {
      console.error("Erro ao baixar arquivo:", error);
      if (isAxiosError(error)) {
        const mensagemErro =
          error.response?.data?.mensagem || error.response?.data;
        if (error.response?.status === 401) {
          // O interceptor já vai redirecionar
          window.alert("Sessão expirada. Redirecionando para login...");
        } else if (error.response?.status === 404) {
          window.alert(
            typeof mensagemErro === "string"
              ? mensagemErro
              : "Arquivo não encontrado no servidor."
          );
        } else if (error.code === "ERR_NETWORK") {
          window.alert("Erro de conexão: Backend não está rodando.");
        } else {
          window.alert(
            typeof mensagemErro === "string"
              ? mensagemErro
              : "Erro ao baixar arquivo. Tente novamente."
          );
        }
      } else {
        window.alert("Erro ao baixar arquivo. Tente novamente.");
      }
    } finally {
      setIsDownloading(false);
    }
  };

  return (
    <>
      {(isUploading || isDownloading) && (
        <div
          style={{
            position: "fixed",
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: "rgba(0, 0, 0, 0.5)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 9999,
          }}
        >
          <div
            style={{
              backgroundColor: "white",
              padding: "2rem",
              borderRadius: "8px",
              textAlign: "center",
            }}
          >
            <h5>
              {isUploading ? "Salvando arquivo..." : "Carregando arquivo..."}
            </h5>
          </div>
        </div>
      )}

      <article className="surface-container-low padding border absolute center middle w50 text-center">
        <h5>Gerenciador de arquivos</h5>
        <div className="large-space"></div>
        <div>
          <label className="radio">
            <input
              type="radio"
              name="radio"
              checked={modoOperacao === "upload"}
              onChange={() => setModoOperacao("upload")}
            />
            <span>Salvar arquivo</span>
          </label>
          <label className="radio letf-margin">
            <input
              type="radio"
              name="radio"
              checked={modoOperacao === "download"}
              onChange={() => setModoOperacao("download")}
            />
            <span>Ler arquivo</span>
          </label>

          {modoOperacao === "upload" && (
            <div>
              <div className="left-align row">
                <button className="max left-align small-round small-padding large-text">
                  <i>attach_file</i>
                  <span>Arquivo</span>
                  <input type="file" onChange={handleFileSelect} />
                </button>
              </div>
              <div className="space"></div>
              <span>
                {selectedFile
                  ? selectedFile.name
                  : "Nenhum arquivo selecionado"}
              </span>
            </div>
          )}

          {modoOperacao === "download" && (
            <div className="row">
              <div className="max field border prefix large-text">
                <i>attach_file</i>
                <input
                  type="text"
                  value={nomeArquivo}
                  onChange={(e) => setNomeArquivo(e.target.value)}
                  placeholder="Insira o nome do arquivo"
                />
              </div>
            </div>
          )}

          <div className="row">
            {modoOperacao === "upload" && (
              <button
                className="max border small-round primary-border small-padding"
                onClick={handleUploadArquivo}
              >
                <i className="absolute large left left-margin">file_save</i>
                <h6 className="no-margin">Salvar arquivo</h6>
              </button>
            )}
            {modoOperacao === "download" && (
              <button
                className="max border small-round primary-border small-padding"
                onClick={handleBaixarArquivo}
              >
                <i className="absolute large left left-margin">file_open</i>
                <h6 className="no-margin">Ler arquivo salvo</h6>
              </button>
            )}
          </div>
        </div>
      </article>
    </>
  );
}

export default HomePage;
