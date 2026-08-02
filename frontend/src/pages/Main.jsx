import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon, Button, Header, BottomNav, RestaurantCard } from '../components/ui';
import api, { mediaUrl } from '../services/api';
import { useGeolocation } from '../hooks/useGeolocation';

export const foodImage = 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=1200&q=85';
export const pizzaImage = 'https://images.unsplash.com/photo-1574071318508-1cdbab80d002?auto=format&fit=crop&w=900&q=85';
export const sushiImage = 'https://images.unsplash.com/photo-1579871494447-9811cf80d66c?auto=format&fit=crop&w=900&q=85';
export const restaurantImages = [
    pizzaImage,
    sushiImage,
    'https://images.unsplash.com/photo-1515003197210-e0cd71810b5f?auto=format&fit=crop&w=1200&q=85',
    'https://images.unsplash.com/photo-1552566626-52f8b828add9?auto=format&fit=crop&w=1200&q=85',
    'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=1200&q=85'
];
export const productImages = [
    'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=900&q=85',
    'https://images.unsplash.com/photo-1553621042-f6e147245754?auto=format&fit=crop&w=900&q=85',
    'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=900&q=85',
    'https://images.unsplash.com/photo-1578985545062-69928b1d9587?auto=format&fit=crop&w=900&q=85',
    'https://images.unsplash.com/photo-1544145945-f90425340c7e?auto=format&fit=crop&w=900&q=85'
];

export const getRestaurantImage = restaurant =>
    mediaUrl(restaurant?.imagen) || restaurantImages[Math.abs((restaurant?.idRestaurante || 1) - 1) % restaurantImages.length];

export const getProductImage = product =>
    mediaUrl(product?.imagen) || productImages[Math.abs((product?.idProducto || 1) - 1) % productImages.length];

export const defaultCategories = [['lunch_dining','Hamburguesas'],['local_pizza','Pizza'],['set_meal','Sushi'],['icecream','Postres'],['local_bar','Bebidas']];

export function Home() { 
    const navigate = useNavigate();
    const location = useGeolocation();
    const [restaurantsData, setRestaurantsData] = useState([]);
    const [categoriesData, setCategoriesData] = useState(defaultCategories);
    
    useEffect(() => {
        api.get('/restaurantes')
           .then(res => {
               if (res.data && res.data.length > 0) {
                   setRestaurantsData(res.data);
               }
           })
           .catch(err => console.error("Error fetching restaurants:", err));
           
        api.get('/categorias')
           .then(res => {
               if (res.data && res.data.length > 0) {
                   setCategoriesData(res.data.map(c => ['category', c.nombre]));
               }
           })
           .catch(err => console.error("Error fetching categories:", err));
    }, []);

    return <div className="page home">
        <header className="home-header">
            <div>
                <Icon filled>location_on</Icon> 
                {location.loading ? 'Obteniendo ubicación...' : location.address}
            </div>
            <button className="icon-button" onClick={() => navigate('/carrito')}>
                <Icon>shopping_cart</Icon>
            </button>
            <button className="search-box" onClick={() => navigate('/buscar')}>
                <Icon>search</Icon> ¿Qué se te antoja hoy?
            </button>
        </header>
        <main>
            <section className="category-strip">
                {categoriesData.slice(0, 5).map(([icon,name]) => 
                    <button key={name} onClick={() => navigate('/categorias')}>
                        <span><Icon>{icon}</Icon></span>{name}
                    </button>
                )}
            </section>
            <section className="promo" style={{ backgroundImage:`linear-gradient(90deg,rgba(0,0,0,.65),transparent),url(${foodImage})` }}>
                <div>
                    <p>OFERTA DEL DÍA</p>
                    <h1>50% OFF en Burger King</h1>
                    <Button onClick={() => navigate('/restaurante/1')}>Pedir ahora</Button>
                </div>
            </section>
            <section className="section">
                <div className="section-title">
                    <div>
                        <h2>Restaurantes recomendados</h2>
                        <p>Cerca de tu ubicación</p>
                    </div>
                    <button className="text-button" onClick={() => navigate('/buscar')}>Ver todo</button>
                </div>
                <div className="restaurant-grid">
                    {restaurantsData.map(item => <RestaurantCard item={{...item, image: getRestaurantImage(item), tag: item.nombreCategoria || 'Restaurante', rating: '4.5'}} key={item.idRestaurante} />)}
                </div>
            </section>
        </main>
        <BottomNav active="home" />
    </div>; 
}

export function Search() { 
    const navigate = useNavigate();
    const [query, setQuery] = useState(''); 
    const [allRestaurants, setAllRestaurants] = useState([]);
    
    useEffect(() => {
        api.get('/restaurantes')
           .then(res => {
               if (res.data) setAllRestaurants(res.data);
           })
           .catch(err => console.error(err));
    }, []);

    const list = allRestaurants.filter(r => r.nombre.toLowerCase().includes(query.toLowerCase())); 
    
    return <div className="page">
        <Header title="Buscar" />
        <main className="section">
            <label className="search-input">
                <Icon>search</Icon>
                <input autoFocus value={query} onChange={e => setQuery(e.target.value)} placeholder="Busca restaurantes o platillos" />
            </label>
            <h2>Explora cerca de ti</h2>
            <div className="restaurant-grid">
                {list.map(item => <RestaurantCard item={{...item, image: getRestaurantImage(item), tag: item.nombreCategoria || 'Restaurante', rating: '4.5'}} key={item.idRestaurante} />)}
            </div>
        </main>
        <BottomNav active="buscar" />
    </div>; 
}

export function Categories() { 
    const navigate = useNavigate();
    const [categoriesData, setCategoriesData] = useState([]);
    
    useEffect(() => {
        api.get('/categorias')
           .then(res => {
               if (res.data) setCategoriesData(res.data);
           })
           .catch(err => console.error(err));
    }, []);

    return <div className="page">
        <Header title="Categorías" />
        <main className="section">
            <h2>¿Qué se te antoja?</h2>
            <div className="category-grid">
                {categoriesData.map((cat) => 
                    <button key={cat.idCategoria} onClick={() => navigate('/buscar')}>
                        <span><Icon>category</Icon></span>
                        <b>{cat.nombre}</b>
                        <small>{cat.descripcion || 'Restaurantes cerca de ti'}</small>
                    </button>
                )}
            </div>
        </main>
        <BottomNav active="buscar" />
    </div>; 
}
