import '../styles/HotelReservationStyle.css'; 
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom'; 
import HeaderHomepage from '../HeaderHomepage'; 
import room1 from '../../images/room1.png'; 
import room2 from '../../images/room2.png'; 
import room3 from '../../images/room3.png'; 
import {getAuthHeaders} from '../FetchHeader';

const roomImages = {
  'The Royal Single Room': room1,
  'The Royal Double Room': room2,
  'The Royal Suite': room3
};

const tipologiaMap = {
  1: 'The Royal Single Room',
  2: 'The Royal Double Room',
  3: 'The Royal Suite'
};

async function fetchReservationsByTipologia(tipologiaId) {
  const headers = getAuthHeaders(); // Get the latest headers
  try {
    const response = await fetch(`https://localhost:7258/api/Reserva/tipologia/${tipologiaId}`,
      { headers: headers }
    );
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
    const data = await response.json();
    console.log(`Data for tipologia ${tipologiaId}:`, data);
    return { roomType: tipologiaMap[tipologiaId], reservations: data };
  } catch (error) {
    console.error(`Error fetching data for tipologia ${tipologiaId}:`, error);
    return { roomType: tipologiaMap[tipologiaId], reservations: [] };
  }
}

function HotelReservations() {
  const [occupancyData, setOccupancyData] = useState([]);
  const navigate = useNavigate();

  
  useEffect(() => {
    async function fetchAllData() {
      const data1 = await fetchReservationsByTipologia(1);
      const data2 = await fetchReservationsByTipologia(2);
      const data3 = await fetchReservationsByTipologia(3);
      setOccupancyData([data1, data2, data3]);
      console.log("Occupancy Data:", [data1, data2, data3]);
    }
    fetchAllData();
  }, []);

  const handleDetailsClick = (item) => {
    console.log("Navigating to details with item:", item);
    navigate('/room-details', { state: { roomData: item } });
  };

  return (
    <div>
      <HeaderHomepage />
      <div className="hotelreservations-list">
        <h2>OCCUPATION AT THE MOMENT</h2>
        <ul>
          {occupancyData.map((item) => (
            <li key={item.roomType} className="hotelreservations-item">
              <img src={roomImages[item.roomType]} alt={item.roomType} className="occupations-room-image" />
              <div className="room-details">
                <span>{item.roomType}: </span>
                <span>{item.reservations.length}</span>
              </div>
              <button className="button-details" onClick={() => handleDetailsClick(item)}>Details</button>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}

export default HotelReservations;