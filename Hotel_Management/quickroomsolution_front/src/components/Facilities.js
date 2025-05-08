import React from "react";
import "./styles/FacilitiesStyle.css";
import swimming from "../images/swimming.png";
import wifi from "../images/wifi.png";
import breakfast from "../images/breakfast.png";
import gym from "../images/gym.png";
import game from "../images/game.png";
import light from "../images/light.png";
import laundry from "../images/laundry.png";
import parking from "../images/parking.png";

function Facility({ imageSrc, altText, title }) {
  return (
    <div className="facility">
      <div className="image-container">
        <img loading="lazy" src={imageSrc} alt={altText} className="imagefacilities" />
        <div className="facility-title">{title}</div>
      </div>
    </div>
  );
}

function Facilities() {
  return (
    <div className="containerfacilities">
      <div className="titlefacilities">Our Facilities</div>
      <div className="subtitlefacilities">We offer modern (5 star) hotel facilities for your comfort.</div>
      <div className="facility-container-line1">
        <Facility
          imageSrc={swimming}
          altText="Swimming Pool"
          title="Swimming Pool"
        />
        <Facility
          imageSrc={wifi}
          altText="Wifi"
          title="Wifi"
        />
        <Facility
          imageSrc={breakfast}
          altText="Breakfast"
          title="Breakfast"
        />
        <Facility
          imageSrc={gym}
          altText="Gym"
          title="Gym"
        />
      </div>
      <div className="facility-container-line2">
        <Facility
          imageSrc={game}
          altText="Game center"
          title="Game center"
        />
        <Facility
          imageSrc={light}
          altText="24/7 Light"
          title="24/7 Light"
        />
        <Facility
          imageSrc={laundry}
          altText="Laundry"
          title="Laundry"
        />
        <Facility
          imageSrc={parking}
          altText="Parking space"
          title="Parking space"
        />
      </div>
    </div>
  );
}

export default Facilities;

      