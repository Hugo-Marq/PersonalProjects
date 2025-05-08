// para não repetir o mesmo código em multiplos

import room1 from "../../images/room1.png";
import room2 from "../../images/room2.png";
import room3 from "../../images/room3.png";

export const getRoomDescriptionByID = (id) => {
    switch (id) {
      case '1':
        return 'The Royal Single Room';
      case '2':
        return 'The Royal Double Room';
      case '3':
        return 'The Royal Suite';
      default:
        return 'Undefined Room';
    }
  };
  

  export const getRoomImageByID = (id) => {
    switch (id) {
      case '1':
        return room1;
      case '2':
        return room2;
      case '3':
        return room3;
      default:
        return null; // Ou a imagem padrão que você deseja exibir para IDs desconhecidos
    }
  };
  

export const getNumberOfDaysBetweenDates = (startDateString, endDateString) => {
  const startDate = new Date(startDateString);
  const endDate = new Date(endDateString);
  const millisecondsPerDay = 1000 * 60 * 60 * 24;
  const differenceInMilliseconds = endDate - startDate;
  const numberOfDays = Math.ceil(differenceInMilliseconds / millisecondsPerDay);
  return numberOfDays;
}