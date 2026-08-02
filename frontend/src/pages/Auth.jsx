import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon, Button, Back } from '../components/ui';
import api from '../services/api';

export function Splash() { 
    const navigate = useNavigate();
    return <main className="splash">
        <div className="splash-brand">
            <div className="logo"><Icon filled>restaurant</Icon></div>
            <h1>DiDi Food</h1>
            <p>La comida que amas, entregada a tu puerta.</p>
        </div>
        <div className="loading"><span className="spinner" /> Localizando restaurantes...</div>
        <Button onClick={() => navigate('/bienvenida')}>Comenzar</Button>
    </main>; 
}

export function Welcome() { 
    const navigate = useNavigate();
    return <main className="auth welcome">
        <div className="hero-icon"><Icon filled>delivery_dining</Icon></div>
        <div>
            <p className="eyebrow">DI DI FOOD</p>
            <h1>Tu comida favorita, más cerca que nunca.</h1>
            <p className="muted">Descubre tus restaurantes favoritos y recibe tu pedido donde estés.</p>
        </div>
        <Button onClick={() => navigate('/registro')}>Crear una cuenta</Button>
        <Button secondary onClick={() => navigate('/login')}>Ya tengo una cuenta</Button>
    </main>; 
}

export function Login() { 
    const navigate = useNavigate();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleLogin = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        try {
            const res = await api.post('/auth/login', {
                email,
                contrasena: password
            });
            if (res.data && res.data.token) {
                localStorage.setItem('token', res.data.token);
                // FIXED BUG: res.data IS the user object!
                localStorage.setItem('user', JSON.stringify(res.data));
                navigate('/home');
            }
        } catch (err) {
            setError(err.response?.data?.mensaje || 'Error al iniciar sesión');
        } finally {
            setLoading(false);
        }
    };

    return <main className="auth form-screen">
        <Back to="/bienvenida" />
        <h1>¡Qué gusto verte de nuevo!</h1>
        <p className="muted">Inicia sesión para continuar.</p>
        {error && <p style={{color: 'red'}}>{error}</p>}
        <form className="fields" onSubmit={handleLogin}>
            <label>Correo electrónico
                <input type="email" required value={email} onChange={e => setEmail(e.target.value)} placeholder="correo@ejemplo.com" />
            </label>
            <label>Contraseña
                <input type="password" required value={password} onChange={e => setPassword(e.target.value)} placeholder="Ingresa tu contraseña" />
            </label>
            <Button disabled={loading}>{loading ? 'Iniciando...' : 'Iniciar sesión'}</Button>
        </form>
        <p className="center muted">¿No tienes cuenta? <button className="text-button" type="button" onClick={() => navigate('/registro')}>Regístrate</button></p>
    </main>; 
}

export function Register() { 
    const navigate = useNavigate();
    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleRegister = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        try {
            await api.post('/usuarios', {
                nombre: name,
                email: email,
                contrasena: password,
                telefono: "0000000000" // Placeholder
            });
            
            // Auto login
            const res = await api.post('/auth/login', {
                email: email,
                contrasena: password
            });
            if (res.data && res.data.token) {
                localStorage.setItem('token', res.data.token);
                localStorage.setItem('user', JSON.stringify(res.data));
                navigate('/permisos');
            }
        } catch (err) {
            setError(err.response?.data?.mensaje || 'Error al registrarse');
        } finally {
            setLoading(false);
        }
    };

    return <main className="auth form-screen">
        <Back to="/bienvenida" />
        <h1>Crea tu cuenta</h1>
        <p className="muted">Solo toma un minuto comenzar a pedir.</p>
        {error && <p style={{color: 'red'}}>{error}</p>}
        <form className="fields" onSubmit={handleRegister}>
            <label>Nombre completo
                <input required value={name} onChange={e => setName(e.target.value)} placeholder="Ingresa tu nombre completo" />
            </label>
            <label>Correo electrónico
                <input type="email" required value={email} onChange={e => setEmail(e.target.value)} placeholder="correo@ejemplo.com" />
            </label>
            <label>Contraseña
                <input type="password" required value={password} onChange={e => setPassword(e.target.value)} placeholder="Crea una contraseña" />
            </label>
            <Button disabled={loading}>{loading ? 'Registrando...' : 'Continuar'}</Button>
        </form>
        <p className="center muted">¿Ya tienes cuenta? <button className="text-button" type="button" onClick={() => navigate('/login')}>Inicia sesión</button></p>
    </main>; 
}

export function Permissions() { 
    const navigate = useNavigate();
    return <main className="auth permissions">
        <div className="permission-map"><Icon filled>location_on</Icon></div>
        <h1>Activa tu ubicación</h1>
        <p className="muted">Así encontraremos restaurantes y repartidores cerca de ti.</p>
        <Button onClick={() => navigate('/home')}>Permitir ubicación</Button>
        <button className="text-button" onClick={() => navigate('/home')}>Ahora no</button>
    </main>; 
}
