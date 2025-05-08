import React from "react";
import ImageCard from "./ImageCard";
import "./styles/LuxuryRoomsStyle.css"; // Importando o arquivo de estilos CSS
import room1 from "../images/room1.png"; // Importando a imagem do quarto 1
import room3 from "../images/room2.png"; // Importando a imagem do quarto 2
import room2 from "../images/room3.png"; // Importando a imagem do quarto 3
import luxury from "../images/luxury.png"; // Importando a imagem de fundo

function LuxuryRooms() {
  return (
    <section className="luxury-rooms-container">
      <div className="luxury-rooms-content" style={{ backgroundImage: `url(${luxury})` }}>
      <header className="luxury-rooms-header">Luxurious Rooms</header>
        <div className="luxury-room">
          <ImageCard
            imgSrc={room1}
            altText="Room 1"
          >
            <p>Television set, Extra sheets and Breakfast</p>
          </ImageCard>
        </div>
        <div className="luxury-room">
          <ImageCard
            imgSrc={room3}
            altText="Room 3"
          >
            <p>Television set, Extra sheets, Breakfast, and fireplace</p>
          </ImageCard>
        </div>
        <div className="luxury-room">
          <ImageCard
            imgSrc={room2}
            altText="Room 2"
          >
            <p>Television set, Extra sheets, Breakfast, fireplace, Console and bed rest</p>
          </ImageCard>
        </div>
      </div>
    </section>
  );
}

export default LuxuryRooms;
