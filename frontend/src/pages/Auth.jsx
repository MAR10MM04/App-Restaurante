import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon, Button, FormFields, Back } from '../components/ui';

export function Splash() { 
    const navigate = useNavigate();
    return <main className="splash"><div className="splash-brand"><div className="logo"><Icon filled>restaurant</Icon></div><h1>DiDi Food</h1><p>La comida que amas, entregada a tu puerta.</p></div><div className="loading"><span className="spinner" /> Localizando restaurantes...</div><Button onClick={() => navigate('/bienvenida')}>Comenzar</Button></main>; 
}

export function Welcome() { 
    const navigate = useNavigate();
    return <main className="auth welcome"><div className="hero-icon"><Icon filled>delivery_dining</Icon></div><div><p className="eyebrow">DI DI FOOD</p><h1>Tu comida favorita, más cerca que nunca.</h1><p className="muted">Descubre tus restaurantes favoritos y recibe tu pedido donde estés.</p></div><Button onClick={() => navigate('/registro')}>Crear una cuenta</Button><Button secondary onClick={() => navigate('/login')}>Ya tengo una cuenta</Button></main>; 
}

export function Login() { 
    const navigate = useNavigate();
    return <main className="auth form-screen"><Back to="/bienvenida" /><h1>¡Qué gusto verte de nuevo!</h1><p className="muted">Inicia sesión para continuar.</p><FormFields fields={['Correo electrónico','Contraseña']} /><Button onClick={() => navigate('/home')}>Iniciar sesión</Button><p className="center muted">¿No tienes cuenta? <button className="text-button" onClick={() => navigate('/registro')}>Regístrate</button></p></main>; 
}

export function Register() { 
    const navigate = useNavigate();
    return <main className="auth form-screen"><Back to="/bienvenida" /><h1>Crea tu cuenta</h1><p className="muted">Solo toma un minuto comenzar a pedir.</p><FormFields fields={['Nombre completo','Correo electrónico','Contraseña']} /><Button onClick={() => navigate('/permisos')}>Continuar</Button><p className="center muted">¿Ya tienes cuenta? <button className="text-button" onClick={() => navigate('/login')}>Inicia sesión</button></p></main>; 
}

export function Permissions() { 
    const navigate = useNavigate();
    return <main className="auth permissions"><div className="permission-map"><Icon filled>location_on</Icon></div><h1>Activa tu ubicación</h1><p className="muted">Así encontraremos restaurantes y repartidores cerca de ti.</p><Button onClick={() => navigate('/home')}>Permitir ubicación</Button><button className="text-button" onClick={() => navigate('/home')}>Ahora no</button></main>; 
}
