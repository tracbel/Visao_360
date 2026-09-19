/**
 * O que todo teste de componente ganha antes de rodar.
 *
 * - `@testing-library/jest-dom/vitest` traz as afirmações de DOM
 *   (`toBeInTheDocument`, `toHaveAttribute`) e os tipos delas;
 * - a limpeza depois de cada teste é explícita porque o vitest deste projeto
 *   roda **sem globais** (`globals: false`): sem `afterEach` global, a limpeza
 *   automática da testing-library não acontece, e um teste veria a tela do
 *   anterior.
 */

import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

afterEach(cleanup);
