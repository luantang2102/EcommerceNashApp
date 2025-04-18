import { createContext, useContext, ReactNode } from 'react';

interface MyContextType {
  user: string;
  theme: string;
}

const MyContext = createContext<MyContextType | undefined>(undefined);

export const useMyContext = (): MyContextType => {
  const context = useContext(MyContext);
  if (!context) {
    throw new Error('useMyContext must be used within a MyContext.Provider');
  }
  return context;
};

export const MyContextProvider: React.FC<{
  children: ReactNode;
  value: MyContextType;
}> = ({ children, value }) => {
  return <MyContext.Provider value={value}>{children}</MyContext.Provider>;
};

export default MyContext;