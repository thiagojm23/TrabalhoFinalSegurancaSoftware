import axios from "axios";

// Configurar baseURL para o backend
const axiosInstance = axios.create({
  baseURL: "https://localhost:7135",
  withCredentials: true, // Importante para enviar cookies httpOnly
});

// Configurar interceptor para tratar erros de autenticação
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    // Se receber 401 Unauthorized, redireciona para login
    if (error.response?.status === 401) {
      // Limpar qualquer estado de autenticação
      localStorage.clear();

      // Redirecionar para login
      window.location.href = "/login";
    }

    return Promise.reject(error);
  }
);

// Exportar tanto a instância quanto o isAxiosError
export const isAxiosError = axios.isAxiosError;
export default axiosInstance;
