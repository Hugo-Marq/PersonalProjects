import React, { useState, useEffect } from "react";
// import "../styles/ReservationsList.css";
import {getAuthHeaders} from '../FetchHeader';

const fetchReservationsForCheckout = async () => {
  try {
    const response = await fetch(
      "https://localhost:7258/api/Reserva/Reservas-Waiting-CheckOut"
    );
    if (!response.ok) {
      throw new Error("Network response was not ok");
    }
    const data = await response.json();
    console.log("Fetched Reservations for Checkout:", data); // Log the fetched data
    return data;
  } catch (error) {
    console.error("Error fetching reservations for checkout:", error);
    return [];
  }
};

const proceedToCheckOut = async (reservationId) => {
  const headers = getAuthHeaders(); // Get the latest headers
  try {
    const response = await fetch(
      `https://localhost:7258/api/Reserva/DoCheckout?id=${reservationId}`,
      {
        method: "PUT",
        headers: headers,
      }
    );
    if (!response.ok) {
      throw new Error("Network response was not ok");
    }
    console.log("Check-out successful for reservation ID:", reservationId);
    return true;
  } catch (error) {
    console.error("Error during check-out:", error);
    return false;
  }
};

const ManageCheckOut = () => {
  const [reservations, setReservations] = useState([]);

  useEffect(() => {
    async function fetchAllReservations() {
      const data = await fetchReservationsForCheckout();
      setReservations(data);
    }
    fetchAllReservations();
  }, []);

  const handleCheckOutClick = async (reservationId) => {
    const success = await proceedToCheckOut(reservationId);
    if (success) {
      alert("Check-out successful!");
      setReservations(
        reservations.filter(
          (reservation) => reservation.reservaId !== reservationId
        )
      );
    } else {
      alert("Error during check-out. Please try again.");
    }
  };

  return (
    <div>
      <h2>Pending Check-Outs</h2>
      <ul>
        {reservations.map((reservation) => (
          <li key={reservation.reservaId} className="reservation-item">
            <div className="reservation-details">
              <span>Reservation ID: {reservation.reservaId}</span>
              <span>Room Type: {reservation.tipologiaId}</span>
              <span>Room: {reservation.quartoQuartoId}</span>
              <span>Guest Name: {reservation.clienteClienteId}</span>
              <button
                className="button-checkout"
                onClick={() => handleCheckOutClick(reservation.reservaId)}
              >
                Proceed to Check-Out
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default ManageCheckOut;
