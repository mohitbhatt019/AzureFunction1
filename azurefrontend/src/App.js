import logo from './logo.svg';
import './App.css';
import { BrowserRouter, Route, Router, Routes } from 'react-router-dom';
import Home from './Screeens/Home';
import Header from './Screeens/Header';
import Login from './Screeens/Login';

function App() {
  return (
    <div className="App">
        <BrowserRouter>
        <Header/>
        <Routes>
          <Route path='home' element={<Home/>}/>
          <Route path='' element={<Home/>}/>
          <Route path='login' element={<Login/>}/>
        </Routes>
        </BrowserRouter>
    </div>
  );
}

export default App;
