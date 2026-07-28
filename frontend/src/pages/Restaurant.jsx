import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon, Button, Header, Back, ProductRow } from '../components/ui';
import { foodImage } from './Main';

export function Restaurant() { 
    const navigate = useNavigate();
    return <div className="page restaurant"><div className="restaurant-hero" style={{ backgroundImage:`linear-gradient(0deg,rgba(0,0,0,.5),transparent),url(${foodImage})` }}><Back /><button className="icon-button white" onClick={() => navigate('/carrito')}><Icon>shopping_cart</Icon></button></div><main className="section"><p className="eyebrow">HAMBURGUESAS · 20-30 MIN</p><h1>Burger King</h1><p className="muted">Hamburguesas a la parrilla, papas crujientes y más.</p><div className="info-row"><span><Icon filled>star</Icon> 4.7</span><span><Icon>delivery_dining</Icon> Envío $25</span></div><h2>Más pedidos</h2><ProductRow name="Super Bacon Burger" price="$149" image={foodImage}/><ProductRow name="Combo Whopper" price="$189" image={foodImage}/></main></div>; 
}

export function Product() { 
    const navigate = useNavigate();
    const [quantity,setQuantity]=useState(1); 
    return <div className="page product"><Header title="Detalle del producto"/><main><img className="product-image" src={foodImage} alt="Super Bacon Burger"/><section className="section"><h1>Super Bacon Burger</h1><p className="muted">Doble carne a la parrilla, queso cheddar, tocino crujiente y vegetales frescos.</p><h2>$149.00</h2><h3>Personaliza tu pedido</h3><label className="option"><span>Queso extra <small>+$15</small></span><input type="checkbox" /></label><label className="option"><span>Papas a la francesa <small>+$35</small></span><input type="checkbox" /></label></section></main><footer className="purchase-bar"><div className="quantity"><button onClick={() => setQuantity(Math.max(1,quantity-1))}>−</button><b>{quantity}</b><button onClick={() => setQuantity(quantity+1)}>+</button></div><Button onClick={() => navigate('/carrito')}>Agregar · ${149 * quantity}</Button></footer></div>; 
}

export function Cart() { 
    const navigate = useNavigate();
    return <div className="page"><Header title="Mi carrito" cart={false}/><main className="section"><div className="cart-restaurant"><Icon filled>storefront</Icon><div><b>Burger King</b><small>Entrega estimada: 20-30 min</small></div></div><ProductRow name="Super Bacon Burger" price="$149" image={foodImage}/><div className="summary"><p>Subtotal <b>$149.00</b></p><p>Costo de envío <b>$25.00</b></p><p>Total <b>$174.00</b></p></div></main><footer className="purchase-bar"><Button onClick={() => navigate('/confirmar-pedido')}>Continuar con el pedido</Button></footer></div>; 
}

export function Confirm() { 
    const navigate = useNavigate();
    return <div className="page"><Header title="Confirmar pedido" cart={false}/><main className="section"><h2>Dirección de entrega</h2><div className="choice"><Icon filled>location_on</Icon><span><b>Casa</b><small>Av. Paseo de la Reforma 222, CDMX</small></span><Icon>chevron_right</Icon></div><h2>Método de pago</h2><div className="choice"><Icon>credit_card</Icon><span><b>Tarjeta terminada en 4242</b><small>Visa</small></span><Icon>chevron_right</Icon></div><div className="summary"><p>Total a pagar <b>$174.00</b></p></div></main><footer className="purchase-bar"><Button onClick={() => navigate('/pedido-realizado')}>Realizar pedido</Button></footer></div>; 
}
