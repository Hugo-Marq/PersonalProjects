import React from 'react';
import { TicketStates } from '../../ticketStates';
import '../styles/TicketDetailsStyle.css';

function Ticket({ ticket, onStatusChange }) {
  const handleStatusChange = (newState) => {
    if (window.confirm(`Tem certeza que deseja alterar o estado do ticket para ${newState}?`)) {
      onStatusChange(ticket.id, newState);
    }
  };

  return (
    <div>
      <h3>{ticket.title}</h3>
      <p>Descrição: {ticket.description}</p>
      <p>Estado: {ticket.state}</p>
      {ticket.state === TicketStates.NOVO && (
        <button onClick={() => handleStatusChange(TicketStates.AGUARDANDO_RESOLUCAO)}>
          Mover para Aguardando Resolução
        </button>
      )}
      {ticket.state === TicketStates.AGUARDANDO_RESOLUCAO && (
        <button onClick={() => handleStatusChange(TicketStates.AGUARDANDO_VALIDACAO)}>
          Mover para Aguardando Validação
        </button>
      )}
      {ticket.state === TicketStates.AGUARDANDO_VALIDACAO && (
        <button onClick={() => handleStatusChange(TicketStates.CONCLUIDO)}>
          Validar e Concluir
        </button>
      )}
    </div>
  );
}

export default Ticket;
