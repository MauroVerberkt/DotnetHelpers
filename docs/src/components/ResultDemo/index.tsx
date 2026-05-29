import React, { useState } from 'react';
import styles from './styles.module.css';

interface ResultDemoProps {
  title?: string;
}

export default function ResultDemo({ title = "Result<T> Demo" }: ResultDemoProps): React.JSX.Element {
  const [isSuccess, setIsSuccess] = useState<boolean>(true);
  const [value, setValue] = useState<string>('User { Name = "John" }');
  const [error, setError] = useState<string>('User not found');

  const toggleResult = () => {
    setIsSuccess(!isSuccess);
  };

  return (
    <div className={styles.container}>
      <h3>{title}</h3>

      <div className={styles.controls}>
        <button onClick={toggleResult} className={styles.button}>
          Toggle Success/Failure
        </button>
      </div>

      <div className={isSuccess ? styles.success : styles.failure}>
        <code>
          {isSuccess
            ? `Result.Success(${value})`
            : `Result.Failure<User>(Error.Create("${error}", "NOT_FOUND"))`
          }
        </code>
      </div>

      <div className={styles.properties}>
        <p><strong>IsSuccess:</strong> <span className={isSuccess ? styles.trueValue : styles.falseValue}>{isSuccess.toString()}</span></p>
        <p><strong>IsFailure:</strong> <span className={!isSuccess ? styles.trueValue : styles.falseValue}>{(!isSuccess).toString()}</span></p>
        <p>
          <strong>{isSuccess ? 'Data' : 'Error'}:</strong>{' '}
          {isSuccess ? value : error}
        </p>
      </div>
    </div>
  );
}
