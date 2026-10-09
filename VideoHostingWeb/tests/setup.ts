import '@testing-library/jest-dom';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { beforeAll, afterEach, afterAll } from 'vitest';

export const handlers = [
  http.get('/api/Videos/GetVideos', () => {
    return HttpResponse.json([
      { id: 1, title: 'Sample Video' }
    ]);
  }),
];

export const server = setupServer(...handlers);

beforeAll(() => server.listen({ onUnhandledRequest: 'bypass' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
