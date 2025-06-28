type Primitive = string | number | boolean | null;

type SettingsObject = { [key: string]: Primitive | SettingsObject };

export default SettingsObject;
