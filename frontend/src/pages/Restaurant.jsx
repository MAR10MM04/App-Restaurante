import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { Icon, Button, Header, Back, ProductRow } from '../components/ui';
import { getProductImage, getRestaurantImage, productImages } from './Main';
import api from '../services/api';
import { useGeolocation } from '../hooks/useGeolocation';

export function Restaurant() { 
    const { id } = useParams();
    const navigate = useNavigate();
    const [restaurant, setRestaurant] = useState(null);
    const [products, setProducts] = useState([]);

    useEffect(() => {
        if (id) {
            api.get(`/restaurantes/${id}`)
               .then(res => setRestaurant(res.data))
               .catch(err => console.error(err));
               
            api.get('/productos')
               .then(res => {
                   if (res.data) {
                       setProducts(res.data.filter(p => p.idRestaurante == id));
                   }
               })
               .catch(err => console.error(err));
        }
    }, [id]);

    if (!restaurant) return <div className="page"><div style={{padding: 20}}>Cargando restaurante...</div></div>;

    return <div className="page restaurant">
        <div className="restaurant-hero" style={{ backgroundImage:`linear-gradient(0deg,rgba(0,0,0,.5),transparent),url(${getRestaurantImage(restaurant)})` }}>
            <Back />
            <button className="icon-button white" onClick={() => navigate('/carrito')}>
                <Icon>shopping_cart</Icon>
            </button>
        </div>
        <main className="section">
            <p className="eyebrow">{restaurant.nombreCategoria?.toUpperCase() || 'RESTAURANTE'}</p>
            <h1>{restaurant.nombre}</h1>
            <p className="muted">{restaurant.direccion}</p>
            <div className="info-row">
                <span><Icon filled>star</Icon> 4.5</span>
                <span><Icon>delivery_dining</Icon> Envío $25</span>
            </div>
            
            <h2 style={{marginTop: 24}}>Menú</h2>
            {products.length === 0 ? (
                <p>No hay productos disponibles.</p>
            ) : (
                products.map(p => (
                    <ProductRow key={p.idProducto} id={p.idProducto} name={p.nombre} price={`$${p.precio}`} image={getProductImage(p)} fallbackImage={productImages[Math.abs(p.idProducto - 1) % productImages.length]} />
                ))
            )}
        </main>
    </div>; 
}

export function Product() { 
    const { id } = useParams();
    const navigate = useNavigate();
    const [product, setProduct] = useState(null);
    const [quantity, setQuantity] = useState(1);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (id) {
            api.get(`/productos/${id}`)
               .then(res => setProduct(res.data))
               .catch(err => console.error(err));
        }
    }, [id]);

    const addToCart = async () => {
        if (!product) return;
        setLoading(true);
        try {
            const userStr = localStorage.getItem('user');
            if (!userStr) {
                navigate('/login');
                return;
            }
            const user = JSON.parse(userStr);
            
            let cartId = null;
            const cartsRes = await api.get('/carritos');
            const userCart = cartsRes.data.find(c => c.idUsuario === user.idUsuario && c.estado === 'Activo');
            
            if (userCart && userCart.idRestaurante === product.idRestaurante) {
                cartId = userCart.idCarrito;
            } else {
                if (userCart) {
                    const replaceCart = window.confirm(
                        'Tu carrito contiene productos de otro restaurante. ' +
                        '¿Quieres vaciarlo y comenzar un pedido nuevo?'
                    );

                    if (!replaceCart) return;

                    await api.patch(`/carritos/${userCart.idCarrito}/estado`, {
                        estado: 'Abandonado'
                    });
                }

                const newCartRes = await api.post('/carritos', {
                    idUsuario: user.idUsuario,
                    idRestaurante: product.idRestaurante,
                    estado: 'Activo'
                });
                cartId = newCartRes.data.idCarrito;
            }
            
            await api.post('/detallescarrito', {
                idCarrito: cartId,
                idProducto: product.idProducto,
                cantidad: quantity,
                precioUnitario: product.precio
            });
            
            navigate('/carrito');
        } catch (err) {
            console.error(err);
            alert('Error al agregar al carrito. ' + (err.response?.data?.mensaje || ''));
        } finally {
            setLoading(false);
        }
    };

    if (!product) return <div className="page"><Header title="Cargando..."/></div>;

    const handleImageError = (e) => {
        e.currentTarget.onerror = null;
        e.currentTarget.src = productImages[Math.abs(product.idProducto - 1) % productImages.length];
    };

    return <div className="page product">
        <Header title="Detalle del producto"/>
        <main>
            <img className="product-image" src={getProductImage(product)} alt={product.nombre} onError={handleImageError} />
            <section className="section">
                <h1>{product.nombre}</h1>
                <p className="muted">{product.descripcion}</p>
                <h2>${product.precio}</h2>
            </section>
        </main>
        <footer className="purchase-bar">
            <div className="quantity">
                <button onClick={() => setQuantity(Math.max(1,quantity-1))}>−</button>
                <b>{quantity}</b>
                <button onClick={() => setQuantity(quantity+1)}>+</button>
            </div>
            <Button onClick={addToCart} disabled={loading}>
                {loading ? 'Agregando...' : `Agregar · $${(product.precio * quantity).toFixed(2)}`}
            </Button>
        </footer>
    </div>; 
}

