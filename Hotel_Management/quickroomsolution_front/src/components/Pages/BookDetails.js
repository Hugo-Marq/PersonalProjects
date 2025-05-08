import React from "react";
import { useLocation } from "react-router-dom";
import { getRoomDescriptionByID, getNumberOfDaysBetweenDates } from './roomUtilities';
import '../styles/BookDetailsStyle.css';

function DetalhesReserva() {
  const location = useLocation();
  const { username, id, inicio, fim, price } = location.state || {};

  const numberOfDays = getNumberOfDaysBetweenDates(inicio, fim);

  return (
    <div>
      <h1>Book Details</h1>
      <p>Username: {username}</p>
      <p>Room ID: {getRoomDescriptionByID(id)}</p>
      <p>Check-in Date: {inicio}</p>
      <p>Check-out Date: {fim}</p>
      <p>Total Price: {price*numberOfDays}</p>

    </div>
  );
}

export default DetalhesReserva;