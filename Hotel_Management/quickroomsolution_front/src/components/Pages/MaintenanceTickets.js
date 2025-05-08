import React, { useState, useEffect } from 'react';
import '../styles/MaintenanceTicketsStyle.css';
import {getAuthHeaders} from '../FetchHeader';

const MaintenanceTickets = () => {
    const [ticketId, setTicketId] = useState('');
    const [tickets, setTickets] = useState([]);
    const [showSuccessPopup, setShowSuccessPopup] = useState(false);
    const [successMessage, setSuccessMessage] = useState('');

    const username = localStorage.getItem("username"); // Retrieve username from localStorage
    if (!username) {
        throw new Error("User not logged in");
    }

    useEffect(() => {
        fetchTickets();
    }, []);

    const headers = getAuthHeaders(); // Get the latest headers
    const handleInitializeMaintenance = async () => {
        try {
            const response = await fetch(`https://localhost:7258/api/Ticket/InicializarManutencao?id=${ticketId}&FuncFornecedorID=${username}`, {
                method: 'PUT',
                headers: headers
            });

            if (!response.ok) {
                const contentType = response.headers.get('Content-Type');
                if (contentType && contentType.includes('application/json')) {
                    const errorData = await response.json();
                    alert(errorData.message || 'Erro ao iniciar manutenção');
                } else {
                    const errorText = await response.text();
                    alert(errorText || 'Erro ao iniciar manutenção');
                }
                return;
            }

            setSuccessMessage('Manutenção iniciada com sucesso');
            setShowSuccessPopup(true);
            await fetchTickets(); // Fetch tickets after initializing maintenance
        } catch (error) {
            alert(error.message || 'Erro desconhecido ao iniciar manutenção');
        }
    };

    const handleFinalizeJob = async (ticketId) => {
        try {
            const response = await fetch(`https://localhost:7258/api/Ticket/FinalizarManutencao?id=${ticketId}`, {
                method: 'PUT',
                headers: headers
            });

            if (!response.ok) {
                const contentType = response.headers.get('Content-Type');
                if (contentType && contentType.includes('application/json')) {
                    const errorData = await response.json();
                    alert(errorData.message || 'Erro ao finalizar manutenção');
                } else {
                    const errorText = await response.text();
                    alert(errorText || 'Erro ao finalizar manutenção');
                }
                return;
            }

            setSuccessMessage('Manutenção finalizada com sucesso');
            setShowSuccessPopup(true);
            await fetchTickets(); // Fetch tickets after finalizing maintenance
        } catch (error) {
            alert(error.message || 'Erro desconhecido ao finalizar manutenção');
        }
    };

    const fetchTickets = async () => {
        try {
            const response = await fetch(`https://localhost:7258/api/Ticket/Filtrados-Por-FuncionarioFornecedor-E-Estado-Iniciado?FuncFornecedorID=${username}`,
                { headers: headers }
            );
            
            if (!response.ok) {
                const contentType = response.headers.get('Content-Type');
                if (contentType && contentType.includes('application/json')) {
                    const errorData = await response.json();
                    alert(errorData.message || 'Erro ao buscar tickets');
                } else {
                    const errorText = await response.text();
                    alert(errorText || 'Erro ao buscar tickets');
                }
                return;
            }

            const data = await response.json();
            setTickets(data);
        } catch (error) {
            alert(error.message || 'Erro desconhecido ao buscar tickets');
        }
    };

    return (
        <div className="maintenance-container">
            <h1>Ticket Management</h1>
            <div className="maintenance-form">
                <h2>Inicialize Job</h2>
                <form onSubmit={(e) => { e.preventDefault(); handleInitializeMaintenance(); }}>
                    <div>
                        <label>Ticket ID:</label>
                        <input type="text" value={ticketId} onChange={(e) => setTicketId(e.target.value)} />
                    </div>
                    <div className="button-container">
                        <button type="submit">Inicialize Job</button>
                    </div>
                </form>
            </div>
            {tickets.length > 0 && (
                <div className="tickets-list">
                    <h2>Tickets with status "Iniciated"</h2>
                    <ul>
                        {tickets.map(ticket => (
                            <li key={ticket.ticketId}>
                                {ticket.ticketId} - {ticket.ticketDescricao}
                                {ticket.ticketEstado !== 5 && (
                                    <button onClick={() => handleFinalizeJob(ticket.ticketId)}>Finalize Job</button>
                                )}
                            </li>
                        ))}
                    </ul>
                </div>
            )}
            {showSuccessPopup && (
                <div className="success-popup">
                    <p>{successMessage}</p>
                    <button onClick={() => setShowSuccessPopup(false)}>Close</button>
                </div>
            )}
        </div>
    );
};

export default MaintenanceTickets;
