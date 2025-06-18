import { Suspense } from 'react';
import { useRoutes } from 'react-router-dom';
// eslint-disable-next-line import/no-unresolved
import routes from '~react-pages';
import Layout from './components/Layout';

export default function App() {
  return (
    <Suspense fallback={<p>Loading...</p>}>
      {useRoutes([
        {
          path: '/',
          element: <Layout />,
          children: routes,
        },
      ])}
    </Suspense>
  );
}
