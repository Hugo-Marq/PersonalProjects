import React from "react";
import "./styles/ImageCardStyle.css";

function ImageCard({imgSrc, altText, children}) {
  return (
    <div className="image-card">
      <img loading="lazy" src={imgSrc} alt={altText} className="room-image" />
      <div className="room-details">{children}</div>
    </div>
  );
}

export default ImageCard;
