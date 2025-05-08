import React from 'react';
import headerimage from '../images/header_homepage_image.png';
import './styles/HeaderHomepageStyle.css';


function HeaderHomepage() {
  return (
    <div className="ContainerHeaderHomepage">
      <div className="column-left">
        <div className="quickroom text">QuickRoom Solution</div>
        <div className="hotelfor text">
          Hotel for every <br />
          moment rich in
          <br />
          emotion
        </div>
        <div className="everymoment text">
          Every moment feels like the first time
          <br />
          in paradise view
        </div>
      </div>
      <div className="column-right">
        <img
          loading="lazy"
          src={headerimage} // Substitua "headerimage" pelo caminho da sua imagem
          className="imgheaderhomepage"
        />
      </div>
    </div>
  );
}

export default HeaderHomepage;
