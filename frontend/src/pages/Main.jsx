import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon, Button, Header, BottomNav, RestaurantCard } from '../components/ui';

export const foodImage = 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=1200&q=85';
export const pizzaImage = 'https://images.unsplash.com/photo-1574071318508-1cdbab80d002?auto=format&fit=crop&w=900&q=85';
export const sushiImage = 'https://images.unsplash.com/photo-1579871494447-9811cf80d66c?auto=format&fit=crop&w=900&q=85';

export const categories = [['lunch_dining','Hamburguesas'],['local_pizza','Pizza'],['set_meal','Sushi'],['icecream','Postres'],['local_bar','Bebidas']];
export const restaurants = [{name:'Pizzeria Napoli Centrale', image:pizzaImage, eta:'25-35 min', tag:'Pizza', rating:'4.8'}, {name:'Sushi Zen Master', image:sushiImage, eta:'40-50 min', tag:'Sushi', rating:'4.9'}, {name:'Burger King', image:foodImage, eta:'20-30 min', tag:'Hamburguesas', rating:'4.7'}];

export function Home() { 
    const navigate = useNavigate();
    return <div className="page home"><header className="home-header"><div><Icon filled>location_on</Icon> Av. Paseo de la Reforma 222</div><button className="icon-button" onClick={() => navigate('/carrito')}><Icon>shopping_cart</Icon></button><button className="search-box" onClick={() => navigate('/buscar')}><Icon>search</Icon> ¿Qué se te antoja hoy?</button></header><main><section className="category-strip">{categories.map(([icon,name]) => <button key={name} onClick={() => navigate('/categorias')}><span><Icon>{icon}</Icon></span>{name}</button>)}</section><section className="promo" style={{ backgroundImage:`linear-gradient(90deg,rgba(0,0,0,.65),transparent),url(${foodImage})` }}><div><p>OFERTA DEL DÍA</p><h1>50% OFF en Burger King</h1><Button onClick={() => navigate('/restaurante')}>Pedir ahora</Button></div></section><section className="section"><div className="section-title"><div><h2>Restaurantes recomendados</h2><p>Basado en tus pedidos anteriores</p></div><button className="text-button" onClick={() => navigate('/buscar')}>Ver todo</button></div><div className="restaurant-grid">{restaurants.map(item => <RestaurantCard item={item} key={item.name} />)}</div></section></main><BottomNav active="home" /></div>; 
}

export function Search() { 
    const navigate = useNavigate();
    const [query, setQuery] = useState(''); 
    const list = restaurants.filter(r => r.name.toLowerCase().includes(query.toLowerCase()) || r.tag.toLowerCase().includes(query.toLowerCase())); 
    return <div className="page"><Header title="Buscar" /><main className="section"><label className="search-input"><Icon>search</Icon><input autoFocus value={query} onChange={e => setQuery(e.target.value)} placeholder="Busca restaurantes o platillos" /></label><h2>Explora cerca de ti</h2><div className="restaurant-grid">{list.map(item => <RestaurantCard item={item} key={item.name} />)}</div></main><BottomNav active="buscar" /></div>; 
}

export function Categories() { 
    const navigate = useNavigate();
    return <div className="page"><Header title="Categorías" /><main className="section"><h2>¿Qué se te antoja?</h2><div className="category-grid">{categories.concat([['ramen_dining','Comida mexicana'],['bakery_dining','Panadería'],['coffee','Café']]).map(([icon,name]) => <button key={name} onClick={() => navigate('/buscar')}><span><Icon>{icon}</Icon></span><b>{name}</b><small>Restaurantes cerca de ti</small></button>)}</div></main><BottomNav active="buscar" /></div>; 
}
