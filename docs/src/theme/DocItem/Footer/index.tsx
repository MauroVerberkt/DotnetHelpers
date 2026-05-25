/**
 * Swizzled DocItem/Footer — tags removed (rendered at top via Layout swizzle).
 */

import React, {type ReactNode} from 'react';
import clsx from 'clsx';
import {ThemeClassNames} from '@docusaurus/theme-common';
import {useDoc} from '@docusaurus/plugin-content-docs/client';
import EditMetaRow from '@theme/EditMetaRow';

export default function DocItemFooter(): ReactNode {
  const {metadata} = useDoc();
  const {editUrl, lastUpdatedAt, lastUpdatedBy} = metadata;

  const canDisplayEditMetaRow = !!(editUrl || lastUpdatedAt || lastUpdatedBy);

  if (!canDisplayEditMetaRow) {
    return null;
  }

  return (
    <footer
      className={clsx(ThemeClassNames.docs.docFooter, 'docusaurus-mt-lg')}>
      <EditMetaRow
        className={clsx(
          'margin-top--sm',
          ThemeClassNames.docs.docFooterEditMetaRow,
        )}
        editUrl={editUrl}
        lastUpdatedAt={lastUpdatedAt}
        lastUpdatedBy={lastUpdatedBy}
      />
    </footer>
  );
}
