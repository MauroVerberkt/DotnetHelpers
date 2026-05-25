import React from 'react';
import Link from '@docusaurus/Link';
import {usePluginData} from '@docusaurus/useGlobalData';
import styles from './styles.module.css';

interface ProjectDocsProps {
  project: string;
}

interface DocEntry {
  id: string;
  title: string;
  description: string;
  permalink: string;
  tags: string[];
  category: 'decisions' | 'design';
}

interface PluginData {
  docs: DocEntry[];
}

export default function ProjectDocs({project}: ProjectDocsProps): React.JSX.Element {
  const {docs} = usePluginData('architecture-docs-data') as PluginData;

  const projectDocs = docs.filter((doc) => doc.tags.includes(project));

  const decisions = projectDocs
    .filter((doc) => doc.category === 'decisions')
    .sort((a, b) => a.id.localeCompare(b.id));

  const design = projectDocs
    .filter((doc) => doc.category === 'design')
    .sort((a, b) => a.title.localeCompare(b.title));

  return (
    <div className={styles.container}>
      <Link to="/architecture/overview" className={styles.backLink}>
        ← Back to Architecture Overview
      </Link>

      {decisions.length > 0 && (
        <section className={styles.section}>
          <h2>Decisions (ADRs)</h2>
          <div className={styles.cardGrid}>
            {decisions.map((doc) => (
              <DocCard key={doc.id} doc={doc} />
            ))}
          </div>
        </section>
      )}

      {design.length > 0 && (
        <section className={styles.section}>
          <h2>Design Documents</h2>
          <div className={styles.cardGrid}>
            {design.map((doc) => (
              <DocCard key={doc.id} doc={doc} />
            ))}
          </div>
        </section>
      )}

      {projectDocs.length === 0 && (
        <p className={styles.empty}>
          No documents tagged with <strong>{project}</strong> yet.
        </p>
      )}
    </div>
  );
}

function DocCard({doc}: {doc: DocEntry}): React.JSX.Element {
  return (
    <Link to={doc.permalink} className={styles.card}>
      <h3 className={styles.cardTitle}>{doc.title}</h3>
      {doc.description && (
        <p className={styles.cardDescription}>{doc.description}</p>
      )}
      <div className={styles.cardTags}>
        {doc.tags.map((tag) => (
          <span key={tag} className={styles.tag}>
            {tag}
          </span>
        ))}
      </div>
    </Link>
  );
}
