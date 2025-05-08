import React, { useState, useEffect } from 'react';
import NewTicketModal from './NewTicketModal';
import '../styles/RoomManagementStyle.css';
import { Modal } from 'react-bootstrap';
import {getAuthHeaders} from '../FetchHeader';

function RoomManagement() {
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedRoom, setSelectedRoom] = useState(null);
  const [services, setServices] = useState([]);
  const [ticketDescription, setTicketDescription] = useState('');
  const headers = getAuthHeaders(); // Get the latest headers
  const fetchRoomsAndServices = async () => {
    try {
      
      const roomsResponse = await fetch('https://localhost:7258/api/Quartos', {
        method: 'GET',
        headers: headers,
      });

      if (!roomsResponse.ok) {
        throw new Error('Erro ao buscar os quartos');
      }

      const roomsData = await roomsResponse.json();
      setRooms(roomsData);

      const servicesResponse = await fetch('https://localhost:7258/api/Servico', {
        method: 'GET',
        headers: headers,
      });

      if (!servicesResponse.ok) {
        throw new Error('Erro ao buscar os serviços');
      }

      const servicesData = await servicesResponse.json();
      setServices(servicesData);

    } catch (error) {
      setError(error.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRoomsAndServices();
  }, []);

  const handleOpenModal = (room) => {
    setSelectedRoom(room);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedRoom(null);
  };

  const handleCreateTicket = async (formData) => {
    try {
      const username = localStorage.getItem('username'); // Retrieve username from localStorage
      if (!username) {
        throw new Error('User not logged in');
      }
      const requestBody = {
        funcionarioFuncionarioId: username, // Replace with actual username logged in
        quartoQuartoId: selectedRoom.quartoId,
        ticketDescricao: formData.ticketDescription,
        ticketDataAbertura: new Date().toISOString(),
        ticketEstado: 1,
        servicoId: formData.selectedServiceId,
      };
  
      console.log('Request Body:', requestBody);
  
      const response = await fetch(`https://localhost:7258/api/Ticket`, {
        method: 'POST',
        headers: headers,
        body: JSON.stringify(requestBody),
      });
  
      if (!response.ok) {
        throw new Error('Erro ao criar o ticket');
      }
  
      // Assuming the ticket creation was successful, update the rooms state or refetch rooms to reflect the changes
      fetchRoomsAndServices();
      setIsModalOpen(false);
      setSelectedRoom(null);
    } catch (error) {
      console.error('Error creating ticket:', error);
      // Handle error as needed
    }
  };
  
  
  
  if (loading) {
    return <p>Loading Rooms...</p>;
  }

  if (error) {
    return <p>Error: {error}</p>;
  }

  return (
    <div>
      <h1>Room's List</h1>
      <div className="room-list">
        {rooms.map((room) => (
          <div key={room.quartoId} className="room-container">
            <div className="room-details">
              <p>Room ID: {room.quartoId}</p>
              <p>Block: {room.bloco}</p>
              <p>Floor: {room.piso}</p>
              <p>Door: {room.porta}</p>
              <p>State: {room.quartoEstado}</p>
              <p>Type ID: {room.tipologiaTipologiaId}</p>
            </div>
            <div className="room-actions">
              <button onClick={() => handleOpenModal(room)}>New Ticket</button>
            </div>
          </div>
        ))}
      </div>
      {isModalOpen && (
        <NewTicketModal
          isOpen={isModalOpen}
          onClose={handleCloseModal}
          room={selectedRoom}
          onCreate={handleCreateTicket}
          services={services} // Pass services as props
          ticketDescription={ticketDescription} // Pass ticketDescription as a prop
          setTicketDescription={setTicketDescription} // Pass setTicketDescription as a prop
        />
      )}
    </div>
  );
}

export default RoomManagement;
