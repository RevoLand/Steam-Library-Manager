import { useContext } from 'react';
import { LibraryContext } from '../context/LibraryContext';

const useLibraries = () => {
  return useContext(LibraryContext);
};

export default useLibraries;
