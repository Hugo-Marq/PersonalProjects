import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { getRoomDescriptionByID, getNumberOfDaysBetweenDates, getRoomImageByID } from './roomUtilities';
import '../styles/BookConfirmationPageStyle.css';
import Modal from 'react-modal';
import {getAuthHeaders} from '../FetchHeader';

function BookConfirmationPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const { username, id, inicio, fim, price } = location.state || {};
  const [numCartao, setNumCartao] = useState('');
  const [showModal, setShowModal] = useState(false);

  const handleCancel = () => {
    navigate('/');
  };

  const handleModalClose = () => {
    // Ao fechar o modal, navegar de volta para a página principal
    navigate('/');
  };

  const headers = getAuthHeaders(); // Get the latest headers
  const handleConfirm = async () => {
    const reservationData = {
      ClienteClienteId: username, // Assuming username is the client ID
      DataInicio: inicio,
      DataFim: fim,
      NumeroCartao: numCartao,
      EstadoReserva: 1, // Set EstadoReserva to 1
      TipologiaId: id

    };
   
    try {
      const response = await fetch('https://localhost:7258/api/Reserva', {
        method: 'POST',
        headers: headers,
        body: JSON.stringify(reservationData)
      });

      if (!response.ok) {
        throw new Error('Failed to add booking');
      }

      // If booking added successfully, set showModal to true
      setShowModal(true);

    } catch (error) {
      console.error('Error:', error);
      // Handle error as needed
    }
  };

  const numberOfDays = getNumberOfDaysBetweenDates(inicio, fim);

  return (
    <div>
      <div className="booking-confirmation-container"></div>
      <h1 className="booking-confirmation-title">Booking Confirmation</h1>
      <div className="image-and-text-container">
      <div className="image-container">
          <img src={getRoomImageByID(id)} alt="Room" className="room-image" />
        </div>
        <div className="info-container">
        <p><strong>Username:</strong> {username}</p>
          <p><strong>Room Type:</strong> {getRoomDescriptionByID(id)}</p>
          <p><strong>Check-in Date:</strong> {inicio}</p>
          <p><strong>Check-out Date:</strong> {fim}</p>
          <p><strong>Price Per Night €:</strong> {price}</p>
          <p><strong>Total Price €:</strong> {price * numberOfDays}</p>
        </div>
      </div>
      <input className='input-num-cartao'
        type="text"
        value={numCartao}
        onChange={(e) => setNumCartao(e.target.value)}
        placeholder="Enter Card Number"
      />
      <div>
      <div className="buttons-container">
        <button onClick={handleCancel}>Cancel</button>
        <button onClick={handleConfirm}>Confirm</button>
      </div>
      </div>
      {/* Modal de confirmação */}
      <Modal
        isOpen={showModal}
        onRequestClose={handleModalClose}
        contentLabel="Booking Confirmation"
        style={{
          content: {
            width: '50%', // Defina o tamanho desejado aqui (porcentagem, pixels, etc.)
            height: '50%', // Defina o tamanho desejado aqui (porcentagem, pixels, etc.)
            margin: 'auto' // Centralize a modal na tela
          }
        }}
      >
        <h2>Booking Confirmed!</h2>
        <p>Your reservation has been confirmed.</p>
        <p>An email with the reservation number has been sent.</p>
        <button onClick={handleModalClose}>OK</button>
      </Modal>
    </div>

  );
}
export default BookConfirmationPage;