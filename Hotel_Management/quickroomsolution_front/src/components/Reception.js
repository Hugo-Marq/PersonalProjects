
import React from "react";
import reception from "../images/reception.png"; 
import "./styles/ReceptionStyle.css";


function Reception() {
  return (
    <section className="reception-container">
      <img
        src={reception}
        alt="Hotel Reception"
        className="reception-image"
      />
      <div className="reception-text">Hotel Reception</div>
    </section>
  );
}

export default Reception;
