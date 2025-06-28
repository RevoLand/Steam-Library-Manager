import { createContext, PropsWithChildren, useCallback, useEffect, useMemo, useState } from 'react';
import Profile from 'src/core/models/Profile';

interface ProfileContextValue {
  profiles?: Profile[];
  profile: Profile | null;
  refreshActiveProfile: () => Promise<void>;
}

export const ProfileContext = createContext<ProfileContextValue>(undefined);

ProfileContext.displayName = 'ProfileContext';

export default function ProfileProvider(props: PropsWithChildren) {
  const [profile, setProfile] = useState<Profile>();

  const refreshActiveProfile = useCallback(async () => {
    const activeProfile = await window.api['get-active-profile']();

    setProfile(activeProfile);
  }, []);

  useEffect(() => {
    refreshActiveProfile();
  }, []);

  const profileContextValue = useMemo(
    () => ({
      profile,
      refreshActiveProfile,
    }),
    [profile]
  );

  return <ProfileContext.Provider value={profileContextValue}>{props.children}</ProfileContext.Provider>;
}
