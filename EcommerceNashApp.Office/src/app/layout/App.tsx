// src/App.tsx
import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Sidebar from './Sidebar';
import Dashboard from './Dashboard';
import MyContext from './Context/MyContext';


const App: React.FC = () => {
  // Define context values
  const values = {
    user: 'John Doe',
    theme: 'light',
  };

  return (
    <BrowserRouter>
      <MyContext.Provider value={values}>
        <section className="main flex">
          <div className="sidebarWrapper w-[15%]">
            <Sidebar />
          </div>
          <div className="contentRight w-[85%] px-3">
            <Routes>
              <Route path="/" element={<Dashboard />} />
            </Routes>
          </div>
        </section>
      </MyContext.Provider>
    </BrowserRouter>
  );
};

export default App;