import Database from 'better-sqlite3';
import { app } from 'electron';
import path from 'node:path';

const dbPath = path.join(app.getPath('userData'), 'slm.db');

export default new Database(dbPath);
