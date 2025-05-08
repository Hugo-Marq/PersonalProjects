import React, { useEffect, useState } from "react";
import axios from "axios";
import "../styles/CleaningTicketsStyle.css";
import { getAuthHeaders } from "../FetchHeader";

const CleaningTickets = () => {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const headers = getAuthHeaders(); // Get the latest headers
  const username = localStorage.getItem("username"); // Retrieve username from localStorage
  if (!username) {
    throw new Error("User not logged in");
  }

  const fetchTickets = async () => {
    setLoading(true);
    try {
      const response = await axios.get(
        `https://localhost:7258/api/TicketLimpeza`,
        { headers: headers }
      );
      setTickets(response.data);
    } catch (error) {
      setError(error);
    } finally {
      setLoading(false);
    }
  };

  // UseEffect para buscar os tickets
  useEffect(() => {
    fetchTickets();
  }, []);

  const iniciarTrabalho = async (ticketId) => {
    try {
      await axios.put(
        `https://localhost:7258/api/TicketLimpeza/InicializarLimpeza`,
        null,
        {
          headers: headers,
          params: {
            id: ticketId,
            AuxLimpezaID: username,
          },
        }
      );
      fetchTickets(); // Refresh tickets after action
    } catch (error) {
      console.error("Error initializing cleaning:", error);
    }
  };

  const finalizarServico = async (ticketId) => {
    try {
      await axios.put(
        `https://localhost:7258/api/TicketLimpeza/FinalizarLimpeza`,
        null,
        {
          headers: headers,
          params: {
            id: ticketId,
            estadoFinalizacao: 3,
          },
        }
      );
      fetchTickets(); // Refresh tickets after action
    } catch (error) {
      console.error("Error finalizing cleaning:", error);
    }
  };

  const finalizarComProblemas = async (ticketId) => {
    try {
      await axios.put(
        `https://localhost:7258/api/TicketLimpeza/FinalizarLimpeza`,
        null,
        {
          headers: headers,
          params: {
            id: ticketId,
            estadoFinalizacao: 4,
          },
        }
      );
      fetchTickets(); // Refresh tickets after action
    } catch (error) {
      console.error("Error finalizing cleaning:", error);
    }
  };

  if (loading) return <p>Loading...</p>;
  if (error) return <p>{error.message}</p>;

  // Ordenar os tickets por estado
  const sortedTickets = tickets.sort((a, b) => {
    if (a.limpezaEstado < b.limpezaEstado) {
      return -1;
    }
    if (a.limpezaEstado > b.limpezaEstado) {
      return 1;
    }
    return 0;
  });

  // Function to map state numbers to their corresponding text descriptions
  const getEstadoText = (estado) => {
    switch (estado) {
      case 1:
        return "Pendente";
      case 2:
        return "Iniciada";
      case 3:
        return "Finalizada";
      case 4:
        return "Finalizada com Problemas";
      default:
        return "Desconhecido";
    }
  };

  return (
    <div className="cleaning-tickets-container">
      <h1>Cleaning Tickets</h1>
      <ul className="ticket-list">
        {sortedTickets.map((ticket) => (
          <li key={ticket.limpezaId} className="ticket-item">
            <p className="ticket-info">ID: {ticket.limpezaId}</p>
            <p className="ticket-info">Room: {ticket.quartoQuartoId}</p>
            <p className="ticket-info">State: {getEstadoText(ticket.limpezaEstado)}</p>
            <p className="ticket-info">Priority: {ticket.limpezaPrioridade}</p>
            <p className="ticket-info">
              Created At: {new Date(ticket.limpezaDataCriacao).toLocaleString()}
            </p>
            {ticket.limpezaDataAtualizacao && (
              <p className="ticket-info">
                Updated At:{" "}
                {new Date(ticket.limpezaDataAtualizacao).toLocaleString()}
              </p>
            )}
            {ticket.limpezaEstado === 1 && (
              <button
                className="action-button"
                onClick={() => iniciarTrabalho(ticket.limpezaId)}
              >
                Iniciar Trabalho
              </button>
            )}
            {ticket.limpezaEstado === 2 && (
              <>
                <button
                  className="action-button"
                  onClick={() => finalizarServico(ticket.limpezaId)}
                >
                  Finalizar Serviço
                </button>
                <button
                  className="action-button"
                  onClick={() => finalizarComProblemas(ticket.limpezaId)}
                >
                  Finalizar com Problemas
                </button>
              </>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default CleaningTickets;
