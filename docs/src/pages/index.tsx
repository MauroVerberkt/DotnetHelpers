import type {ReactNode} from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';

import styles from './index.module.css';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <Heading as="h1" className="hero__title">
          {siteConfig.title}
        </Heading>
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        <div className={styles.buttons}>
          <Link
            className="button button--secondary button--lg"
            to="/docs/getting-started/installation">
            Get Started
          </Link>
        </div>
      </div>
    </header>
  );
}

function Feature({title, description, emoji}: {title: string; description: string; emoji: string}) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center padding-horiz--md">
        <div style={{fontSize: '3rem', marginBottom: '1rem'}}>{emoji}</div>
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          <Feature
            emoji="🎯"
            title="Result<T> Pattern"
            description="Represent operation outcomes explicitly. No more exceptions for control flow - chain operations with Map, Bind, and transform data safely."
          />
          <Feature
            emoji="🔒"
            title="Business Rules"
            description="Define rules in JSON, get strongly-typed classes at compile time. Roslyn analyzers validate usage and source generators do the heavy lifting."
          />
          <Feature
            emoji="⚡"
            title="Option<T> Type"
            description="Eliminate null reference exceptions with an explicit Option type. Pattern match on presence or absence of values safely."
          />
        </div>
      </div>
    </section>
  );
}

export default function Home(): ReactNode {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title="Home"
      description="Functional patterns for .NET: Result, Option, and Business Rules">
      <HomepageHeader />
      <main>
        <HomepageFeatures />
      </main>
    </Layout>
  );
}
