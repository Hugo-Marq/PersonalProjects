import "./styles/NavbarSearchStyle.css"; // Verifique o caminho do arquivo de estilos
import roomtype from "../images/roomtype.png"; // Verifique o caminho da imagem
import person from "../images/person.png"; // Verifique o caminho da imagem
import Checkin from "../images/Checkin.png";
import Checkout from "../images/Checkin.png";
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { getNumberOfDaysBetweenDates } from "./Pages/roomUtilities"

function NavbarSearch() {
  const [id, setId] = useState("");
  const [inicio, setInicio] = useState("");
  const [fim, setFim] = useState("");
  const navigate = useNavigate();

  const handleSearch = async () => {
    const numberOfDays = getNumberOfDaysBetweenDates(inicio, fim);
    if (id === "" || inicio === "" || fim === "") {
      alert("Please fill in all fields");
      return;
    }
    else if ( numberOfDays < 1) {
      alert("Reservation days must 1 or greater");
      return;
    }
    
    try {
      const response = await fetch('https://localhost:7258/api/Quartos/Can-Book', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ id, inicio, fim })
      });
  
      if (response.ok) {
        const data = await response.json(); // Parse response body as JSON
        const existe = data.canBook; // Assuming the boolean value is under 'canBook' key
        if (existe) {
          // Room is available
          navigate("/search-result-client", { state: { id, inicio, fim } });
          
          console.log("Room is available. Proceeding...");
        } else {
          // Room is not available
          alert('No rooms available for the selected dates.');
        }
      } else {
        // Handle non-OK response
        alert('Failed to check room availability.');
      }
    } catch (error) {
      // Handle network or other errors
      console.error('Error fetching available rooms:', error);
      alert('An error occurred while checking availability.');
    }
  };

  return (
    <div className="room-search-navbar">
      <div className="input-group">
        <div className="icon-label">
          <img src={roomtype} alt="Room Type" className="icon" />
          <label className="input-label">Room Type</label>
        </div>
        <select
          value={id}
          onChange={(e) => setId(e.target.value)}
          className="dropdown"
        >
          <option value="" disabled>
            Select Room Type
          </option>
          <option value="1">Single</option>
          <option value="2">Double</option>
          <option value="3">Suite</option>
        </select>
      </div>

      <div className="input-group">
        <div className="icon-label">
          <img src={Checkin} alt="Check-in" className="icon" />
          <label className="input-label">Check-in</label>
        </div>
        <input
          type="date"
          value={inicio}
          onChange={(e) => setInicio(e.target.value)}
          className="date-input"
        />
      </div>

      <div className="input-group">
        <div className="icon-label">
          <img src={Checkout} alt="Check-out" className="icon" />
          <label className="input-label">Check-out</label>
        </div>
        <input
          type="date"
          value={fim}
          onChange={(e) => setFim(e.target.value)}
          className="date-input"
        />
      </div>

      <div className="input-group search-group">
        <button className="search-button" onClick={handleSearch}>
          BOOK NOW
        </button>
      </div>
    </div>
  );
}
export default NavbarSearch;