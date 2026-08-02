import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon, Button, Header, BottomNav, Back } from '../components/ui';
import api from '../services/api';
import { useGeolocation } from '../hooks/useGeolocation';
import { CircleMarker, MapContainer, Popup, TileLayer, useMap, useMapEvents } from 'react-leaflet';

const defaultRestaurantLocation = [18.1858933, -91.0427157];

function MapClickSelector({ position, onSelect }) {
    useMapEvents({
        click(event) {
            onSelect([event.latlng.lat, event.latlng.lng]);
        }
    });

    return position ? <CircleMarker center={position} radius={11} pathOptions={{ color: '#fff', weight: 4, fillColor: '#ff6b00', fillOpacity: 1 }}><Popup>Ubicación de tu restaurante</Popup></CircleMarker> : null;
}

function RecenterRestaurantMap({ position }) {
    const map = useMap();

    useEffect(() => {
        if (position) map.setView(position, 16);
    }, [map, position]);

    return null;
}

export function Profile() { 
    const navigate = useNavigate();
    const [user, setUser] = useState(null);

    useEffect(() => {
        const storedUser = localStorage.getItem('user');
        if (storedUser) {
            setUser(JSON.parse(storedUser));
        } else {
            navigate('/bienvenida');
        }
    }, [navigate]);

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        navigate('/bienvenida');
    };

    if (!user) return null;

    const isPropietario = user.roles && user.roles.includes('Propietario');
    const isRepartidor = user.tienePerfilRepartidor || (user.roles && user.roles.includes('Repartidor'));

    return <div className="page">
        <Header title="Mi perfil" cart={false}/>
        <main className="section profile">
            <div className="avatar">{user.nombre ? user.nombre.substring(0,2).toUpperCase() : 'U'}</div>
            <h2>{user.nombre || 'Usuario'}</h2>
            <p className="muted">{user.email}</p>
            
            <div style={{marginTop: 20}}>
                {[['person','Información personal'],['location_on','Mis direcciones'],['credit_card','Métodos de pago'],['help','Ayuda y soporte']].map(([icon,text]) => 
                    <button key={text} className="choice">
                        <Icon>{icon}</Icon>
                        <span><b>{text}</b></span>
                        <Icon>chevron_right</Icon>
                    </button>
                )}
            </div>

            <div style={{marginTop: 20}}>
                <h3 style={{marginBottom: 10, fontSize: 16}}>Socios AppComida</h3>
                {!isPropietario ? (
                    <button className="choice highlight" onClick={() => navigate('/registro-restaurante')}>
                        <Icon filled>storefront</Icon>
                        <span><b>Quiero vender mi comida</b></span>
                        <Icon>chevron_right</Icon>
                    </button>
                ) : (
                    <button className="choice highlight" onClick={() => navigate('/panel-restaurante')} style={{border: '1px solid #ff7a00', background: '#fff5ec'}}>
                        <Icon filled style={{color: '#ff7a00'}}>storefront</Icon>
                        <span><b style={{color: '#ff7a00'}}>Mi Restaurante (Panel)</b></span>
                        <Icon>chevron_right</Icon>
                    </button>
                )}

                {!isRepartidor ? (
                    <button className="choice highlight" onClick={() => navigate('/registro-repartidor')}>
                        <Icon filled>two_wheeler</Icon>
                        <span><b>Quiero ser Repartidor</b></span>
                        <Icon>chevron_right</Icon>
                    </button>
                ) : (
                    <button className="choice highlight" onClick={() => navigate('/panel-repartidor')} style={{border: '1px solid #ff7a00', background: '#fff5ec'}}>
                        <Icon filled style={{color: '#ff7a00'}}>two_wheeler</Icon>
                        <span><b style={{color: '#ff7a00'}}>Perfil Repartidor (Panel)</b></span>
                        <Icon>chevron_right</Icon>
                    </button>
                )}
            </div>

            <Button secondary onClick={handleLogout} style={{marginTop: 30}}>Cerrar sesión</Button>
        </main>
        <BottomNav active="perfil" />
    </div>; 
}

