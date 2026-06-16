<!-- src/views/auth/LoginView.vue -->
<template>
    <div class="auth-layout">
        <div class="auth-card animar-entrada">
            <!-- Logo -->
            <div class="auth-logo">
                <span class="logo-icono">₿</span>
                <h1 class="auth-titulo">CryptoWallet</h1>
            </div>

            <h2 class="auth-subtitulo">Iniciá sesión</h2>
            <p class="texto-secundario" style="margin-bottom: 2rem;">
                ¿No tenés cuenta?
                <RouterLink to="/registro" class="link-auth">Registrate</RouterLink>
            </p>

            <div class="form-grupo">
                <label class="form-label">Email</label>
                <input  
                    v-model="form.email"
                    type="email"
                    placeholder="tu@email.com"
                    class="form-control"
                    :class="{ 'es-error' : errores.email }"
                    @keyup.enter="ingresar"
                />
                <span v-if="errores.email" class="form-error">{{ errores.email }}</span>
            </div>

            <div class="form-grupo" style="margin-top: 1rem; ">
                <label class="form-label">Contraseña</label>
                <input
                    v-model="form.password" 
                    type="password"
                    placeholder="••••••••"
                    class="form-control"
                    :class="{ 'es-error' : errores.password}"
                    @keyup.enter="ingresar"
                />
                <span v-if="errores.password" class="form-error">{{ errores.password }}</span>
            </div>

            <div v-if="authStore.error" class="banner-error" style="margin-top: 1rem; ">
                ⚠️ {{ authStore.error }}
            </div>

            <button
                @click="ingresar"
                class="btn btn-primario auth-btn"
                :disabled="authStore.cargando"
            >
                {{ authStore.cargando ? 'Ingresando...' : 'Ingresar' }}
            </button>

        </div>
    </div>
</template>

<script setup>

    import { reactive  } from 'vue';
    import { useRouter } from 'vue-router';
    import { useAuthStore } from '@/stores/auth';

    const router = useRouter()
    const authStore = useAuthStore()

    const form = reactive({ email: '', password: '' })
    const errores = reactive({ email: '', password: ''})

    const validar = () => {
        errores.email = ''
        errores.password = ''
        let valido = true

        if (!form.email) {
            errores.email = 'El email es obligatorio'
            valido = false
        }

        if (!form.password) {
            errores.password = 'La contraseña es obligatoria'
            valido = false
        }

        return valido
    }

    const ingresar = async () => {
        if (!validar()) return
        try {
            await authStore.login({ email: form.email, password: form.password })
            router.push('/dashboard')
        } catch {
            // El error ya lo maneja el store
        }
    } 

</script>

<style scoped>

    .auth-layout{
        min-height: 100vh;
        display: flex;
        align-items: center;
        justify-content: center;
        background: var(--bg-primary);
        padding: var(--espacio-md);
        width: 100vw;
        position: fixed;
        inset: 0;
    }

    .auth-card {
        background: var(--bg-card);
        border: 1px solid var(--border);
        border-radius: var(--radio-xl);
        padding: var(--espacio-2xl);
        width: 100%;
        max-width: 420px;
        box-shadow: var(--sombra-lg);
    }

    .auth-logo {
        display: flex;
        align-items: center;
        gap: var(--espacio-md);
        margin-bottom: var(--espacio-xl);
    }

    .logo-icono {
        font-size: 1.5rem;
        background: var(--acento);
        width: 44px;
        height: 44px;
        border-radius: var(--radio-md);
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .auth-titulo {
        font-size: var(--texto-xl);
        font-weight: 700;
    }

    .auth-subtitulo {
        font-size: var(--texto-2xl);
        font-weight: 700;
        margin-bottom: var(--espacio-xs);
    }

    .link-auth {
        color: var(--acento);
        text-decoration: none;
        font-weight: 600;
    }

    .link-auth:hover { text-decoration: underline; }

    .auth-btn {
        width: 100%;
        margin-top: var(--espacio-xl);
        padding: 0.75rem;
        font-size: var(--texto-base);
    }

    .banner-error {
        background: rgba(255, 77, 109, 0.1);
        border: 1px solid var(--peligro);
        border-radius: var(--radio-md);
        padding: var(--espacio-md);
        color: var(--peligro);
        font-size: var(--texto-sm);
    }

</style>