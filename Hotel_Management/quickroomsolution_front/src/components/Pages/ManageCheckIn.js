import React, { useState, useEffect } from "react";
// import "../styles/ReservationsList.css";
import {getAuthHeaders} from '../FetchHeader';

const fetchReservations = async () => {
  try {
    const response = await fetch(
      "https://localhost:7258/api/Reserva/Reservas-Waiting-CheckIn"
    );
    if (!response.ok) {
      throw new Error("Network response was not ok");
    }
    const data = await response.json();
    console.log("Fetched Reservations:", data); // Log the fetched data
    return data;
  } catch (error) {
    console.error("Error fetching reservations:", error);
    return [];
  }
};

const proceedToCheckIn = async (reservationId) => {
  const headers = getAuthHeaders(); // Get the latest headers
  try {
    const response = await fetch(
      `https://localhost:7258/api/Reserva/DoCheckin?id=${reservationId}`,
      {
        method: "PUT",
        headers: headers,
      }
    );
    if (!response.ok) {
      throw new Error("Network response was not ok");
    }
    console.log("Check-in successful for reservation ID:", reservationId);
    return true;
  } catch (error) {
    console.error("Error during check-in:", error);
    return false;
  }
};

const ManageCheckIn = () => {
  const [reservations, setReservations] = useState([]);

  useEffect(() => {
    async function fetchAllReservations() {
      const data = await fetchReservations();
      setReservations(data);
    }
    fetchAllReservations();
  }, []);

  const handleCheckInClick = async (reservationId) => {
    const success = await proceedToCheckIn(reservationId);
    if (success) {
      alert("Check-in successful!");
      setReservations(
        reservations.filter(
          (reservation) => reservation.reservaId !== reservationId
        )
      );
    } else {
      alert("Error during check-in. Please try again.");
    }
  };

  return (
    <div>
      <h2>Pending Check-Ins</h2>
      <ul>
        {reservations.map((reservation) => (
          <li key={reservation.ReservaId} className="reservation-item">
            <div className="reservation-details">
              <span>Reservation ID: {reservation.reservaId}</span>
              <span>Room Type: {reservation.tipologiaId}</span>
              <span>Room: {reservation.quartoQuartoId}</span>
              <span>Guest Name: {reservation.clienteClienteId}</span>
              <button
                className="button-checkin"
                onClick={() => handleCheckInClick(reservation.reservaId)}
              >
                Proceed to Check-In
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default ManageCheckIn;
