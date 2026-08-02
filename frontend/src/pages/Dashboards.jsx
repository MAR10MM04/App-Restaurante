import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon, Button, Header } from '../components/ui';
import api, { mediaUrl } from '../services/api';
import { productImages } from './Main';
import { CircleMarker, MapContainer, Popup, TileLayer, useMap } from 'react-leaflet';
import { useGeolocation } from '../hooks/useGeolocation';

const defaultMapPosition = [18.1858933, -91.0427157];

function mapDistanceInKm([lat1, lng1], [lat2, lng2]) {
    const radians = value => value * Math.PI / 180;
    const latDistance = radians(lat2 - lat1);
    const lngDistance = radians(lng2 - lng1);
    const value = Math.sin(latDistance / 2) ** 2 +
        Math.cos(radians(lat1)) * Math.cos(radians(lat2)) * Math.sin(lngDistance / 2) ** 2;
    return 6371 * 2 * Math.atan2(Math.sqrt(value), Math.sqrt(1 - value));
}

function FitDeliveryMap({ positions }) {
    const map = useMap();
    useEffect(() => {
        if (positions.length === 1) map.setView(positions[0], 16);
        if (positions.length > 1) map.fitBounds(positions, { padding: [35, 35], maxZoom: 16 });
    }, [map, positions]);
    return null;
}

const nextOrderState = {
    Pendiente: ['Confirmado', 'Confirmar'],
    Confirmado: ['Preparando', 'Comenzar preparación'],
    Preparando: ['Listo', 'Marcar como listo']
};

