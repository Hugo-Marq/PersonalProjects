import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import {getAuthHeaders} from '../FetchHeader';

function ChangeReservationRoom() {
  const location = useLocation();
  const reservation = location.state?.reservation;
  const [availableRooms, setAvailableRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [changeRoomSuccess, setChangeRoomSuccess] = useState(false);
  const [updatedReservation, setUpdatedReservation] = useState(null);

  useEffect(() => {
    if (reservation) {
      console.log("Reservation data received:", reservation);
      fetchAvailableRooms(reservation.reservaId);
    } else {
      console.error("No reservation data found in location state.");
      setLoading(false);
    }
  }, [reservation]);
  const headers = getAuthHeaders(); // Get the latest headers

  const fetchAvailableRooms = async (reservaId) => {
    console.log("Fetching available rooms with reservaId:", reservaId);
    try {
      const response = await fetch(`https://localhost:7258/api/Quartos/Quartos-Livres-Para-Troca?reservaId=${reservaId}`,
        { headers: headers }
      );
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json();
      console.log("Available rooms fetched:", data);
      setAvailableRooms(data);
      setLoading(false);
    } catch (error) {
      console.error("Error fetching available rooms:", error);
      setError(error);
      setLoading(false);
    }
  };

  const handleChangeRoom = async (reservaId, selectedRoom) => {
    const novoQuartoId = selectedRoom.quartoId;
    console.log(`Changing room for reservaId: ${reservaId} to novoQuartoId: ${novoQuartoId}`);
    try {
      const response = await fetch(`https://localhost:7258/api/Reserva/TrocarQuarto?reservaId=${reservaId}&novoQuartoId=${novoQuartoId}`, {
        method: 'PUT',
        headers: headers
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json();
      console.log("Room change successful:", data);
      setChangeRoomSuccess(true);
      setUpdatedReservation({ ...reservation, quartoQuartoId: novoQuartoId });

      // Display the confirmation alert with new room information
      const confirmationMessage = `Room change successful.\nNew Room ID: ${selectedRoom.quartoId}\nRoom Type: ${selectedRoom.tipologiaTipologiaId}\nClick OK to close.`;
      window.alert(confirmationMessage);

      // Close modal if needed
      setChangeRoomSuccess(false);
      setUpdatedReservation(null);

    } catch (error) {
      console.error("Error changing room:", error);
      setError(error);
    }
  };

  if (loading) {
    return <div>Loading available rooms...</div>;
  }

  if (error) {
    return <div>Error: {error.message}</div>;
  }

  return (
    <div>
      <h1>Change Reservation Room</h1>
      {reservation ? (
        <>
          <p>Client ID: {reservation.clienteClienteId}</p>
          <p>Check-in Date: {reservation.dataInicio}</p>
          <p>Check-out Date: {reservation.dataFim}</p>
          <p>Room ID: {reservation.quartoQuartoId}</p>

          <h2>Available Rooms for Change</h2>
          <ul>
            {availableRooms.map((room) => (
              <li key={room.quartoId}>
                <p>Room ID: {room.quartoId}</p>
                <p>Room Type: {room.tipologiaTipologiaId}</p>
                <button onClick={() => handleChangeRoom(reservation.reservaId, room)}>Change Room</button>
              </li>
            ))}
          </ul>
        </>
      ) : (
        <div>No reservation data available</div>
      )}
    </div>
  );
}

export default ChangeReservationRoom;
