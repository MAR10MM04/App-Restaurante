import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon, Button, Header, Back, BottomNav } from '../components/ui';
import { foodImage, pizzaImage } from './Main';

export function OrderPlaced() { 
    const navigate = useNavigate();
    return <main className="status-screen"><div className="success-icon"><Icon filled>check</Icon></div><h1>¡Pedido realizado!</h1><p className="muted">Tu pedido fue enviado al restaurante. Te avisaremos cuando salga rumbo a ti.</p><div className="order-number">Pedido #DF-8294</div><Button onClick={() => navigate('/seguimiento')}>Seguir mi pedido</Button><Button secondary onClick={() => navigate('/home')}>Volver al inicio</Button></main>; 
}

export function Tracking() { 
    const navigate = useNavigate();
    return <div className="page tracking"><div className="map"><Back to="/home" /><div className="map-pin"><Icon filled>delivery_dining</Icon></div></div><main className="tracking-sheet"><span className="handle"/><p className="eyebrow">LLEGARÁ EN 18-25 MIN</p><h1>Tu pedido está en camino</h1><p className="muted">Carlos está recogiendo tu pedido en Burger King.</p><div className="timeline"><p className="done"><Icon filled>check_circle</Icon> Pedido confirmado</p><p className="done"><Icon filled>check_circle</Icon> Restaurante preparando</p><p><Icon>radio_button_unchecked</Icon> Repartidor en camino</p><p><Icon>radio_button_unchecked</Icon> Pedido entregado</p></div><Button onClick={() => navigate('/pedido-entregado')}>Simular entrega</Button></main></div>; 
}

export function Delivered() { 
    const navigate = useNavigate();
    return <main className="status-screen"><div className="success-icon"><Icon filled>delivery_dining</Icon></div><h1>¡Pedido entregado!</h1><p className="muted">Esperamos que disfrutes tu comida.</p><Button onClick={() => navigate('/calificacion')}>Calificar pedido</Button><Button secondary onClick={() => navigate('/home')}>Volver al inicio</Button></main>; 
}

export function Rating() { 
    const navigate = useNavigate();
    const [rate,setRate]=useState(0); 
    return <main className="status-screen rating-screen"><h1>¿Cómo estuvo tu pedido?</h1><p className="muted">Tu opinión nos ayuda a mejorar.</p><div className="stars">{[1,2,3,4,5].map(n => <button key={n} onClick={() => setRate(n)}><Icon filled={n <= rate}>star</Icon></button>)}</div><textarea placeholder="Cuéntanos más sobre tu experiencia (opcional)"/><Button onClick={() => navigate('/historial')} disabled={!rate}>Enviar calificación</Button></main>; 
}

export function History() { 
    const navigate = useNavigate();
    return <div className="page"><Header title="Mis pedidos" cart={false}/><main className="section"><h2>Pedidos recientes</h2>{[{name:'Burger King',date:'Hoy · 1:30 PM',total:'$174.00'},{name:'Pizzeria Napoli Centrale',date:'12 Jul · 8:15 PM',total:'$225.00'}].map(order => <article className="order-card" key={order.name}><img src={order.name.includes('Pizza')?pizzaImage:foodImage} alt=""/><div><h3>{order.name}</h3><p>{order.date}</p><b>{order.total}</b></div><button className="text-button" onClick={() => navigate('/restaurante')}>Repetir</button></article>)}</main><BottomNav active="orders" /></div>; 
}
