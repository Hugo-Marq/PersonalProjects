import React, { useState, useEffect } from 'react';
import '../styles/ManageTicketsStyle.css';
import {getAuthHeaders} from '../FetchHeader';

function ManageTickets() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filterByValidation, setFilterByValidation] = useState(false);

  // Mapeamento dos estados
  const stateMap = {
    1: 'Pendente',
    2: 'Atribuido',
    3: 'ParaRevisao',
    4: 'Iniciado',
    5: 'Finalizado',
    6: 'Cancelado',
    7: 'Rejeitado',
    8: 'Concluido',
    9: 'Validado',
    // Adicione mais estados conforme necessário
  };
  const headers = getAuthHeaders(); // Get the latest headers
  useEffect(() => {
    const fetchTickets = async () => {
      try {
        console.log(filterByValidation);
        const endpoint = filterByValidation
          ? 'https://localhost:7258/api/Ticket/Filtrados-Por-Estado?estado=1'
          : 'https://localhost:7258/api/Ticket';

        const response = await fetch(endpoint, {
          method: 'GET',
          headers: headers,
        });

        if (!response.ok) {
          throw new Error('Failed to fetch tickets');
        }

        const data = await response.json();
        setTickets(data);
        setLoading(false);
      } catch (error) {
        setError(error.message);
        setLoading(false);
      }
    };

    fetchTickets();
  }, [filterByValidation]);

  const handleFilterChange = (e) => {
    setFilterByValidation(e.target.checked);
  };

  const updateTicketStatus = async (ticket, status) => {
    const url = status === 'Rejeitado'
      ? 'https://localhost:7258/api/Ticket/RejeitarTicket'
      : 'https://localhost:7258/api/Ticket/AprovarTicket';

    try {
      console.log('Sending ticket data:', ticket);
      console.log('Endpoint:', url);

      const response = await fetch(url, {
        method: 'PUT',
        headers: headers,
        body: JSON.stringify(ticket),
      });

      if (!response.ok) {
        const errorMessage = await response.text();
        alert(errorMessage);
        return;
      }

      const updatedTicket = await response.json();

      // Update the state to reflect the changes
      setTickets((prevTickets) =>
        prevTickets.map((t) =>
          t.ticketId === updatedTicket.ticketId ? updatedTicket : t
        )
      );
      const message = status === 'Rejeitado'
        ? 'Ticket foi rejeitado.'
        : 'Ticket foi validado.';
      alert(message);
    } catch (error) {
      alert('An error occurred while updating the ticket status');
      console.error(error);
    }
  };

  const approveService = async (ticket) => {
    const url = 'https://localhost:7258/api/Ticket/Aprovar-Manutencao-Prestada';

    try {
      console.log('Sending ticket data:', ticket);
      console.log('Endpoint:', url);

      const response = await fetch(url, {
        method: 'PUT',
        headers: headers,
        body: JSON.stringify(ticket),
      });

      if (!response.ok) {
        const errorMessage = await response.text();
        alert(errorMessage);
        return;
      }

      const updatedTicket = await response.json();

      // Update the state to reflect the changes
      setTickets((prevTickets) =>
        prevTickets.map((t) =>
          t.ticketId === updatedTicket.ticketId ? updatedTicket : t
        )
      );
      alert('Service has been validated.');
    } catch (error) {
      alert('An error occurred while validating the service');
      console.error(error);
    }
  };

  if (loading) {
    return <p>Loading...</p>;
  }

  if (error) {
    return <p className="error-message">Error: {error}</p>;
  }

  if (tickets.length === 0) {
    return <p className="no-tickets">No tickets to validate.</p>;
  }

  return (
    <div className="manage-tickets-container">
      <h1>Ticket List</h1>
      <div className="filter-container">
        <label>
          Filter by Pending:
          <input
            className="filter-checkbox"
            type="checkbox"
            checked={filterByValidation}
            onChange={handleFilterChange}
          />
        </label>
      </div>
      <ul className="ticket-list">
        {tickets.map((ticket) => (
          <li key={ticket.ticketId} className="ticket-box">
            <div className="ticket-details">
              <p>Ticket ID: {ticket.ticketId}</p>
              <p>Description: {ticket.ticketDescricao}</p>
              <p>Opening Date: {new Date(ticket.ticketDataAbertura).toLocaleString()}</p>
              {/* Mapeamento do estado */}
              <p>Ticket Status: {stateMap[ticket.ticketEstado]}</p>
            </div>
            {ticket.ticketEstado === 5 && (
              <div className="ticket-actions">
                <button onClick={() => approveService(ticket)}>Validate Service</button>
              </div>
            )}
            {ticket.ticketEstado === 1 && (
              <div className="ticket-actions">
                <button onClick={() => updateTicketStatus(ticket, 'Concluido')}>Validate</button>
                <button onClick={() => updateTicketStatus(ticket,'Rejeitado')}>Reject</button>
              </div>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default ManageTickets;
