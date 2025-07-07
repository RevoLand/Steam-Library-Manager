import { scan } from 'react-scan/all-environments';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { initializeRendererPlatforms } from 'src/features/platforms/init-renderer';
import App from './_App';
import LibraryProvider from './context/LibraryContext';
import ProfileProvider from './context/ProfileContext';
import TaskProvider from './context/TaskContext';
import './index.css';

initializeRendererPlatforms();

scan({
  enabled: true,
  trackUnnecessaryRenders: true,
});

// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
const root = createRoot(document.getElementById('root')!);

root.render(
  <StrictMode>
    <BrowserRouter>
      <ProfileProvider>
        <LibraryProvider>
          <TaskProvider>
            <App />
          </TaskProvider>
        </LibraryProvider>
      </ProfileProvider>
    </BrowserRouter>
  </StrictMode>
);
