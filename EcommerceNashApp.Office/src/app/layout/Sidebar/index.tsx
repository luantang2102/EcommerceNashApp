import React from 'react';
import Logo from '../../../assets/logo.png';
import { Link } from 'react-router-dom';
import { Button, Divider } from '@mui/material';
import { Category, Dashboard, Group, Inventory } from '@mui/icons-material';

const Sidebar: React.FC = () => {
  return (
    <>
      <div className="sidebar fixed top-0 left-0 z-[100] w-[15%]">
        <Link to="/">
          <div className="logoWrapper py-3 px-2 flex justify-center items-center">
            <img src={Logo} alt="Logo" className="w-100" />
          </div>
        </Link>


        <div className="sidebarTabs flex flex-col mt-3 items-start px-3">
            <Button 
              style={{ color: 'black', marginBottom: '2vh', justifyContent: 'flex-start', width: '100%' }}
            >
              <span className="icon mr-3 w-[25px] h-[25px] flex justify-center items-center rounded-md ">
          <Dashboard />
              </span>
              <span style={{ color: 'black', fontSize: 16, fontWeight: 600 }}>Dashboard</span>
            </Button>

          <Divider style={{ width: '100%', backgroundColor: '#E0E0E0', marginBottom: '2vh' }} />

            <Button 
              style={{ color: 'black', marginBottom: '2vh', justifyContent: 'flex-start', width: '100%' }}
            >
              <span className="icon mr-3 w-[25px] h-[25px] flex justify-center items-center rounded-md ">
          <Group />
              </span>
              <span style={{ color: 'black', fontSize: 16, fontWeight: 600 }}>Customers</span>
            </Button>

            <Button 
              style={{ color: 'black', marginBottom: '2vh', justifyContent: 'flex-start', width: '100%' }}
            >
              <span className="icon mr-3 w-[25px] h-[25px] flex justify-center items-center rounded-md ">
          <Inventory />
              </span>
              <span style={{ color: 'black', fontSize: 16, fontWeight: 600 }}>Products</span>
            </Button>
            
            <Button 
              style={{ color: 'black', marginBottom: '2vh', justifyContent: 'flex-start', width: '100%' }}
            >
              <span className="icon mr-3 w-[25px] h-[25px] flex justify-center items-center rounded-md ">
          <Category />
              </span>
              <span style={{ color: 'black', fontSize: 16, fontWeight: 600 }}>Categories</span>
            </Button>
        </div>
      </div>
    </>  
  );
};

export default Sidebar;