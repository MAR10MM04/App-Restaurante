import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon, Button, Header, BottomNav } from '../components/ui';

export function Profile() { 
    const navigate = useNavigate();
    return <div className="page"><Header title="Mi perfil" cart={false}/><main className="section profile"><div className="avatar">AM</div><h2>Ana Martínez</h2><p className="muted">ana.martinez@email.com</p>{[['person','Información personal'],['location_on','Mis direcciones'],['credit_card','Métodos de pago'],['help','Ayuda y soporte']].map(([icon,text]) => <button key={text} className="choice"><Icon>{icon}</Icon><span><b>{text}</b></span><Icon>chevron_right</Icon></button>)}<Button secondary onClick={() => navigate('/bienvenida')}>Cerrar sesión</Button></main><BottomNav active="perfil" /></div>; 
}
