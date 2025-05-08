import React, { useState } from 'react';
import '../styles/BudgetsStyle.css'; // Importa o arquivo de estilos CSS
import {getAuthHeaders} from '../FetchHeader';

function Budgets() {
  const [orcamento, setOrcamento] = useState({
    empresaId: '',
    ticketId: '',
    valorOrcamento: '',
    observacoes: ''
  });
  const [showSuccessPopup, setShowSuccessPopup] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setOrcamento((prevOrcamento) => ({
      ...prevOrcamento,
      [name]: value,
    }));
  };

  const headers = getAuthHeaders(); // Get the latest headers
  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
        const budgetPayload = {
            ticketTicketId: orcamento.ticketId,
            fornecedorFornecedorId: orcamento.empresaId,
            valorOrcamento: orcamento.valorOrcamento,
            descricacaoOrcamento: orcamento.observacoes,
            orcamentoEstado: 1
            // add other necessary properties here
          };
      const response = await fetch('https://localhost:7258/api/Orcamento', {
        method: 'POST',
        headers: headers,
        body: JSON.stringify(budgetPayload),
      });

      if (!response.ok) {
        const errorMessage = await response.text();
        alert(errorMessage);
        return;
      }

      // Limpa o formulário após o envio bem-sucedido
      setOrcamento({
          empresaId: '',
          ticketId: '',
          valorOrcamento: '',
          observacoes: ''
      });

      // Exibe a pop-up de sucesso
      setShowSuccessPopup(true);
    } catch (error) {
      alert('Ocorreu um erro ao inserir o orçamento');
      console.error(error);
    }
  };

  const handleCancel = () => {
    // Limpa o formulário quando o botão "Cancelar" é clicado
    setOrcamento({
        empresaId: '',
        ticketId: '',
        valorOrcamento: '',
        observacoes: ''
    });
  };

  return (
    <div className="budgets-container">
      <h2>Inserir Orçamento</h2>
      <form onSubmit={handleSubmit}>
      <div>
          <label htmlFor="empresaId">EmpresaID:</label>
          <input
            type="text"
            id="empresaId"
            name="empresaId"
            value={orcamento.empresaId}
            onChange={handleChange}
          />
        </div>
        <div>
          <label htmlFor="ticketId">ID do Ticket:</label>
          <input
            type="text"
            id="ticketId"
            name="ticketId"
            value={orcamento.ticketId}
            onChange={handleChange}
          />
        </div>
        <div>
          <label htmlFor="valorOrcamento">Valor do Orçamento:</label>
          <input
            type="text"
            id="valorOrcamento"
            name="valorOrcamento"
            value={orcamento.valorOrcamento}
            onChange={handleChange}
          />
        </div>
        <div>
          <label htmlFor="observacoes">Observações:</label>
          <textarea
            id="observacoes"
            name="observacoes"
            value={orcamento.observacoes}
            onChange={handleChange}
          />
        </div>
        <div className="button-container">
          <button type="submit">Enviar Orçamento</button>
          <button type="button" onClick={handleCancel}>Cancelar</button>
        </div>
      </form>
      {/* Pop-up de sucesso */}
      {showSuccessPopup && (
        <div className="success-popup">
          <p>O orçamento foi enviado com sucesso!</p>
          <button onClick={() => setShowSuccessPopup(false)}>Fechar</button>
        </div>
      )}
    </div>
  );
}

export default Budgets;

