import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Icon, Button, Header, Back, BottomNav } from '../components/ui';
import { foodImage } from './Main';
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import api from '../services/api';
import L from 'leaflet';

delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

const mapIcon = (emoji, color) => L.divIcon({
    className: '',
    html: `<div style="width:38px;height:38px;border-radius:50%;background:${color};border:3px solid white;box-shadow:0 2px 8px #0005;display:grid;place-items:center;font-size:20px">${emoji}</div>`,
    iconSize: [38, 38],
    iconAnchor: [19, 19]
});

const customerIcon = mapIcon('🏠', '#1677ff');
const restaurantIcon = mapIcon('🍽️', '#ff7a00');
const driverIcon = mapIcon('🛵', '#15803d');
const requestedCustomerPosition = [18.1858933, -91.0427157];

function distanceInKm([lat1, lng1], [lat2, lng2]) {
    const radians = value => value * Math.PI / 180;
    const earthRadius = 6371;
    const latDistance = radians(lat2 - lat1);
    const lngDistance = radians(lng2 - lng1);
    const value = Math.sin(latDistance / 2) ** 2 +
        Math.cos(radians(lat1)) * Math.cos(radians(lat2)) * Math.sin(lngDistance / 2) ** 2;
    return earthRadius * 2 * Math.atan2(Math.sqrt(value), Math.sqrt(1 - value));
}

function FitMapToMarkers({ positions }) {
    const map = useMap();

    useEffect(() => {
        if (positions.length === 1) map.setView(positions[0], 15);
        if (positions.length > 1) map.fitBounds(positions, { padding: [45, 45], maxZoom: 15 });
    }, [map, positions]);

    return null;
}

export function OrderPlaced() {
    const navigate = useNavigate();
    const routeLocation = useLocation();
    const order = routeLocation.state?.order;
    const orderNumber = order?.numeroPedido || order?.idPedido || localStorage.getItem('lastOrderId');

    return <main className="status-screen"><div className="success-icon"><Icon filled>check</Icon></div><h1>¡Pedido realizado!</h1><p className="muted">Tu pedido fue enviado al restaurante. Te avisaremos cuando salga rumbo a ti.</p><div className="order-number">Pedido #{orderNumber}</div><Button onClick={() => navigate('/seguimiento', { state: { orderId: order?.idPedido } })}>Seguir mi pedido</Button><Button secondary onClick={() => navigate('/home')}>Volver al inicio</Button></main>;
}

