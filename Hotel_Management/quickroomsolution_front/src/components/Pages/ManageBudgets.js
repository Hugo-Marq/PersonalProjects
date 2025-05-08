import React, { useState, useEffect } from 'react';
import '../styles/ManageBudgetsStyle.css';
import { getAuthHeaders } from '../FetchHeader';

function ManageBudgets() {
  const [budgets, setBudgets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [actionType, setActionType] = useState(null);
  const [selectedBudgetId, setSelectedBudgetId] = useState(null);
  const [filterChecked, setFilterChecked] = useState(false);

  const headers = getAuthHeaders(); // Get the latest headers
  useEffect(() => {
    const fetchBudgets = async () => {
      try {
        const url = filterChecked
          ? 'https://localhost:7258/api/Orcamento/Filtrados-Por-Estado'
          : 'https://localhost:7258/api/Orcamento';

        const response = await fetch(url, {
          method: 'GET',
          headers: headers,
        });

        if (!response.ok) {
          alert('No Pending Budgets');
        }

        const data = await response.json();
        setBudgets(data);
      } catch (error) {
        setError(error.message);
      } finally {
        setLoading(false);
      }
    };

    fetchBudgets();
  }, [filterChecked]);

  const handleAcceptBudget = (budgetId) => {
    setActionType('accept');
    setSelectedBudgetId(budgetId);
    setDialogOpen(true);
  };

  const handleRejectBudget = (budgetId) => {
    setActionType('reject');
    setSelectedBudgetId(budgetId);
    setDialogOpen(true);
  };

  const handleConfirmAction = async () => {
    try {
      let url = '';
      if (actionType === 'accept') {
        url = 'https://localhost:7258/api/Orcamento/Aceitar-Orcamento';
      } else if (actionType === 'reject') {
        url = 'https://localhost:7258/api/Orcamento/Rejeitar-Orcamento';
      }

      const budget = budgets.find((b) => b.orcamentoId === selectedBudgetId);

      const budgetPayload = {
        orcamentoId: budget.orcamentoId,
        ticketTicketId: budget.ticketTicketId,
        fornecedorFornecedorId: budget.fornecedorFornecedorId,
        valorOrcamento: budget.valorOrcamento,
        descricacaoOrcamento: budget.descricacaoOrcamento,
        orcamentoEstado: 1
      };

      const response = await fetch(url, {
        method: 'PUT',
        headers: headers,
        body: JSON.stringify(budgetPayload),
      });
      console.log(headers);
      if (!response.ok) {
        const errorMessage = await response.text();
        alert(errorMessage);
        return;
      }

      const updatedBudget = await response.json();

      // Update the state to reflect the changes
      setBudgets((prevBudgets) =>
        prevBudgets.map((b) =>
          b.orcamentoId === updatedBudget.orcamentoId ? updatedBudget : b
        )
      );

      const message = actionType === 'reject'
        ? 'Budget foi rejeitado.'
        : 'Budget foi validado.';
      alert(message);
      setDialogOpen(false);
    } catch (error) {
      alert('An error occurred while updating the Budget status');
      console.error(error);
    }
  };

  const getBudgetStatus = (estado) => {
    switch (estado) {
      case 2:
        return 'Accepted';
      case 3:
        return 'Rejected';
      default:
        return 'Pending';
    }
  };

  return (
    <div className="manage-budgets-container">
      <h1>Budget List</h1>
      <div className="filter-checkbox">
        <label>
          <input
            type="checkbox"
            checked={filterChecked}
            onChange={() => setFilterChecked(!filterChecked)}
          />
          Filter by Pending
        </label>
      </div>
      {loading && <p>Loading...</p>}
      {error && <p className="error-message">Error: {error}</p>}
      {budgets.length === 0 && !loading && <p className="no-budgets">No budgets to display.</p>}
      {budgets.length > 0 && (
        <ul className="budget-list">
          {budgets.map((budget) => (
            <li key={budget.orcamentoId} className={`budget-container ${budget.orcamentoEstado === 2 ? 'accepted' : budget.orcamentoEstado === 3 ? 'rejected' : ''}`}>
              <div className="budget-details">
                <p>Budget ID: {budget.orcamentoId}</p>
                <p>Amount: {budget.valorOrcamento}</p>
                <p>Description: {budget.descricacaoOrcamento}</p>
                <p>Status: {getBudgetStatus(budget.orcamentoEstado)}</p>
                <p>Refer to ticket: {budget.ticketTicketId}</p>
              </div>
              {budget.orcamentoEstado === 2 ? (
                <div className="accepted-budget">
                  <p className="accepted-budget-text">BUDGET ACCEPTED</p>
                </div>
              ) : budget.orcamentoEstado === 3 ? (
                <div className="rejected-budget">
                  <p className="rejected-budget-text">BUDGET REJECTED</p>
                </div>
              ) : (
                <div className="budget-actions">
                  <button onClick={() => handleAcceptBudget(budget.orcamentoId)}>Accept Budget</button>
                  <button onClick={() => handleRejectBudget(budget.orcamentoId)}>Reject Budget</button>
                </div>
              )}
            </li>
          ))}
        </ul>
      )}
      {dialogOpen && (
        <div className="confirmation-dialog">
          <p>Are you sure you want to {actionType === 'accept' ? 'accept' : 'reject'} this budget?</p>
          <div>
            <button onClick={handleConfirmAction}>Yes</button>
            <button onClick={() => setDialogOpen(false)}>No</button>
          </div>
        </div>
      )}
    </div>
  );
}

export default ManageBudgets;
