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

const DEFAULT_IMAGE = 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=1200&q=85';

export function ProductRow({ id, name, price, image, fallbackImage = DEFAULT_IMAGE }) {
    const navigate = useNavigate();
    const handleImageError = (e) => {
        e.currentTarget.onerror = null;
        e.currentTarget.src = fallbackImage;
    };
    return <article className="product-row" onClick={() => navigate(`/producto/${id || 1}`)}><div><h3>{name}</h3><p className="muted">Delicioso platillo preparado al momento.</p><b>{price}</b></div><img src={image || DEFAULT_IMAGE} alt={name} onError={handleImageError}/></article>; 
}

export function RestaurantCard({ item }) { 
    const navigate = useNavigate();
    return <article className="restaurant-card" onClick={() => navigate(`/restaurante/${item.idRestaurante || 1}`)}><div className="restaurant-image" style={{ backgroundImage:`url(${item.image || item.imagen || DEFAULT_IMAGE})` }}><span className="rating"><Icon filled>star</Icon>{item.rating || '4.5'}</span></div><div className="card-body"><h3>{item.nombre || item.name}</h3><p><Icon>schedule</Icon> {item.eta || '30 min'} <b>·</b> Envío $25</p><span className="chip">{item.tag || 'Restaurante'}</span></div></article>; 
}
