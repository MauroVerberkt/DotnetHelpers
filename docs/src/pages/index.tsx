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
        <p className="hero__subtitle">
          Production-grade building blocks for .NET — explicit error handling,
          compile-time validated business rules, and null-safe optionals.
        </p>
        <div className={styles.badges}>
          <a href="https://github.com/MauroVerberkt/DotnetHelpers/actions/workflows/ci.yml">
            <img src="https://github.com/MauroVerberkt/DotnetHelpers/actions/workflows/ci.yml/badge.svg" alt="CI" />
          </a>
          {' '}
          <a href="https://app.codecov.io/github/MauroVerberkt/DotnetHelpers">
            <img src="https://codecov.io/github/MauroVerberkt/DotnetHelpers/graph/badge.svg" alt="codecov" />
          </a>
          {' '}
          <img src="https://img.shields.io/badge/.NET-8.0+-purple.svg" alt=".NET 8.0+" />
          {' '}
          <img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="MIT License" />
        </div>
        <div className={styles.buttons}>
          <Link
            className="button button--secondary button--lg"
            to="/docs/getting-started/installation">
            Explore the Docs
          </Link>
          <Link
            className="button button--outline button--secondary button--lg margin-left--md"
            href="https://github.com/MauroVerberkt/DotnetHelpers">
            GitHub
          </Link>
        </div>
      </div>
    </header>
  );
}

function ValueProp({title, description, details}: {title: string; description: string; details: string[]}) {
  return (
    <div className={clsx('col col--4')}>
      <div className={styles.valueProp}>
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
        <ul className={styles.valuePropDetails}>
          {details.map((detail, idx) => (
            <li key={idx}>{detail}</li>
          ))}
        </ul>
      </div>
    </div>
  );
}

function HomepageValueProps() {
  return (
    <section className={styles.valueProps}>
      <div className="container">
        <div className="row">
          <ValueProp
            title="Explicit over Implicit"
            description="No exceptions for control flow. No nulls sneaking through. The type system tells you exactly what can fail and forces you to handle it."
            details={[
              'Result<T> for operations that can fail',
              'Option<T> for values that may be absent',
              'Compiler-enforced handling of both paths',
            ]}
          />
          <ValueProp
            title="Compile-time over Runtime"
            description="Source generators and Roslyn analyzers replace runtime reflection and magic strings. Errors are caught before your code ever runs."
            details={[
              'Business rules defined in JSON, generated as C# classes',
              'Analyzers validate correct usage at build time',
              'Zero runtime reflection cost',
            ]}
          />
          <ValueProp
            title="Composable by Design"
            description="Everything chains. Build pipelines where failures propagate automatically and operations compose naturally."
            details={[
              'Map, Bind, Match — full monadic composition',
              'Async and CancellationToken support throughout',
              'Independent packages, zero cross-dependencies',
            ]}
          />
        </div>
      </div>
    </section>
  );
}

function HomepageEngineering() {
  const items = [
    'Source generators over reflection',
    'Roslyn analyzers for compile-time validation',
    'Independent packages — use what you need',
    'Deterministic builds with SourceLink',
    'Full async/CancellationToken support',
    'XML docs and nullable annotations throughout',
  ];

  return (
    <section className={styles.engineering}>
      <div className="container">
        <Heading as="h2" className="text--center margin-bottom--lg">
          Engineering Principles
        </Heading>
        <div className={styles.engineeringGrid}>
          {items.map((item, idx) => (
            <div key={idx} className={styles.engineeringItem}>
              {item}
            </div>
          ))}
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
      description="Production-grade functional building blocks for .NET">
      <HomepageHeader />
      <main>
        <HomepageValueProps />
        <HomepageEngineering />
      </main>
    </Layout>
  );
}
