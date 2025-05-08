import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import HeaderHomepage from '../HeaderHomepage';
import '../styles/RoomDetailsStyle.css';
import room1 from '../../images/room1.png';
import room2 from '../../images/room2.png';
import room3 from '../../images/room3.png';

const roomImages = {
  'The Royal Single Room': room1,
  'The Royal Double Room': room2,
  'The Royal Suite': room3
};

function RoomDetails() {
  const location = useLocation();
  const navigate = useNavigate();
  const roomData = location.state?.roomData;

  console.log("Room Details received data:", roomData);

  if (!roomData) {
    return <div>No room data available</div>;
  }

  const { roomType, reservations } = roomData;
  

  const handleNavigate = (reservation) => {
    navigate('/change-reservation-room', { state: { reservation } });
  };

  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
  };

  return (
    <div>
      <HeaderHomepage />
      <div className="room-details-container">
        <h1>Room Details</h1>
        <img
          src={roomImages[roomType]}
          alt={roomType}
          className="room-details-image"
        />
        <h2>{roomType}</h2>
        <ul>
          {reservations.map((reservation, index) => (
            <li key={index}>
              <p>Client ID: {reservation.clienteClienteId}</p>
              <p>Check-in Date: {formatDate(reservation.dataInicio)}</p>
              <p>Check-out Date: {formatDate(reservation.dataFim)}</p>
              <p>Room ID : {reservation.quartoQuartoId}</p>
              <button onClick={() => handleNavigate(reservation)}>Change Res. Room</button>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}

export default RoomDetails;