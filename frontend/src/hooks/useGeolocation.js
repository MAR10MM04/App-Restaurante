import { useState, useEffect } from 'react';
import axios from 'axios';

export function useGeolocation() {
  const [location, setLocation] = useState({
    latitude: null,
    longitude: null,
    address: 'Ubicación desconocida',
    loading: true,
    error: null,
  });

  useEffect(() => {
    if (!navigator.geolocation) {
      setLocation(prev => ({
        ...prev,
        error: 'La geolocalización no es soportada por este navegador.',
        loading: false,
      }));
      return;
    }

    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const { latitude, longitude } = position.coords;
        let address = 'Ubicación actual';
        
        try {
          // Reverse geocoding using OpenStreetMap (Nominatim)
          const response = await axios.get(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${latitude}&lon=${longitude}`);
          if (response.data && response.data.display_name) {
            // Extraer una dirección más corta
            const ad = response.data.address;
            if (ad.road) {
              address = `${ad.road}${ad.house_number ? ' ' + ad.house_number : ''}, ${ad.city || ad.town || ad.village || ad.state}`;
            } else {
              address = response.data.display_name.split(',').slice(0, 2).join(', ');
            }
          }
        } catch (error) {
          console.error('Error fetching address:', error);
        }

        setLocation({
          latitude,
          longitude,
          address,
          loading: false,
          error: null,
        });
      },
      (error) => {
        setLocation(prev => ({
          ...prev,
          error: error.message,
          loading: false,
          address: 'No se pudo obtener tu ubicación actual',
          latitude: null,
          longitude: null
        }));
      },
      { enableHighAccuracy: true, timeout: 15000, maximumAge: 10000 }
    );
  }, []);

  return location;
}