export function OwnerDashboard() {
    const navigate = useNavigate();
    const [restaurants, setRestaurants] = useState([]);
    const [restaurantId, setRestaurantId] = useState(null);
    const [orders, setOrders] = useState([]);
    const [products, setProducts] = useState([]);
    const [categories, setCategories] = useState([]);
    const [tab, setTab] = useState('pedidos');
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState('');
    const [form, setForm] = useState({
        nombre: '', descripcion: '', precio: '', idCategoria: '', imagen: null
    });

    useEffect(() => {
        const user = JSON.parse(localStorage.getItem('user') || 'null');
        if (!user) return navigate('/login');

        Promise.all([
            api.get(`/restaurantes/propietario/${user.idUsuario}`),
            api.get('/categorias')
        ]).then(([restaurantRes, categoryRes]) => {
            const ownedRestaurants = restaurantRes.data || [];
            setRestaurants(ownedRestaurants);
            setCategories(categoryRes.data || []);
            if (ownedRestaurants.length) {
                const savedId = Number(localStorage.getItem('ownerRestaurantId'));
                const selected = ownedRestaurants.find(item => item.idRestaurante === savedId) || ownedRestaurants[0];
                setRestaurantId(selected.idRestaurante);
            } else {
                setLoading(false);
            }
        }).catch(err => {
            setError(err.response?.data?.mensaje || 'No se pudo cargar tu restaurante.');
            setLoading(false);
        });
    }, [navigate]);

    const loadRestaurantData = async id => {
        setLoading(true);
        setError('');
        try {
            const [ordersRes, productsRes] = await Promise.all([
                api.get(`/pedidos/restaurante/${id}`),
                api.get(`/productos/restaurante/${id}`)
            ]);
            setOrders(ordersRes.data || []);
            setProducts(productsRes.data || []);
            localStorage.setItem('ownerRestaurantId', String(id));
        } catch (err) {
            setError(err.response?.data?.mensaje || 'No se pudo cargar la información del panel.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (restaurantId) loadRestaurantData(restaurantId);
    }, [restaurantId]);

    const createProduct = async event => {
        event.preventDefault();
        setSaving(true);
        setError('');
        try {
            const data = new FormData();
            data.append('IdRestaurante', restaurantId);
            data.append('IdCategoria', form.idCategoria);
            data.append('Nombre', form.nombre);
            data.append('Descripcion', form.descripcion);
            data.append('Precio', form.precio);
            data.append('Disponible', 'true');
            if (form.imagen) data.append('Imagen', form.imagen);

            await api.post('/productos/form', data, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            setForm({ nombre: '', descripcion: '', precio: '', idCategoria: '', imagen: null });
            await loadRestaurantData(restaurantId);
        } catch (err) {
            const apiError = err.response?.data;
            setError(apiError?.mensaje || Object.values(apiError?.errors || {}).flat().join(' ') || 'No se pudo crear el producto.');
        } finally {
            setSaving(false);
        }
    };

    const deleteProduct = async product => {
        if (!window.confirm(`¿Eliminar “${product.nombre}”?`)) return;
        try {
            await api.delete(`/productos/${product.idProducto}`);
            setProducts(current => current.filter(item => item.idProducto !== product.idProducto));
        } catch (err) {
            setError(err.response?.data?.mensaje || 'No se pudo eliminar el producto. Si ya tiene pedidos, márcalo como no disponible.');
        }
    };

    const advanceOrder = async order => {
        const next = nextOrderState[order.estado];
        if (!next) return;
        try {
            await api.patch(`/pedidos/${order.idPedido}/estado`, { estado: next[0] });
            setOrders(current => current.map(item =>
                item.idPedido === order.idPedido ? { ...item, estado: next[0] } : item
            ));
        } catch (err) {
            setError(err.response?.data?.mensaje || 'No se pudo actualizar el pedido.');
        }
    };

    const selectedRestaurant = restaurants.find(item => item.idRestaurante === restaurantId);

    if (!loading && restaurants.length === 0) return <div className="page"><Header title="Panel de restaurante" cart={false}/><main className="section center"><h2>Aún no tienes un restaurante</h2><p className="muted">Registra uno para comenzar a recibir pedidos y publicar productos.</p><Button onClick={() => navigate('/registro-restaurante')}>Registrar restaurante</Button></main></div>;

    return <div className="page owner-dashboard">
        <Header title="Panel de restaurante" cart={false}/>
        <main className="section">
            <div className="owner-heading">
                <div><p className="eyebrow">ADMINISTRACIÓN</p><h1>{selectedRestaurant?.nombre || 'Mi restaurante'}</h1><p className="muted">{selectedRestaurant?.direccion}</p></div>
                {restaurants.length > 1 && <select value={restaurantId || ''} onChange={event => setRestaurantId(Number(event.target.value))}>{restaurants.map(item => <option key={item.idRestaurante} value={item.idRestaurante}>{item.nombre}</option>)}</select>}
            </div>

            <div className="owner-stats">
                <div><Icon>receipt_long</Icon><b>{orders.length}</b><span>Pedidos</span></div>
                <div><Icon>restaurant_menu</Icon><b>{products.length}</b><span>Productos</span></div>
                <div><Icon>payments</Icon><b>${orders.reduce((sum, order) => sum + Number(order.total), 0).toFixed(2)}</b><span>Ventas</span></div>
            </div>

            <nav className="owner-tabs">
                <button className={tab === 'pedidos' ? 'active' : ''} onClick={() => setTab('pedidos')}>Pedidos</button>
                <button className={tab === 'productos' ? 'active' : ''} onClick={() => setTab('productos')}>Productos</button>
                <button className={tab === 'agregar' ? 'active' : ''} onClick={() => setTab('agregar')}>Agregar producto</button>
            </nav>

            {error && <p className="dashboard-error">{error}</p>}
            {loading && <p className="muted">Cargando información...</p>}

            {!loading && tab === 'pedidos' && <section>
                <h2>Pedidos recibidos</h2>
                {orders.length === 0 && <p className="muted">Todavía no hay pedidos para este restaurante.</p>}
                <div className="dashboard-list">{orders.map(order => <article className="dashboard-card" key={order.idPedido}>
                    <div className="dashboard-card-title"><div><b>{order.numeroPedido || `Pedido #${order.idPedido}`}</b><small>{new Date(order.fechaPedido).toLocaleString('es-MX')}</small></div><span className="chip">{order.estado}</span></div>
                    <p>Cliente: <b>{order.usuario}</b></p><p>Total: <b>${Number(order.total).toFixed(2)}</b></p>
                    {nextOrderState[order.estado] && <Button onClick={() => advanceOrder(order)}>{nextOrderState[order.estado][1]}</Button>}
                </article>)}</div>
            </section>}

            {!loading && tab === 'productos' && <section>
                <div className="section-title"><h2>Productos publicados</h2><button className="text-button" onClick={() => setTab('agregar')}>+ Agregar</button></div>
                {products.length === 0 && <p className="muted">Aún no has agregado productos.</p>}
                <div className="dashboard-products">{products.map((product, index) => <article className="dashboard-product" key={product.idProducto}>
                    <img src={mediaUrl(product.imagen) || productImages[index % productImages.length]} alt={product.nombre}/>
                    <div><b>{product.nombre}</b><small>{product.categoria}</small><span>${Number(product.precio).toFixed(2)}</span></div>
                    <button className="delete-product" onClick={() => deleteProduct(product)} aria-label={`Eliminar ${product.nombre}`}><Icon>delete</Icon></button>
                </article>)}</div>
            </section>}

            {!loading && tab === 'agregar' && <section className="product-form-panel">
                <h2>Agregar producto</h2>
                <form className="fields" onSubmit={createProduct}>
                    <label>Nombre<input required maxLength="150" value={form.nombre} onChange={event => setForm({...form, nombre: event.target.value})} placeholder="Ej. Hamburguesa especial"/></label>
                    <label>Categoría<select required value={form.idCategoria} onChange={event => setForm({...form, idCategoria: event.target.value})}><option value="">Selecciona una categoría</option>{categories.map(category => <option key={category.idCategoria} value={category.idCategoria}>{category.nombre}</option>)}</select></label>
                    <label>Descripción<textarea maxLength="500" value={form.descripcion} onChange={event => setForm({...form, descripcion: event.target.value})} placeholder="Ingredientes y descripción"/></label>
                    <label>Precio<input required type="number" min="0.01" step="0.01" value={form.precio} onChange={event => setForm({...form, precio: event.target.value})} placeholder="0.00"/></label>
                    <label>Imagen del producto<input type="file" accept="image/*" onChange={event => setForm({...form, imagen: event.target.files?.[0] || null})}/></label>
                    <Button disabled={saving}>{saving ? 'Guardando...' : 'Publicar producto'}</Button>
                </form>
            </section>}
        </main>
    </div>;
}

export function DriverDashboard() {
    const navigate = useNavigate();
    const location = useGeolocation();
    const [driver, setDriver] = useState(null);
    const [availableOrders, setAvailableOrders] = useState([]);
    const [myOrders, setMyOrders] = useState([]);
    const [activeOrder, setActiveOrder] = useState(null);
    const [orderDetail, setOrderDetail] = useState(null);
    const [tab, setTab] = useState('disponibles');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const loadDriverData = async () => {
        const user = JSON.parse(localStorage.getItem('user') || 'null');
        if (!user) return navigate('/login');
        try {
            const driverRes = await api.get(`/repartidores/usuario/${user.idUsuario}`);
            const profile = driverRes.data;
            setDriver(profile);
            const [allOrdersRes, ownOrdersRes] = await Promise.all([
                api.get('/pedidos'),
                api.get(`/pedidos/repartidor/${profile.idRepartidor}`)
            ]);
            const ownOrders = ownOrdersRes.data || [];
            setAvailableOrders((allOrdersRes.data || []).filter(order => order.estado === 'Listo' && !order.idRepartidor));
            setMyOrders(ownOrders);
            const current = ownOrders.find(order => !['Entregado', 'Cancelado'].includes(order.estado));
            setActiveOrder(current || null);
            if (current) {
                const detailRes = await api.get(`/pedidos/${current.idPedido}/detalle`);
                setOrderDetail(detailRes.data);
                setTab('activa');
            }
        } catch (err) {
            if (err.response?.status === 404) return navigate('/registro-repartidor');
            setError(err.response?.data?.mensaje || 'No se pudo cargar el panel de repartidor.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { loadDriverData(); }, []);

    const acceptOrder = async order => {
        setError('');
        try {
            await api.patch(`/pedidos/${order.idPedido}/repartidor`, { idRepartidor: driver.idRepartidor });
            await api.patch(`/pedidos/${order.idPedido}/estado`, { estado: 'En camino' });
            await api.patch(`/repartidores/${driver.idRepartidor}/ocupado`);
            const detailRes = await api.get(`/pedidos/${order.idPedido}/detalle`);
            setActiveOrder({ ...order, estado: 'En camino', idRepartidor: driver.idRepartidor });
            setOrderDetail(detailRes.data);
            setAvailableOrders(current => current.filter(item => item.idPedido !== order.idPedido));
            setTab('activa');
        } catch (err) {
            setError(err.response?.data?.mensaje || 'No se pudo aceptar esta entrega.');
        }
    };

    const completeDelivery = async () => {
        try {
            await api.patch(`/pedidos/${activeOrder.idPedido}/estado`, { estado: 'Entregado' });
            const [driverRes, ordersRes] = await Promise.all([
                api.get(`/repartidores/${driver.idRepartidor}`),
                api.get(`/pedidos/repartidor/${driver.idRepartidor}`)
            ]);
            const updatedOrders = ordersRes.data || [];
            const nextActiveOrder = updatedOrders.find(order =>
                !['Entregado', 'Cancelado'].includes(order.estado)
            );
            setDriver(driverRes.data);
            setMyOrders(updatedOrders);

            if (nextActiveOrder) {
                const detailRes = await api.get(`/pedidos/${nextActiveOrder.idPedido}/detalle`);
                setActiveOrder(nextActiveOrder);
                setOrderDetail(detailRes.data);
                setError('La entrega fue completada. Todavía tienes otra entrega activa asignada.');
                setTab('activa');
            } else {
                setActiveOrder(null);
                setOrderDetail(null);
                setTab('historial');
            }
        } catch (err) {
            setError(err.response?.data?.mensaje || 'No se pudo completar la entrega.');
        }
    };

    const deliveredOrders = myOrders.filter(order => order.estado === 'Entregado');
    const earnings = deliveredOrders.length * 25;
    const driverPosition = location.latitude != null ? [location.latitude, location.longitude] : defaultMapPosition;
    const customerPosition = orderDetail?.direccionEntrega
        ? [orderDetail.direccionEntrega.latitud, orderDetail.direccionEntrega.longitud]
        : defaultMapPosition;
    const savedRestaurantPosition = orderDetail?.restaurante?.latitud != null
        ? [orderDetail.restaurante.latitud, orderDetail.restaurante.longitud]
        : null;
    const restaurantPosition = savedRestaurantPosition &&
        mapDistanceInKm(customerPosition, savedRestaurantPosition) <= 30
        ? savedRestaurantPosition
        : [customerPosition[0] + 0.01, customerPosition[1] - 0.008];
    const deliveryPositions = [driverPosition, restaurantPosition, customerPosition];

    return <div className="page driver-dashboard"><Header title="Panel de repartidor" cart={false}/><main className="section">
        <div className="owner-heading"><div><p className="eyebrow">REPARTIDOR</p><h1>{driver?.nombre || 'Mis entregas'}</h1><p className="muted">{location.loading ? 'Obteniendo tu ubicación...' : location.error ? 'Ubicación predeterminada activa' : 'GPS activo'}</p></div><span className="chip">{driver?.estado}</span></div>
        <div className="owner-stats"><div><Icon>two_wheeler</Icon><b>{deliveredOrders.length}</b><span>Entregas</span></div><div><Icon>payments</Icon><b>${earnings.toFixed(2)}</b><span>Ganancias</span></div><div><Icon>star</Icon><b>{deliveredOrders.filter(order => order.calificacionRepartidor).length}</b><span>Calificaciones</span></div></div>
        <nav className="owner-tabs"><button className={tab === 'disponibles' ? 'active' : ''} onClick={() => setTab('disponibles')}>Disponibles</button><button className={tab === 'activa' ? 'active' : ''} onClick={() => setTab('activa')}>Entrega activa</button><button className={tab === 'historial' ? 'active' : ''} onClick={() => setTab('historial')}>Historial</button><button className={tab === 'ganancias' ? 'active' : ''} onClick={() => setTab('ganancias')}>Ganancias y comentarios</button></nav>
        {error && <p className="dashboard-error">{error}</p>}
        {loading && <p>Buscando entregas...</p>}

        {!loading && tab === 'disponibles' && <section><h2>Pedidos listos para entregar</h2>{activeOrder && <p className="dashboard-error">Termina tu entrega activa antes de aceptar otra.</p>}{availableOrders.length === 0 ? <p className="muted">No hay entregas disponibles en este momento.</p> : <div className="dashboard-list">{availableOrders.map(order => <article key={order.idPedido} className="dashboard-card"><div className="dashboard-card-title"><b>{order.numeroPedido}</b><span className="chip">{order.estado}</span></div><p><Icon>storefront</Icon> {order.restaurante}</p><p>Pedido: <b>${Number(order.total).toFixed(2)}</b></p><p>Ganancia estimada: <b>$25.00</b></p><Button disabled={Boolean(activeOrder)} onClick={() => acceptOrder(order)}>Aceptar entrega</Button></article>)}</div>}</section>}

        {!loading && tab === 'activa' && <section>{!activeOrder ? <p className="muted">No tienes una entrega activa.</p> : <><h2>{activeOrder.numeroPedido}</h2><div className="driver-map"><MapContainer center={driverPosition} zoom={15} style={{height:'100%',width:'100%'}}><TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" attribution='&copy; OpenStreetMap'/><FitDeliveryMap positions={deliveryPositions}/><CircleMarker center={driverPosition} radius={10} pathOptions={{color:'#fff',weight:4,fillColor:'#15803d',fillOpacity:1}}><Popup>Tu ubicación actual</Popup></CircleMarker><CircleMarker center={restaurantPosition} radius={10} pathOptions={{color:'#fff',weight:4,fillColor:'#ff7a00',fillOpacity:1}}><Popup>{orderDetail?.restaurante?.nombre}<br/>{orderDetail?.restaurante?.direccion}</Popup></CircleMarker><CircleMarker center={customerPosition} radius={10} pathOptions={{color:'#fff',weight:4,fillColor:'#1677ff',fillOpacity:1}}><Popup>Cliente<br/>{orderDetail?.direccionEntrega?.colonia}</Popup></CircleMarker></MapContainer></div><div className="delivery-legend"><span>🛵 Tú</span><span>🍽️ Restaurante</span><span>🏠 Cliente</span></div><article className="dashboard-card"><p><b>Recoger en:</b> {orderDetail?.restaurante?.direccion}</p><p><b>Entregar en:</b> {orderDetail?.direccionEntrega?.colonia}</p><Button onClick={completeDelivery}>Confirmar pedido entregado</Button></article></>}</section>}

        {!loading && tab === 'historial' && <section><h2>Historial de entregas</h2>{deliveredOrders.length === 0 ? <p className="muted">Aún no tienes entregas terminadas.</p> : <div className="dashboard-list">{deliveredOrders.map(order => <article className="dashboard-card" key={order.idPedido}><div className="dashboard-card-title"><b>{order.numeroPedido}</b><span className="chip">Entregado</span></div><p>{order.restaurante}</p><p>{new Date(order.fechaPedido).toLocaleString('es-MX')}</p><b>Ganancia: $25.00</b></article>)}</div>}</section>}

        {!loading && tab === 'ganancias' && <section><h2>Ganancias</h2><article className="earnings-card"><small>Total acumulado</small><strong>${earnings.toFixed(2)}</strong><p>$25.00 por cada entrega completada.</p></article><h2>Comentarios y calificaciones</h2>{deliveredOrders.filter(order => order.calificacionRepartidor).length === 0 ? <p className="muted">Todavía no has recibido calificaciones.</p> : deliveredOrders.filter(order => order.calificacionRepartidor).map(order => <article className="dashboard-card" key={order.idPedido}><b>{'★'.repeat(order.calificacionRepartidor)}{'☆'.repeat(5-order.calificacionRepartidor)}</b><p>{order.numeroPedido} · {order.restaurante}</p><small className="muted">El sistema actual guarda la calificación numérica del cliente.</small></article>)}</section>}
    </main></div>;
}
