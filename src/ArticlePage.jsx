import React, { useEffect, useState } from 'react';
import { Helmet } from 'react-helmet';
import { useNavigate, useParams } from 'react-router-dom';

function ArticlePage({ articles }) {
  const { id } = useParams();
  const [article, setArticle] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (articles && articles.length > 0) {
      const found = articles.find((a) => a.id === id);
      setArticle(found);
    }
  }, [id, articles]);

  if (!article) {
    return <div className="app-container"><p>Nessun articolo disponibile.</p></div>;
  }

  return (
    <div className="app-container">
      <Helmet>
        <title>{article.titolo}</title>
        <meta name="description" content={article.testoGenerato.introduzione || 'Articolo generato da AI'} />
        <meta name="keywords" content="Lavoro, News, AI Generated, Notizie" />
        <meta name="robots" content="index, follow" />
      </Helmet>
      <div className="article-card">
        <h1 className="article-title">{article.titolo}</h1>
        <div className="article-structured">
          {article.testoGenerato.introduzione && (
            <div className="article-introduzione">
              <h2>Introduzione</h2>
              <p>{article.testoGenerato.introduzione}</p>
            </div>
          )}
          {article.testoGenerato.paragrafi &&
            article.testoGenerato.paragrafi.map((para, index) => (
              <div key={index} className="article-paragraph">
                <h3>{para.sottotitolo}</h3>
                <p>{para.testo}</p>
              </div>
            ))}
          {article.testoGenerato.conclusione && (
            <div className="article-conclusione">
              <h2>Conclusione</h2>
              <p>{article.testoGenerato.conclusione}</p>
            </div>
          )}
        </div>
        {article.immagine ? (
          <img className="article-image" src={article.immagine} alt="Immagine AI" />
        ) : (
          <p className="no-image-text">Nessuna immagine generata</p>
        )}
        <p className="article-source">
          Fonte:{" "}
          {article.fonte?.url ? (
            <a href={article.fonte.url} target="_blank" rel="noopener noreferrer">
              {article.fonte.nome}
            </a>
          ) : (
            'Sconosciuta'
          )}
        </p>
        <p className="article-prompt">
          <strong>Prompt immagine:</strong> {article.promptImmagine}
        </p>
        <p className="article-date">
          Pubblicato il: {new Date(article.data).toLocaleString()}
        </p>
        <p className="article-tags">
          Tags: {article.tags ? article.tags.join(', ') : 'Nessun tag'}
        </p>
        <button className="generate-button" onClick={() => navigate('/')}>
          Torna alla Home
        </button>
      </div>
    </div>
  );
}

export default ArticlePage;

