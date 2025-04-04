import React, { useState, useEffect } from 'react';
import { GoogleGenAI } from '@google/genai';
import { useNavigate } from 'react-router-dom';

const NEWS_API_URL =
  'https://newsapi.org/v2/everything?q=lavoro&language=it&apiKey=393cedd6a4d84b509aca0f06dff7944d';
const GEMINI_API_KEY = 'AIzaSyAaenegCj9fk3ZKRxIhEcm9ZTqOxFMHH0g';

function parseArticleJSON(text) {
  const match = text.match(/{[\s\S]*}/);
  if (match) {
    try {
      return JSON.parse(match[0]);
    } catch (err) {
      console.error("Errore nel parsing JSON estratto:", err);
    }
  }
  return {
    introduzione: text,
    paragrafi: [],
    conclusione: ""
  };
}

function Home({ addArticle }) {
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

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

  async function generateArticleText(content) {
    const ai = new GoogleGenAI({ apiKey: GEMINI_API_KEY });
    const prompt = `Genera un articolo strutturato in JSON secondo lo schema seguente (senza delimitatori markdown):
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
      return response.text;
    } catch (error) {
      console.error("Errore nella generazione del testo:", error);
      return "";
    }
  }

  async function generateArticleImage(content) {
    const ai = new GoogleGenAI({ apiKey: GEMINI_API_KEY });
    const imagePrompt = `Genera un'immagine ad alta definizione che rappresenti la seguente notizia come se fosse una scena 3D realistica e dettagliata. L'immagine deve includere:
1. Un titolo sovrapposto in alto che riassuma il tema principale.
2. Una breve descrizione in basso che evidenzi il messaggio.
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
      response.candidates[0].content.parts.forEach((part) => {
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

  async function handleGenerate() {
    setLoading(true);
    const newsArticle = await fetchNews();
    if (!newsArticle) {
      setLoading(false);
      return;
    }
    const newsContent = newsArticle.content || newsArticle.description || newsArticle.title;
    const generatedTextRaw = await generateArticleText(newsContent);
    const generatedTextJSON = parseArticleJSON(generatedTextRaw);
    const { image: generatedImage, prompt: imagePromptUsed } = await generateArticleImage(newsContent);
    // Assegna un ID univoco all'articolo (ad esempio, usando Date.now())
    const completeArticle = {
      id: Date.now().toString(),
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

    addArticle(completeArticle);
    setLoading(false);
    navigate(`/article/${completeArticle.id}`);
  }

  return (
    <div className="app-container">
      <h1 className="app-title">AI News Article Generator</h1>
      <button className="generate-button" onClick={handleGenerate} disabled={loading}>
        {loading ? "Generazione in corso..." : "Genera Articolo"}
      </button>
    </div>
  );
}

export default Home;

