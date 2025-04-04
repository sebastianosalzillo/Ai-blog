import React, { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Home from './Home';
import ArticlePage from './ArticlePage';
import NavBar from './NavBar';

function App() {
  const [articles, setArticles] = useState([]);

  // Al montaggio, carica la lista degli articoli salvati dal localStorage
  useEffect(() => {
    const saved = localStorage.getItem('articoli');
    if (saved) {
      setArticles(JSON.parse(saved));
    }
  }, []);

  // Funzione per aggiungere un nuovo articolo
  const addArticle = (article) => {
    const newArticles = [...articles, article];
    setArticles(newArticles);
    localStorage.setItem('articoli', JSON.stringify(newArticles));
  };

  return (
    <BrowserRouter>
      <NavBar articles={articles} />
      <Routes>
        <Route path="/" element={<Home addArticle={addArticle} />} />
        <Route path="/article/:id" element={<ArticlePage articles={articles} />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
