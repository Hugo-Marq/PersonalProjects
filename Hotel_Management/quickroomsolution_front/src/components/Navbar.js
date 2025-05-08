import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './styles/NavbarStyle.css';
import logoImg from '../images/logo.png';
import menuData from '../menuData'; // Ensure the path is correct
import menu from '../images/menu.png';


function Navbar({ isLoggedIn, userType, nivelCargo, handleLogout }) {
  const [selectedItem, setSelectedItem] = useState(null);
  const navigate = useNavigate();

  // Add debug statements to check props
  console.log('Navbar props:', { isLoggedIn, userType, nivelCargo });

  const handleItemClick = (item) => {
    setSelectedItem(item);
    navigate(item.path);
  };

  const handleLoginClick = () => {
    navigate('/login');
  };

  const handleLogoutClick = () => {
    handleLogout();
    navigate('/');
  };

  // Determine the user type based on nivelCargo
  const getUserTypeByNivelCargo = (nivelCargo) => {
    switch (nivelCargo) {
      case '1':
        return 'gerente';
      case '2':
        return 'rececionista';
      case '3':
        return 'limpeza';
      default:
        return userType; // fallback to userType if nivelCargo is not provided
    }
  };

  const effectiveUserType = getUserTypeByNivelCargo(nivelCargo);

  const renderDropdowns = () => {
    if (isLoggedIn && menuData[effectiveUserType]) {
      return (
        <div className="dropdownusers">
          <button className="dropbtnusers">
            <img src={menu} alt="Menu Icon" className="menu-users-icon" />
            Menu
          </button>
          <div className="dropdown-users-content">
            {menuData[effectiveUserType].map((item) => (
              <a key={item.path} onClick={() => handleItemClick(item)}>
                {item.label}
              </a>
            ))}
          </div>
        </div>
      );
    }
    return null;
  };

  return (
    <div className="navbar-container">
      <img loading="lazy" src={logoImg} className="logo" alt="Company Logo" />
      <div className="menu-users-items">
        <div className={`menu-item ${selectedItem === '/' ? 'selected' : ''}`} onClick={() => handleItemClick({ path: '/' })}>
          Home
        </div>
        <div className={`menu-item ${selectedItem === '/explore' ? 'selected' : ''}`} onClick={() => handleItemClick('/explore')}>
          Explore
        </div>
        <div className={`menu-item ${selectedItem === '/rooms' ? 'selected' : ''}`} onClick={() => handleItemClick('/rooms')}>
          Rooms
        </div>
        <div className={`menu-item ${selectedItem === '/about' ? 'selected' : ''}`} onClick={() => handleItemClick('/about')}>
          About
        </div>
        <div className={`menu-item ${selectedItem === '/contacts' ? 'selected' : ''}`} onClick={() => handleItemClick('/contacts')}>
          Contacts
        </div>
        {renderDropdowns()}
      </div>
      <div className="login-button" onClick={isLoggedIn ? handleLogoutClick : handleLoginClick}>
        {isLoggedIn ? 'Logout' : 'Login'}
      </div>
    </div>
  );
}

export default Navbar;