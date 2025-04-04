// App.jsx
import React, { useState, useEffect } from 'react';
import { GoogleGenAI } from '@google/genai';

const NEWS_API_URL =
  'https://newsapi.org/v2/everything?q=lavoro&language=it&apiKey=393cedd6a4d84b509aca0f06dff7944d';
const GEMINI_API_KEY = 'AIzaSyAaenegCj9fk3ZKRxIhEcm9ZTqOxFMHH0g';

// Funzione per estrarre il contenuto JSON valido dal testo
function parseArticleJSON(text) {
  // Rimuove delimitatori di code block tipo ```json e spazi extra
  // Cerca la prima occorrenza di una stringa che inizia con "{" e termina con "}"
  const match = text.match(/{[\s\S]*}/);
  if (match) {
    try {
      return JSON.parse(match[0]);
    } catch (err) {
      console.error("Errore nel parsing JSON estratto:", err);
    }
  }
  // Se non viene trovato nulla, restituisce un fallback con il testo raw come introduzione
  return {
    introduzione: text,
    paragrafi: [],
    conclusione: ""
  };
}

function App() {
  const [article, setArticle] = useState(null);
  const [loading, setLoading] = useState(false);

  // Carica l'articolo salvato dal localStorage (se presente)
  useEffect(() => {
    const savedArticle = localStorage.getItem('articolo-lavoro');
    if (savedArticle) {
      try {
        setArticle(JSON.parse(savedArticle));
      } catch (err) {
        console.error("Errore nel parsing dell'articolo salvato:", err);
      }
    }
  }, []);

  // Recupera il primo articolo dalla risposta della NewsAPI
  async function fetchNews() {
    try {
      const response = await fetch(NEWS_API_URL);
      const data = await response.json();
      if (data.articles && data.articles.length > 0) {
        return data.articles[0];
      }
    } catch (error) {
      console.error("Errore durante il recupero delle notizie:", error);
    }
    return null;
  }

  // Genera il testo dell'articolo richiedendo un output strutturato in JSON
  async function generateArticleText(content) {
    const ai = new GoogleGenAI({ apiKey: GEMINI_API_KEY });
    const prompt = `Genera un articolo strutturato in JSON seguendo lo schema:
{
  "titolo": string,
  "introduzione": string,
  "paragrafi": [
    {
      "sottotitolo": string,
      "testo": string
    }
  ],
  "conclusione": string
}
Return: Article JSON.
Basati sulla seguente notizia:
${content}`;
    try {
      const response = await ai.models.generateContent({
        model: 'gemini-2.0-flash',
        contents: prompt,
      });
      // Restituisce il testo raw della risposta
      return response.text;
    } catch (error) {
      console.error("Errore nella generazione del testo:", error);
      return "";
    }
  }

  // Genera l'immagine correlata alla notizia con titolo e breve descrizione
  async function generateArticleImage(content) {
    const ai = new GoogleGenAI({ apiKey: GEMINI_API_KEY });
    const imagePrompt = `Genera un'immagine ad alta definizione che rappresenti la seguente notizia come se fosse una scena 3D realistica e dettagliata. L'immagine deve includere:
1. Un titolo ben visibile (sovrapposto) che riassuma il tema principale.
2. Una breve descrizione visiva posizionata nella parte inferiore.
Assicurati che il titolo sia in alto e la descrizione in basso, in modo da enfatizzare il messaggio della notizia.
Notizia:
${content}`;
    try {
      const response = await ai.models.generateContent({
        model: 'gemini-2.0-flash-exp-image-generation',
        contents: imagePrompt,
        config: {
          responseModalities: ['Text', 'Image'],
        },
      });
      let imageData = '';
      // Esamina ciascuna parte della risposta
      response.candidates[0].content.parts.forEach((part, index) => {
        console.log(`Parte ${index} della risposta immagine:`, part);
        if (part.inlineData && part.inlineData.data) {
          imageData = part.inlineData.data;
        }
      });
      if (imageData) {
        return {
          image: `data:image/png;base64,${imageData}`,
          prompt: imagePrompt,
        };
      } else {
        console.warn("Nessun dato inline trovato per l'immagine.");
      }
    } catch (error) {
      console.error("Errore nella generazione dell'immagine:", error);
    }
    return { image: "", prompt: imagePrompt };
  }

  // Funzione principale: gestisce l'intero processo (fetch notizia, generazione testo e immagine, costruzione oggetto articolo)
  async function handleGenerate() {
    setLoading(true);
    const newsArticle = await fetchNews();
    if (!newsArticle) {
      setLoading(false);
      return;
    }
    // Usa "content", "description" o "title" come input per l'IA
    const newsContent =
      newsArticle.content || newsArticle.description || newsArticle.title;

    // Genera il testo strutturato in JSON
    const generatedTextRaw = await generateArticleText(newsContent);
    const generatedTextJSON = parseArticleJSON(generatedTextRaw);

    // Genera l'immagine e ottieni il prompt usato
    const { image: generatedImage, prompt: imagePromptUsed } =
      await generateArticleImage(newsContent);

    // Costruisce l'oggetto articolo completo
    const completeArticle = {
      titolo: newsArticle.title,
      testoGenerato: generatedTextJSON,
      immagine: generatedImage,
      data: new Date().toISOString(),
      fonte: {
        nome: newsArticle.source?.name || 'Sconosciuta',
        url: newsArticle.url || '#',
      },
      promptImmagine: imagePromptUsed,
      tags: ['Lavoro', 'News', 'AI Generated']
    };

    localStorage.setItem('articolo-lavoro', JSON.stringify(completeArticle));
    setArticle(completeArticle);
    setLoading(false);
  }

  return (
    <div className="app-container">
      <h1 className="app-title">AI News Article Generator</h1>
      <button className="generate-button" onClick={handleGenerate} disabled={loading}>
        {loading ? "Generazione in corso..." : "Genera Articolo"}
      </button>

      {article && (
        <div className="article-card">
          <h2 className="article-title">{article.titolo}</h2>
          <div className="article-structured">
            {article.testoGenerato.introduzione && (
              <div className="article-introduzione">
                <h3>Introduzione</h3>
                <p>{article.testoGenerato.introduzione}</p>
              </div>
            )}
            {article.testoGenerato.paragrafi &&
              article.testoGenerato.paragrafi.map((para, index) => (
                <div key={index} className="article-paragraph">
                  <h4>{para.sottotitolo}</h4>
                  <p>{para.testo}</p>
                </div>
              ))}
            {article.testoGenerato.conclusione && (
              <div className="article-conclusione">
                <h3>Conclusione</h3>
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
        </div>
      )}
    </div>
  );
}

export default App;