export function Tracking() {
    const navigate = useNavigate();
    const routeLocation = useLocation();
    const [order, setOrder] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        let cancelled = false;

        const loadOrder = async () => {
            try {
                let orderId = routeLocation.state?.orderId || localStorage.getItem('lastOrderId');
                if (!orderId) {
                    const user = JSON.parse(localStorage.getItem('user') || 'null');
                    if (!user) return navigate('/login');
                    const ordersRes = await api.get(`/pedidos?idUsuario=${user.idUsuario}`);
                    orderId = ordersRes.data?.[0]?.idPedido;
                }
                if (!orderId) throw new Error('No se encontró un pedido para mostrar.');

                const response = await api.get(`/pedidos/${orderId}/detalle`);
                if (!cancelled) {
                    setOrder(response.data);
                    setError('');
                }
            } catch (err) {
                if (!cancelled) setError(err.response?.data?.mensaje || err.message || 'No se pudo cargar el seguimiento.');
            } finally {
                if (!cancelled) setLoading(false);
            }
        };

        loadOrder();
        const refresh = window.setInterval(loadOrder, 15000);
        return () => {
            cancelled = true;
            window.clearInterval(refresh);
        };
    }, [navigate, routeLocation.state]);

    const customerPosition = requestedCustomerPosition;
    const savedRestaurantPosition = order?.restaurante?.latitud != null && order?.restaurante?.longitud != null
        ? [order.restaurante.latitud, order.restaurante.longitud]
        : null;
    const restaurantPosition = savedRestaurantPosition && distanceInKm(customerPosition, savedRestaurantPosition) <= 30
        ? savedRestaurantPosition
        : [customerPosition[0] + 0.012, customerPosition[1] - 0.009];
    const driverPosition = customerPosition && restaurantPosition
        ? [
            restaurantPosition[0] + (customerPosition[0] - restaurantPosition[0]) * 0.45,
            restaurantPosition[1] + (customerPosition[1] - restaurantPosition[1]) * 0.45
        ]
        : null;
    const positions = [customerPosition, restaurantPosition, driverPosition].filter(Boolean);

    return <div className="page tracking">
        <div className="map" style={{ height: '50vh', position: 'relative', zIndex: 0 }}>
            <div style={{ position: 'absolute', top: 16, left: 16, zIndex: 1000 }}><Back to="/home" /></div>
            {loading ? (
                <div style={{display:'flex',justifyContent:'center',alignItems:'center',height:'100%'}}>Cargando mapa...</div>
            ) : error || positions.length === 0 ? (
                <div style={{display:'flex',justifyContent:'center',alignItems:'center',height:'100%',padding:24,textAlign:'center'}}>{error || 'El pedido no tiene coordenadas para mostrar.'}</div>
            ) : (
                <MapContainer center={positions[0]} zoom={14} style={{ height: '100%', width: '100%' }} zoomControl={false}>
                    <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>' />
                    <FitMapToMarkers positions={positions} />
                    <Marker position={customerPosition} icon={customerIcon}><Popup><b>Tu ubicación</b><br />18.1858933, -91.0427157</Popup></Marker>
                    {restaurantPosition && <Marker position={restaurantPosition} icon={restaurantIcon}><Popup><b>{order.restaurante.nombre}</b><br />{order.restaurante.direccion}</Popup></Marker>}
                    {driverPosition && <Marker position={driverPosition} icon={driverIcon}><Popup><b>{order.repartidor?.nombre || 'Repartidor por asignar'}</b><br />Posición estimada en ruta</Popup></Marker>}
                </MapContainer>
            )}
        </div>
        <main className="tracking-sheet" style={{ position: 'relative', zIndex: 10 }}>
            <span className="handle"/>
            <p className="eyebrow">LLEGARÁ EN 18-25 MIN</p>
            <h1>Tu pedido está en camino</h1>
            <p className="muted">{order?.repartidor?.nombre || 'Tu repartidor'} está recogiendo tu pedido en {order?.restaurante?.nombre || 'el restaurante'}.</p>
            {driverPosition && <small className="muted">La ubicación del repartidor es estimada hasta habilitar GPS en su aplicación.</small>}
            <div className="timeline">
                <p className="done"><Icon filled>check_circle</Icon> Pedido confirmado</p>
                <p className="done"><Icon filled>check_circle</Icon> Restaurante preparando</p>
                <p><Icon>radio_button_unchecked</Icon> Repartidor en camino</p>
                <p><Icon>radio_button_unchecked</Icon> Pedido entregado</p>
            </div>
            <Button onClick={() => navigate('/pedido-entregado')}>Simular entrega</Button>
        </main>
    </div>;
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
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const user = JSON.parse(localStorage.getItem('user') || 'null');
        if (!user) {
            navigate('/login');
            return;
        }

        api.get(`/pedidos/usuario/${user.idUsuario}`)
            .then(response => setOrders(response.data || []))
            .catch(err => setError(err.response?.data?.mensaje || 'No se pudieron cargar tus pedidos.'))
            .finally(() => setLoading(false));
    }, [navigate]);

    const formatDate = value => new Intl.DateTimeFormat('es-MX', {
        day: 'numeric',
        month: 'short',
        hour: 'numeric',
        minute: '2-digit'
    }).format(new Date(value));

    return <div className="page">
        <Header title="Mis pedidos" cart={false}/>
        <main className="section">
            <h2>Pedidos recientes</h2>
            {loading && <p className="muted">Cargando tus pedidos...</p>}
            {error && <p style={{color: 'red'}}>{error}</p>}
            {!loading && !error && orders.length === 0 && <p className="muted">Todavía no has realizado pedidos.</p>}
            {orders.map(order => <article className="order-card" key={order.idPedido}>
                <img src={foodImage} alt={order.restaurante}/>
                <div>
                    <h3>{order.restaurante}</h3>
                    <p>{formatDate(order.fechaPedido)} · {order.estado}</p>
                    <b>${Number(order.total).toFixed(2)}</b>
                </div>
                <button className="text-button" onClick={() => {
                    localStorage.setItem('lastOrderId', String(order.idPedido));
                    navigate('/seguimiento', { state: { orderId: order.idPedido } });
                }}>Ver pedido</button>
            </article>)}
        </main>
        <BottomNav active="orders" />
    </div>;
}
