// Footer.js

import React from 'react';
import './styles/FooterStyle.css'; // Importe o arquivo de estilos

function Footer() {
  return (
    <footer className="FooterContainer">
      {/* Main Elements Container */}
      <div className="MainElementsContainer">
        {/* Company Info */}
        <div className="ColumnQuickroom">
          <h2 className="Title">QuickRoom Solution</h2>
          <p className="CompanyInfo">
            The service at the Hotel Monteleone was exceptional. There was absolutely no issue that was not addressed timely and with satisfactory results. We were particularly impressed with how the hotel staff anticipated our needs (periodically coming by the Board Room to check with us).
          </p>
        </div>

        {/* Company Links */}
        <div className="ColumnLinks">
          <h2 className="Title">Company</h2>
          <ul className="CompanyLinks">
            <li><a href="#">Privacy policy</a></li>
            <li><a href="#">Refund policy</a></li>
            <li><a href="#">F.A.Q</a></li>
          </ul>
        </div>

        {/* Social Media */}
        <div className="ColumnSocialMedia">
          <h2 className="Title">Social Media</h2>
          <ul className="SocialMedia">
            <li><a href="#">Facebook</a></li>
            <li><a href="#">Instagram</a></li>
            <li><a href="#">LinkedIn</a></li>
            <li><a href="#">Twitter</a></li>
          </ul>
        </div>
      </div>

      {/* Newsletter Container */}
      <div className="NewsletterContainer">
        <h2 className="NewsletterTitle">Newsletter Sign Up</h2>
        <div className="FormContainer">
          <input
            type="email"
            placeholder="Enter your email here...."
            className="EmailInput"
          />
          <button className="SubmitButton">Submit</button>
        </div>
      </div>

      {/* Footer Info */}
      <div className="FooterInfo">QuickRoom Solution 2024.</div>
    </footer>
  );
}

export default Footer;
