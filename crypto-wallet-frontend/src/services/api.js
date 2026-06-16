// src/services/api.js
// ============================================
// Configuración central de Axios.
// Todos los llamados a la API pasan por acá.
// ============================================
import axios from 'axios';

// Instancia configurada apuntando a nuestra API de ASP.NET
const api = axios.create({
    baseURL: 'https://localhost:7004/api',      //URL de tu backend
    timeout: 10000,
    headers: { 'Content-Type': 'application/json' }
})

// ── Interceptor de REQUEST ────────────────────────────────
//Agrega el token JWT en cada petición automáticamente
api.interceptors.request.use(config => {
    const token = localStorage.getItem('token')
    if (token){
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
})



// ── Interceptor de respuesta ──────────────────────────────
// Se ejecuta después de cada respuesta.
// Si hay un error, lo transforma en algo legible.
api.interceptors.response.use(
    respuesta => respuesta,
    error => {
        //Si el token expiró, mandamos al login
        if (error.response?.status === 401){
            localStorage.removeItem('token')
            localStorage.removeItem('usuario')
            window.location.href = '/login'
        }

        const mensaje = error.response?.data?.mensaje 
            || 'Error de conexión con el servidor'
        return Promise.reject(new Error(mensaje))
    }
)

// Métodos organizados por recurso
export const transaccionesApi = {
    obtenerTodas: (pagina = 1, tamañoPagina = 20, codigoCripto, accion, fechaDesde, fechaHasta) => api.get('/transacciones', { params: { pagina, tamañoPagina, codigoCripto, accion, fechaDesde, fechaHasta }}),
    obtenerPorId: (id) => api.get(`/transacciones/${id}`),
    previewEdicion: (id, cantidad) => api.get(`/transacciones/${id}/preview`, { params: {cantidad }}),
    crear: (datos) => api.post('/transacciones', datos),
    editar: (id, datos) => api.patch(`/transacciones/${id}`, datos),
    eliminar: (id) => api.delete(`/transacciones/${id}`)
}

export const portfolioApi = {
    obtener: () => api.get('/portfolio', { timeout: 20000 }),
    obtenerHistorial: () => api.get('portfolio/historial')
}

export const criptosApi = {
    obtenerTodas: () => api.get('/criptomonedas')
}

//Endpoints de autenticación
export const authApi = {
    registro: (datos) => api.post('/auth/registro', datos),
    login: (datos) => api.post('/auth/login', datos),
    perfil: () => api.get('/auth/perfil'),
    obtenerMetodosPago: () => api.get('/auth/metodos-pago'),
    agregarMetodoPago: (datos) => api.post('/auth/metodos-pago', datos),
    eliminarMetodoPago: (id) => api.delete(`/auth/metodos-pago/${id}`)
}

export default api