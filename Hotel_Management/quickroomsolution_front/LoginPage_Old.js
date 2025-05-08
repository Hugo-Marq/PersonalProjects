import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import banner_pages from '../images/banner_pages.png';
import login from '../images/login.png'; 
import './styles/LoginPageStyle.css';
import LoginPageLayout from './layouts/LoginPageLayout.js';

function LoginPage() {
  const [id, setId] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleLogin = async () => {
    if (id === '' || password === '') {
      setError('Please enter both ID and password.');
      return;
    }

    try {
      const response = await fetch('https://localhost:7258/api/Funcionario/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ ID: id, Password: password }),
      });

      if (!response.ok) {
        throw new Error('Login failed');
      }

      const data = await response.json();
      localStorage.setItem('token', data.token);  // Store the JWT token in localStorage
      navigate('/');  // Redirect to the home page or another destination
    } catch (error) {
      setError('Invalid ID or password.');
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
            ID:
            <input
              type="text"
              value={id}
              onChange={(e) => setId(e.target.value)}
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
        </div>
        <button className="login-button" onClick={handleLogin}>Login</button>
      </div>
    </LoginPageLayout>
  );
}

export default LoginPage;
