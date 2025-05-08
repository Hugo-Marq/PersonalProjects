import React, { useState, useEffect } from "react"; // Importe o useState do React
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Navbar from "./components/Navbar";
import Footer from "./components/Footer";
import HomePage from "./components/Pages/HomePage";
import LoginPage from "./components/Pages/LoginPage";
import HotelReservations from "./components/Pages/HotelReservations";
import ManageTickets from './components/Pages/ManageTickets';
//import ManageSuppliers from './Pages/ManageSuppliers';
import ManageBudgets from './components/Pages/ManageBudgets';
import RoomManagement from './components/Pages/RoomManagement';
import RoomDetails from "./components/Pages/RoomDetails";
//import AccountDetails from './Pages/AccountDetails';
import CleaningTickets from './components/Pages/CleaningTickets';
import MaintenanceTickets from './components/Pages/MaintenanceTickets';
import Budgets from './components/Pages/Budgets';
import SearchResultClient from './components/Pages/SearchResultClient';
import BookConfirmationPage from "./components/Pages/BookConfirmationPage";
import BookDetails from "./components/Pages/BookDetails";
import ChangeReservationRoom from "./components/Pages/ChangeReservationRoom";
import ManageCheckIn from './components/Pages/ManageCheckIn';
import ManageCheckOut from './components/Pages/ManageCheckOut';

import "./App.css";
import "./components/styles/FooterStyle.css";
import "./components/styles/NavbarStyle.css";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userType, setUserType] = useState("");
  const [nivelCargo, setNivelCargo] = useState("");
  const [username, setUsername] = useState("");

  useEffect(() => {
    const token = localStorage.getItem('token');
    const storedUserType = localStorage.getItem('userType');
    const storedNivelCargo = localStorage.getItem('nivelCargo');
    const storedUsername = localStorage.getItem('username');
  
    console.log("Token:", token); // Add this line to log the token
  
    if (token) {
      setIsLoggedIn(true);
      setUserType(storedUserType);
      setNivelCargo(storedNivelCargo);
      setUsername(storedUsername);
    }
  }, [isLoggedIn]);  // Add isLoggedIn as a dependency

  const handleLogout = () => {
    setIsLoggedIn(false);
    setUserType("");
    setNivelCargo("");
    setUsername("");
    localStorage.removeItem("token");
    localStorage.removeItem("userType");
    localStorage.removeItem("nivelCargo");
    localStorage.removeItem("username");
  };

  return (
    <Router>
      <div className="App">
        <header>
          <Navbar
            isLoggedIn={isLoggedIn}
            userType={userType}
            handleLogout={handleLogout}
            nivelCargo={nivelCargo}
          />
        </header>
        <main>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route
              path="/login"
              element={
                <LoginPage
                  setIsLoggedIn={setIsLoggedIn}
                  setUserType={setUserType}
                  setNivelCargo={setNivelCargo}
                  setUsername={setUsername}
                />
              }
            />
            <Route path="/hotel-reservations" element={<HotelReservations />} />
            <Route path="/manage-tickets" element={<ManageTickets />} />
            {/* <Route path="/manage-suppliers" element={<ManageSuppliers />} /> */}
            <Route path="/manage-budgets" element={<ManageBudgets />} />
            <Route path="/room-management" element={<RoomManagement />} />
            <Route path="/room-details" element={<RoomDetails />} />
            {/* <Route path="/account-details" element={<AccountDetails />} /> */}
            <Route path="/cleaning-tickets" element={<CleaningTickets />} />
            <Route path="/maintenance-tickets" element={<MaintenanceTickets />} />
            <Route path="/budgets" element={<Budgets />} />
            <Route path="/search-result-client" element={<SearchResultClient />} />
            <Route path="/book-confirmation-page" element={<BookConfirmationPage />} />
            <Route path="/book-details" element={<BookDetails />} />
            <Route path="/change-reservation-room" element={<ChangeReservationRoom />} />
            <Route path="/check-in" element={<ManageCheckIn />} />
            <Route path="/check-out" element={<ManageCheckOut />} />
          </Routes>
        </main>
        <footer>
          <Footer />
        </footer>
      </div>
    </Router>
  );
}

export default App;
