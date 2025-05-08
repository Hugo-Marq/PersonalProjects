import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom"; // Import useNavigate hook
import { getRoomDescriptionByID, getRoomImageByID } from './roomUtilities';
import '../styles/SearchResultClientStyle.css';

function SearchResultClient() {
  const location = useLocation();
  const navigate = useNavigate(); // Initialize useNavigate
  const { id, inicio, fim } = location.state || {};

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [tipologiaData, setTipologiaData] = useState(null); // State to store fetched data
  const [price, setPrice] = useState('');

  useEffect(() => {
    if (id) {
      const fetchTipologia = async () => {
        try {
          const response = await fetch(`https://localhost:7258/api/Tipologias/${id}`);
          if (!response.ok) {
            throw new Error(`Error: ${response.statusText}`);
          }
          const tipologiaData = await response.json();
          setTipologiaData(tipologiaData); // Set the fetched data to state
          console.log(tipologiaData); // For debugging purposes
          setPrice(tipologiaData.preco);
        } catch (error) {
          setError(error.message);
        } finally {
          setLoading(false);
        }
      };

      fetchTipologia();
    }
  }, [id]); // Effect runs again if `id` changes

  if (loading) {
    return <p>Loading...</p>;
  }

  if (error) {
    return <p>Error: {error}</p>;
  }

  const roomDescription = getRoomDescriptionByID(id);
  const roomImage = getRoomImageByID(id);
  

  const handleCancel = () => {
    navigate("/");
  };

  const handleConfirm = () => {
    const username = localStorage.getItem("username"); // Assuming username is stored in localStorage after login
    navigate("/book-confirmation-page", { state: { username, id, inicio, fim, price } });
  };

  return (
    <div className="search-result-container">
      <h1 className="search-results-title">Search Results</h1>
      <div className="image-and-text-container">
      <div className="image-container">
      {roomImage && <img src={roomImage} alt={roomDescription} className="room-image" />}
      </div>
      <div className="info-container">
        <p><strong>Room type:</strong> {roomDescription}</p>
        <p><strong>Check-in date:</strong> {inicio}</p>
        <p><strong>Check-out date:</strong> {fim}</p>
        <p><strong>Price per night € :</strong> {tipologiaData.preco}</p>
      </div>
        </div>
      <div className="buttons-container">
        <button onClick={handleCancel}>Cancel</button>
        <button onClick={handleConfirm}>Confirm</button>
      </div>
    </div>
  );
}

export default SearchResultClient;