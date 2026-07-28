import React from 'react';
import { useNavigate } from 'react-router-dom';

export const Icon = ({ children, filled = false }) => <span className="material-symbols-outlined" style={{ fontVariationSettings: filled ? "'FILL' 1" : undefined }}>{children}</span>;

export function Button({ children, onClick, secondary = false, disabled = false }) { return <button disabled={disabled} className={`button ${secondary ? 'secondary' : ''}`} onClick={onClick}>{children}</button>; }

export function Back({ to }) { 
    const navigate = useNavigate();
    return <button className="icon-button" onClick={() => to ? navigate(to) : navigate(-1)} aria-label="Volver"><Icon>arrow_back</Icon></button>; 
}

export function Header({ title, cart = true }) { 
    const navigate = useNavigate();
    return <header className="topbar"><Back /><h1>{title}</h1>{cart ? <button className="icon-button" onClick={() => navigate('/carrito')}><Icon>shopping_cart</Icon></button> : <span className="topbar-spacer" />}</header>; 
}

export function BottomNav({ active = 'home' }) { 
    const navigate = useNavigate();
    const items = [['home','Home','/home'],['search','Buscar','/buscar'],['receipt_long','Pedidos','/historial'],['person','Perfil','/perfil']]; 
    return <nav className="bottom-nav">{items.map(([icon,label,to]) => <button key={label} className={active === label.toLowerCase() || (active === 'orders' && label === 'Pedidos') ? 'active' : ''} onClick={() => navigate(to)}><Icon filled={active === label.toLowerCase()}>{icon}</Icon><span>{label}</span></button>)}</nav>; 
}

export function FormFields({ fields }) { return <div className="fields">{fields.map((field) => <label key={field}>{field}<input type={field === 'Contraseña' ? 'password' : 'text'} placeholder={field === 'Correo electrónico' ? 'correo@ejemplo.com' : `Ingresa tu ${field.toLowerCase()}`} /></label>)}</div>; }

export function ProductRow({ name, price, image }) { 
    const navigate = useNavigate();
    return <article className="product-row" onClick={() => navigate('/producto')}><div><h3>{name}</h3><p className="muted">Carne a la parrilla, queso, tocino y vegetales frescos.</p><b>{price}</b></div><img src={image} alt={name}/></article>; 
}

export function RestaurantCard({ item }) { 
    const navigate = useNavigate();
    return <article className="restaurant-card" onClick={() => navigate('/restaurante')}><div className="restaurant-image" style={{ backgroundImage:`url(${item.image})` }}><span className="rating"><Icon filled>star</Icon>{item.rating}</span></div><div className="card-body"><h3>{item.name}</h3><p><Icon>schedule</Icon> {item.eta} <b>·</b> Envío $25</p><span className="chip">{item.tag}</span></div></article>; 
}