export function Cart() { 
    const navigate = useNavigate();
    const [cartItems, setCartItems] = useState([]);
    const [cartId, setCartId] = useState(null);
    const [total, setTotal] = useState(0);

    useEffect(() => {
        loadCart();
    }, []);

    const loadCart = async () => {
        const userStr = localStorage.getItem('user');
        if (!userStr) return navigate('/login');
        const user = JSON.parse(userStr);
        
        try {
            const cartsRes = await api.get('/carritos');
            const activeCart = cartsRes.data.find(c => c.idUsuario === user.idUsuario && c.estado === 'Activo');
            
            if (activeCart) {
                setCartId(activeCart.idCarrito);
                const detailsRes = await api.get('/detallescarrito');
                const items = detailsRes.data.filter(d => d.idCarrito === activeCart.idCarrito);
                setCartItems(items);
                
                const sum = items.reduce((acc, item) => acc + (item.cantidad * item.precioUnitario), 0);
                setTotal(sum);
            }
        } catch (err) {
            console.error(err);
        }
    };

    return <div className="page">
        <Header title="Mi carrito" cart={false}/>
        <main className="section">
            <div className="cart-restaurant">
                <Icon filled>storefront</Icon>
                <div><b>Tu Pedido</b><small>Entrega estimada: 20-30 min</small></div>
            </div>
            
            {cartItems.length === 0 ? (
                <p style={{padding: 20, textAlign: 'center'}}>Tu carrito está vacío</p>
            ) : (
                cartItems.map(item => (
                    <ProductRow key={item.idDetalleCarrito} id={item.idProducto} name={item.producto || `Producto #${item.idProducto}`} price={`$${item.precioUnitario} x ${item.cantidad}`} image={getProductImage(item)} fallbackImage={productImages[Math.abs(item.idProducto - 1) % productImages.length]} />
                ))
            )}
            
            {cartItems.length > 0 && (
                <div className="summary" style={{marginTop: 20}}>
                    <p>Subtotal <b>${total.toFixed(2)}</b></p>
                    <p>Costo de envío <b>$25.00</b></p>
                    <p>Total <b>${(total + 25).toFixed(2)}</b></p>
                </div>
            )}
        </main>
        
        {cartItems.length > 0 && (
            <footer className="purchase-bar">
                <Button onClick={() => navigate('/confirmar-pedido', { state: { cartId, total: total + 25 } })}>
                    Continuar con el pedido
                </Button>
            </footer>
        )}
    </div>; 
}

