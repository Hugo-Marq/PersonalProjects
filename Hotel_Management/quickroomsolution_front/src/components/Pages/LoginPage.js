import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import banner_pages from '../../images/banner_pages.png';
import login from '../../images/login.png';
import '../styles/LoginPageStyle.css';
import LoginPageLayout from '../layouts/LoginPageLayout.js';

function LoginPage({ setIsLoggedIn, setUserType, setNivelCargo, setUsername }) {
  const [username, setUsernameLocal] = useState('');
  const [password, setPassword] = useState('');
  const [userType, setUserTypeLocal] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleLogin = async () => {
    if (username === '' || password === '' || userType === '') {
      setError('Please enter username, password, and select user type.');
      return;
    }
    let url;
    if (userType === 'Funcionario') {
      url = 'https://localhost:7258/api/Funcionario/login';
    } else if (userType === 'Cliente') {
      url = 'https://localhost:7258/api/Cliente/login';
    } else if (userType === 'Fornecedor') {
      url = 'https://localhost:7258/api/FuncionarioFornecedor/login';
    }
  
    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ ID: username, Password: password }),
      });
  
      const responseBody = await response.text();
      let data;
      try {
        data = JSON.parse(responseBody);
      } catch (parseError) { 
        setError('Tipo de utilizador incorreto!');
        return;
      }
  
      const token = data.Token || data.token;
      const nivelCargo = data.nivelCargo;
  
      // Store token in local storage
      localStorage.setItem('token', token);
      localStorage.setItem('userType', userType);
      localStorage.setItem('nivelCargo', nivelCargo);
      localStorage.setItem('username', username);
  
      // Set state variables
      setIsLoggedIn(true);
      setUserType(userType);
      setNivelCargo(nivelCargo);
      setUsername(username);
  
      // Navigate to home page after state updates
      navigate('/');
    } catch (error) {
      setError('Invalid username or password.');
    }
  };

  return (
    <LoginPageLayout>
      <div className="ContainerHeaderLoginPage">
        <img loading="lazy" src={banner_pages} className="banner_pages" alt="Login Banner" />
      </div>
      <div className="login-container">
        <div className="login-icon">
          <img src={login} alt="Login Icon" />
        </div>
        {error && <div className="error-message">{error}</div>}
        <div className="login-inputs">
          <label>
            Username:
            <input
              type="text"
              value={username}
              onChange={(e) => setUsernameLocal(e.target.value)}
            />
          </label>
          <label>
            Password:
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </label>
          <label>
            User Type:
            <select value={userType} onChange={(e) => setUserTypeLocal(e.target.value)}>
              <option value="" disabled>Select</option>
              <option value="Funcionario">Funcionario</option>
              <option value="Cliente">Cliente</option>
              <option value="Fornecedor">Fornecedor</option>
            </select>
          </label>
        </div>
        <button className="login-button" onClick={handleLogin}>Login</button>
      </div>
    </LoginPageLayout>
  );
}

export default LoginPage;
