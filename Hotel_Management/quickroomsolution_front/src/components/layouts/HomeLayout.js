// src/components/layouts/HomeLayout.js

import React from 'react';
import Navbar from '../Navbar';
import Footer from '../Footer';
import HeaderHomepage from '../HeaderHomepage';
import NavbarSearch from '../NavbarSearch';
import Facilities from '../Facilities';
import LuxuryRooms from '../LuxuryRooms';
import Reception from '../Reception';

import '../styles/HeaderHomepageStyle.css';
import '../styles/FacilitiesStyle.css';
import '../styles/FooterStyle.css';
import '../styles/NavbarStyle.css';
import '../styles/NavbarSearchStyle.css';
import '../styles/LuxuryRoomsStyle.css';
import '../styles/ImageCardStyle.css';
import '../styles/ReceptionStyle.css';

function HomePage({ userType }) {
  return (
    <div className="HomePage">
      <HeaderHomepage className="HeaderHomepage" />
      {userType === 'cliente' && <NavbarSearch className="NavbarSearch" />}
      <Facilities className="Facilities" />
      <LuxuryRooms className="LuxuryRooms" />
      <Reception className="Reception" />
    </div>
  );
}

export default HomePage;
