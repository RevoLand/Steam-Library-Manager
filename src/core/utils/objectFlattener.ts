import { SettingsObject } from '../models/SettingsObject';

export const flattenObject = (obj: SettingsObject, prefix = ''): Record<string, string> => {
  const result: Record<string, string> = {};

  for (const key in obj) {
    if (!Object.prototype.hasOwnProperty.call(obj, key)) {
      continue;
    }

    const fullKey = prefix ? `${prefix}.${key}` : key;
    const value = obj[key];

    if (typeof value === 'object' && value !== null) {
      // JSON.stringify ile serialize edilebilir objeyse doğrudan sakla
      result[fullKey] = JSON.stringify(value);
    } else {
      result[fullKey] = String(value);
    }
  }

  return result;
};

export const unflattenObject = (flatObj: Record<string, unknown>): SettingsObject => {
  const result: SettingsObject = {};

  for (const flatKey in flatObj) {
    const keys = flatKey.split('.');
    let current = result;

    keys.forEach((key, index) => {
      if (index === keys.length - 1) {
        current[key] = flatObj[flatKey] as SettingsObject;
      } else {
        current[key] ??= {};
        current = current[key] as SettingsObject;
      }
    });
  }

  return result;
};
