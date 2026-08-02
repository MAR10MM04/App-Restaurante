import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Splash, Welcome, Login, Register, Permissions } from './pages/Auth';
import { Home, Search, Categories } from './pages/Main';
import { Restaurant, Product, Cart, Confirm } from './pages/Restaurant';
import { OrderPlaced, Tracking, Delivered, Rating, History } from './pages/Orders';
import { Profile, PartnerRegistration, DriverRegistration } from './pages/Profile';
import { OwnerDashboard, DriverDashboard } from './pages/Dashboards';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/splash" replace />} />
        <Route path="/splash" element={<Splash />} />
        <Route path="/bienvenida" element={<Welcome />} />
        <Route path="/login" element={<Login />} />
        <Route path="/registro" element={<Register />} />
        <Route path="/permisos" element={<Permissions />} />
        
        <Route path="/home" element={<Home />} />
        <Route path="/buscar" element={<Search />} />
        <Route path="/categorias" element={<Categories />} />
        
        <Route path="/restaurante/:id" element={<Restaurant />} />
        <Route path="/producto/:id" element={<Product />} />
        <Route path="/carrito" element={<Cart />} />
        <Route path="/confirmar-pedido" element={<Confirm />} />
        
        <Route path="/pedido-realizado" element={<OrderPlaced />} />
        <Route path="/seguimiento" element={<Tracking />} />
        <Route path="/pedido-entregado" element={<Delivered />} />
        <Route path="/calificacion" element={<Rating />} />
        <Route path="/historial" element={<History />} />
        
        <Route path="/perfil" element={<Profile />} />
        <Route path="/registro-restaurante" element={<PartnerRegistration />} />
        <Route path="/registro-repartidor" element={<DriverRegistration />} />
        <Route path="/panel-restaurante" element={<OwnerDashboard />} />
        <Route path="/panel-repartidor" element={<DriverDashboard />} />
        
        {/* Fallback route */}
        <Route path="*" element={<Navigate to="/splash" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
