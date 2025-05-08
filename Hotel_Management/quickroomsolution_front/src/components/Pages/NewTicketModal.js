import React, { useState, useEffect } from 'react';
import '../styles/NewTicketModalStyle.css';
import Modal from 'react-modal';

function NewTicketModal({ isOpen, onClose, room, onCreate, services }) {
  const [formData, setFormData] = useState({
    ticketDescription: '',
    selectedServiceId: '',
  });

  useEffect(() => {
    if (!isOpen) {
      setFormData({
        ticketDescription: '',
        selectedServiceId: '',
      });
    }
  }, [isOpen]);

  const handleSubmit = (e) => {
    e.preventDefault();
    onCreate(formData);
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value,
    });
  };

  const handleServiceChange = (e) => {
    const { value } = e.target;
    setFormData({
      ...formData,
      selectedServiceId: value,
    });
  };

  if (!isOpen) {
    return null;
  }

  return (
    <Modal isOpen={isOpen} onRequestClose={onClose} className="modal">
      <div className="modal-content">
        <h2>Create Ticket for Room ID: {room.quartoId}</h2>
        <p>Block: {room.bloco}</p>
        <p>Floor: {room.piso}</p>
        <p>Door: {room.porta}</p>
        <p>State: {room.quartoEstado}</p>
        <p>Type ID: {room.tipologiaTipologiaId}</p>
        <form onSubmit={handleSubmit}>
          <label>
            Ticket Description:
            <textarea
              name="ticketDescription"
              value={formData.ticketDescription}
              onChange={handleInputChange}
              required
            />
          </label>
          <label>
            Select Service:
            <select
              name="selectedServiceId"
              value={formData.selectedServiceId}
              onChange={handleServiceChange}
              required
            >
              <option value="" disabled>Select a service</option>
              {services.map(service => (
                <option key={service.servicoId} value={service.servicoId}>
                  {service.servicoTipo}
                </option>
              ))}
            </select>
          </label>
          <div className="modal-actions">
            <button type="submit">Create Ticket</button>
            <button type="button" onClick={onClose}>Close</button>
          </div>
        </form>
      </div>
    </Modal>
  );
}

export default NewTicketModal;