export function Confirm() { 
    const navigate = useNavigate();
    const location = useLocation();
    const [loading, setLoading] = useState(false);
    const [address, setAddress] = useState(null);
    const [loadingAddress, setLoadingAddress] = useState(true);
    const currentLocation = useGeolocation();
    const syncedLocation = useRef(null);

    useEffect(() => {
        const userStr = localStorage.getItem('user');
        if (!userStr) {
            navigate('/login');
            return;
        }

        const user = JSON.parse(userStr);
        api.get(`/direcciones/usuario/${user.idUsuario}`)
            .then(res => setAddress(res.data?.[0] || null))
            .catch(err => console.error('Error al cargar la dirección:', err))
            .finally(() => setLoadingAddress(false));
    }, [navigate]);

    useEffect(() => {
        if (loadingAddress || currentLocation.loading ||
            currentLocation.latitude == null || currentLocation.longitude == null) return;

        const locationKey = `${currentLocation.latitude},${currentLocation.longitude}`;
        if (syncedLocation.current === locationKey) return;
        syncedLocation.current = locationKey;

        const userStr = localStorage.getItem('user');
        if (!userStr) return;
        const user = JSON.parse(userStr);
        const payload = {
            idUsuario: user.idUsuario,
            colonia: currentLocation.address,
            latitud: currentLocation.latitude,
            longitud: currentLocation.longitude
        };

        const saveLocation = address
            ? api.put(`/direcciones/${address.idDireccion}`, payload)
            : api.post('/direcciones', payload);

        saveLocation
            .then(res => setAddress(res.data))
            .catch(err => {
                syncedLocation.current = null;
                console.error('Error al guardar la ubicación:', err);
            });
    }, [address, loadingAddress, currentLocation]);

    const handlePlaceOrder = async () => {
        if (!address) {
            alert('Necesitas registrar una dirección de entrega antes de realizar el pedido.');
            return;
        }

        setLoading(true);
        try {
            const userStr = localStorage.getItem('user');
            if (!userStr) return navigate('/login');
            const user = JSON.parse(userStr);
            
            const cartsRes = await api.get('/carritos');
            const activeCart = cartsRes.data.find(c => c.idUsuario === user.idUsuario && c.estado === 'Activo');
            
            if (activeCart) {
                const totalAmount = location.state?.total || 150.00;
                const cartRes = await api.get(`/carritos/${activeCart.idCarrito}/detalle`);
                const orderRes = await api.post('/pedidos', {
                    idUsuario: user.idUsuario,
                    idRestaurante: activeCart.idRestaurante,
                    idDireccionEntrega: address.idDireccion,
                    tipoPago: 'Efectivo',
                    total: totalAmount
                });

                for (const item of cartRes.data.detalles) {
                    await api.post('/detallespedido', {
                        idPedido: orderRes.data.idPedido,
                        idProducto: item.idProducto,
                        cantidad: item.cantidad
                    });
                }
                
                await api.patch(`/carritos/${activeCart.idCarrito}/estado`, {
                    estado: 'Procesado'
                });

                localStorage.setItem('lastOrderId', String(orderRes.data.idPedido));
                navigate('/pedido-realizado', { state: { order: orderRes.data } });
            } else {
                alert('No tienes un carrito activo para procesar.');
            }
        } catch (err) {
            console.error(err);
            const apiError = err.response?.data;
            const validationErrors = apiError?.errors
                ? Object.values(apiError.errors).flat().join(' ')
                : '';
            alert('Error al procesar el pedido. ' + (apiError?.mensaje || validationErrors || 'Inténtalo nuevamente.'));
        } finally {
            setLoading(false);
        }
    };

    return <div className="page">
        <Header title="Confirmar pedido" cart={false}/>
        <main className="section">
            <h2>Dirección de entrega</h2>
            <div className="choice">
                <Icon filled>location_on</Icon>
                <span>
                    <b>{loadingAddress || currentLocation.loading ? 'Obteniendo tu ubicación...' : address ? 'Tu ubicación de entrega' : 'Sin dirección registrada'}</b>
                    <small>{currentLocation.error && address ? `${address.colonia} (ubicación guardada)` : address?.colonia || currentLocation.error || 'Permite el acceso a tu ubicación para continuar'}</small>
                </span>
                <Icon>chevron_right</Icon>
            </div>
            <h2>Método de pago</h2>
            <div className="choice">
                <Icon>credit_card</Icon>
                <span><b>Efectivo</b><small>Pago contra entrega</small></span>
                <Icon>chevron_right</Icon>
            </div>
            {location.state?.total && (
                <div className="summary" style={{marginTop: 20}}>
                    <p>Total a pagar <b>${location.state.total.toFixed(2)}</b></p>
                </div>
            )}
        </main>
        <footer className="purchase-bar">
            <Button disabled={loading || loadingAddress || currentLocation.loading || !address} onClick={handlePlaceOrder}>
                {loading ? 'Procesando...' : 'Realizar pedido'}
            </Button>
        </footer>
    </div>; 
}
