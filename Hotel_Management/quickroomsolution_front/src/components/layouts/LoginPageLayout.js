import React from 'react';
import '../styles/LoginPageStyle.css';
import Navbar from '../Navbar';
import Footer from '../Footer';



function LoginPageLayout({ children }) {
  return (
    <div>
      
      <main>
        {children}
      </main>
     </div>
  );
}

export default LoginPageLayout;

