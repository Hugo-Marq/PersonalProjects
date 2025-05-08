import React from 'react';
import HeaderHomepage from '../HeaderHomepage';
import NavbarSearch from '../NavbarSearch';
import Facilities from '../Facilities';
import LuxuryRooms from '../LuxuryRooms';
import Reception from '../Reception';


import '../styles/HeaderHomepageStyle.css';
import '../styles/NavbarSearchStyle.css';
import '../styles/FacilitiesStyle.css';
import '../styles/LuxuryRoomsStyle.css';
import '../styles/ReceptionStyle.css';

function HomePage() {
  return (
    <div>
      <HeaderHomepage className="HeaderHomepage" />
      <NavbarSearch className="NavbarSearch" />
      <Facilities className="Facilities" />
      <LuxuryRooms className="LuxuryRooms" />
      <Reception className="Reception" />
    </div>
  );
}

export default HomePage;
