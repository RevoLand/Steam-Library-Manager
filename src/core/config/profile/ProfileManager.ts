import { deleteProperty, getProperty, setProperty } from 'dot-prop';
import db from 'src/core/db';
import { Profile } from 'src/core/models/Profile';
import { SettingsObject } from 'src/core/models/SettingsObject';
import { flattenObject, unflattenObject } from 'src/core/utils/objectFlattener';

export class ProfileManager {
  static getProfile(profileId: string): Profile {
    return db.prepare('SELECT * FROM profiles WHERE id = ?').get(profileId) as Profile;
  }

  static getActiveProfile(): Profile {
    return db.prepare('SELECT * FROM profiles WHERE is_active = 1').get() as Profile;
  }

  static getProfiles(): Profile[] {
    return db.prepare('SELECT * FROM profiles').all() as Profile[];
  }

  static getSettings(): SettingsObject {
    const profile = this.getActiveProfile();
    const rows = db.prepare('SELECT key, value FROM profile_settings WHERE profile_id = ?').all(profile.id) as {
      key: string;
      value: string;
    }[];

    const flat: Record<string, unknown> = {};

    for (const row of rows) {
      try {
        flat[row.key] = JSON.parse(row.value);
      } catch {
        flat[row.key] = row.value;
      }
    }

    return unflattenObject(flat);
  }

  static saveSettings(obj: SettingsObject) {
    const profile = this.getActiveProfile();
    const flat = flattenObject(obj);

    const insert = db.prepare(`
    INSERT INTO profile_settings (profile_id, key, value)
    VALUES (?, ?, ?)
    ON CONFLICT(profile_id, key) DO UPDATE SET value=excluded.value
  `);

    const transaction = db.transaction(() => {
      for (const [key, value] of Object.entries(flat)) {
        insert.run(profile.id, key, value);
      }
    });

    transaction();
  }

  static getSetting<T = unknown>(key: string): T | undefined {
    const obj = this.getSettings();

    return getProperty(obj, key) as T;
  }

  static setSetting(key: string, value: unknown): void {
    const obj = this.getSettings();

    setProperty(obj, key, value);
    this.saveSettings(obj);
  }

  static deleteSetting(key: string): void {
    const obj = this.getSettings();

    deleteProperty(obj, key);
    this.saveSettings(obj);
  }
}