export function PartnerRegistration() {
    const navigate = useNavigate();
    const [name, setName] = useState('');
    const [address, setAddress] = useState('');
    const [phone, setPhone] = useState('');
    const [description, setDescription] = useState('');
    const [image, setImage] = useState('');
    const [openingTime, setOpeningTime] = useState('09:00');
    const [closingTime, setClosingTime] = useState('21:00');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const location = useGeolocation();
    const [mapPosition, setMapPosition] = useState(defaultRestaurantLocation);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        try {
            const user = JSON.parse(localStorage.getItem('user'));
            
            const ownedRestaurants = await api.get(`/restaurantes/propietario/${user.idUsuario}`);
            let restaurant = ownedRestaurants.data?.find(item =>
                item.nombre.trim().toLowerCase() === name.trim().toLowerCase()
            );

            if (!restaurant) {
                const restaurantRes = await api.post('/restaurantes', {
                    idUsuarioPropietario: user.idUsuario,
                    nombre: name,
                    descripcion: description || null,
                    direccion: address,
                    telefono: phone,
                    horarioApertura: openingTime,
                    horarioCierre: closingTime,
                    imagen: image || null,
                    latitud: mapPosition[0],
                    longitud: mapPosition[1]
                });
                restaurant = restaurantRes.data;
            }

            try {
                await api.post(`/usuarios/${user.idUsuario}/roles`, {
                    nombreRol: 'Propietario'
                });
            } catch (roleError) {
                if (![404, 409].includes(roleError.response?.status)) throw roleError;
            }
            
            if (!user.roles) user.roles = [];
            if (!user.roles.includes('Propietario')) {
                user.roles.push('Propietario');
                localStorage.setItem('user', JSON.stringify(user));
            }
            
            alert('¡Restaurante registrado con éxito!');
            localStorage.setItem('ownerRestaurantId', String(restaurant.idRestaurante));
            navigate('/panel-restaurante', { state: { restaurantId: restaurant.idRestaurante } });
        } catch (err) {
            setError(err.response?.data?.mensaje || 'Ocurrió un error al registrar el restaurante.');
        } finally {
            setLoading(false);
        }
    };

    return <main className="auth form-screen">
        <Back to="/perfil" />
        <h1>Registra tu Restaurante</h1>
        <p className="muted">Empieza a vender en minutos.</p>
        {error && <p style={{color: 'red'}}>{error}</p>}
        <form onSubmit={handleSubmit} className="fields">
            <label>Ubicación del restaurante en el mapa
                <small className="muted">Haz clic sobre el mapa para colocar el punto exacto.</small>
                <div className="restaurant-location-map">
                    <MapContainer center={mapPosition} zoom={16} scrollWheelZoom style={{height: '100%', width: '100%'}}>
                        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>' />
                        <RecenterRestaurantMap position={mapPosition}/>
                        <MapClickSelector position={mapPosition} onSelect={setMapPosition}/>
                    </MapContainer>
                </div>
                <span className="map-coordinates">Latitud: {mapPosition[0].toFixed(7)} · Longitud: {mapPosition[1].toFixed(7)}</span>
                {location.latitude != null && <button type="button" className="use-current-location" onClick={() => setMapPosition([location.latitude, location.longitude])}><Icon>my_location</Icon> Usar mi ubicación actual</button>}
            </label>
            <label>Nombre del Restaurante
                <input required value={name} onChange={e => setName(e.target.value)} placeholder="Ej. Tacos El Rey" />
            </label>
            <label>Dirección
                <input required value={address} onChange={e => setAddress(e.target.value)} placeholder="Calle Principal 123" />
            </label>
            <label>Teléfono
                <input required value={phone} onChange={e => setPhone(e.target.value)} placeholder="5551234567" />
            </label>
            <label>Descripción
                <input value={description} onChange={e => setDescription(e.target.value)} placeholder="Describe tu tipo de comida" />
            </label>
            <label>URL de la foto del restaurante
                <input type="url" value={image} onChange={e => setImage(e.target.value)} placeholder="https://..." />
            </label>
            <div style={{display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12}}>
                <label>Hora de apertura
                    <input type="time" required value={openingTime} onChange={e => setOpeningTime(e.target.value)} />
                </label>
                <label>Hora de cierre
                    <input type="time" required value={closingTime} onChange={e => setClosingTime(e.target.value)} />
                </label>
            </div>
            <small className="muted">{location.loading ? 'Buscando tu ubicación actual...' : location.error ? 'No se obtuvo el GPS; selecciona manualmente el punto en el mapa.' : 'Puedes ajustar el punto haciendo clic en el mapa.'}</small>
            <Button disabled={loading}>{loading ? 'Guardando...' : 'Registrar Restaurante'}</Button>
        </form>
    </main>;
}

export function DriverRegistration() {
    const navigate = useNavigate();
    const [vehicle, setVehicle] = useState('Motocicleta');
    const [plate, setPlate] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        try {
            const user = JSON.parse(localStorage.getItem('user'));
            
            try {
                await api.get(`/repartidores/usuario/${user.idUsuario}`);
            } catch (profileError) {
                if (profileError.response?.status !== 404) throw profileError;
                await api.post('/repartidores', { idUsuario: user.idUsuario });
            }

            try {
                await api.post(`/usuarios/${user.idUsuario}/roles`, {
                    nombreRol: 'Repartidor'
                });
            } catch (roleError) {
                if (![404, 409].includes(roleError.response?.status)) throw roleError;
            }
            
            if (!user.roles) user.roles = [];
            if (!user.roles.includes('Repartidor')) {
                user.roles.push('Repartidor');
                user.tienePerfilRepartidor = true;
                localStorage.setItem('user', JSON.stringify(user));
            }
            
            alert('¡Perfil de repartidor creado con éxito!');
            navigate('/panel-repartidor');
        } catch (err) {
            setError(err.response?.data?.mensaje || 'Ocurrió un error al registrarse como repartidor.');
        } finally {
            setLoading(false);
        }
    };

    return <main className="auth form-screen">
        <Back to="/perfil" />
        <h1>Conviértete en Repartidor</h1>
        <p className="muted">Genera ganancias extra a tu ritmo.</p>
        {error && <p style={{color: 'red'}}>{error}</p>}
        <form onSubmit={handleSubmit} className="fields">
            <label>Tipo de Vehículo
                <select value={vehicle} onChange={e => setVehicle(e.target.value)} style={{width: '100%', padding: '12px', borderRadius: '8px', border: '1px solid #ddd', marginTop: '8px', marginBottom: '16px'}}>
                    <option value="Motocicleta">Motocicleta</option>
                    <option value="Bicicleta">Bicicleta</option>
                    <option value="Automóvil">Automóvil</option>
                </select>
            </label>
            <label>Placa del Vehículo (Opcional)
                <input value={plate} onChange={e => setPlate(e.target.value)} placeholder="XYZ-123" />
            </label>
            <Button disabled={loading}>{loading ? 'Guardando...' : 'Registrarme'}</Button>
        </form>
    </main>;
}
