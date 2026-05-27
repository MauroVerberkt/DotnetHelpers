import React, {useState, useEffect, useRef, type ReactNode} from 'react';
import clsx from 'clsx';
import useIsBrowser from '@docusaurus/useIsBrowser';
import IconLightMode from '@theme/Icon/LightMode';
import IconDarkMode from '@theme/Icon/DarkMode';
import type {Props} from '@theme/ColorModeToggle';
import styles from './styles.module.css';

export default function ColorModeToggle({
  className,
  buttonClassName,
  value,
}: Props): ReactNode {
  const isBrowser = useIsBrowser();
  const [showTooltip, setShowTooltip] = useState(false);
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const handleClick = () => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    setShowTooltip(true);
    timeoutRef.current = setTimeout(() => {
      setShowTooltip(false);
    }, 1500);
  };

  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  return (
    <div className={clsx(styles.toggle, className)}>
      <button
        className={clsx(
          'clean-btn',
          styles.toggleButton,
          !isBrowser && styles.toggleButtonDisabled,
          buttonClassName,
        )}
        type="button"
        onClick={handleClick}
        disabled={!isBrowser}
        title="dark mode"
        aria-label="Toggle color mode"
      >
        <IconLightMode
          aria-hidden
          className={clsx(styles.toggleIcon, styles.lightToggleIcon)}
        />
        <IconDarkMode
          aria-hidden
          className={clsx(styles.toggleIcon, styles.darkToggleIcon)}
        />
      </button>
      <div className={`${styles.tooltip} ${showTooltip ? styles.tooltipVisible : ''}`}>
        <span className={styles.tooltipText}>No light mode.</span>
        <span className={styles.tooltipSub}>By design.</span>
      </div>
    </div>
  );
}
