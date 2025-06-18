import { useContext } from 'react';
import { ProfileContext } from '../context/ProfileContext';

const useProfiles = () => {
  return useContext(ProfileContext);
};

export default useProfiles;
