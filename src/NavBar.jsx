import React from 'react';
import { Link } from 'react-router-dom';

function NavBar({ articles }) {
  return (
    <nav className="nav-bar">
      <ul>
        <li>
          <Link to="/">Home</Link>
        </li>
        {articles.map((article) => (
          <li key={article.id}>
            <Link to={`/article/${article.id}`}>{article.titolo}</Link>
          </li>
        ))}
      </ul>
    </nav>
  );
}

export default NavBar;
