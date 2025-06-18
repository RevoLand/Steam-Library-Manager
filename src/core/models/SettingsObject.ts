type Primitive = string | number | boolean | null;
export type SettingsObject = { [key: string]: Primitive | SettingsObject };
