import db from '.';

export function initDatabase() {
  db.prepare(
    `
    CREATE TABLE IF NOT EXISTS profiles (
      id TEXT PRIMARY KEY,
      name TEXT NOT NULL,
      is_active INTEGER NOT NULL DEFAULT 0
    )
  `
  ).run();

  db.prepare(
    `
    CREATE TABLE IF NOT EXISTS profile_settings (
      profile_id TEXT,
      key TEXT,
      value TEXT,
      PRIMARY KEY (profile_id, key),
      FOREIGN KEY (profile_id) REFERENCES profiles(id) ON DELETE CASCADE
    )
  `
  ).run();

  const count = db.prepare('SELECT COUNT(*) AS count FROM profiles').get().count;

  if (count === 0) {
    db.prepare('INSERT INTO profiles (id, name, is_active) VALUES (?, ?, ?)').run('default', 'Default', 1);
  }
}
