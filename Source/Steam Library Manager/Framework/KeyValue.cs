/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which can be found at https://github.com/SteamRE/SteamKit/blob/master/SteamKit2/SteamKit2/license.txt
 * All credits goes to SteamKit developers
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using File = Alphaleonis.Win32.Filesystem.File;

namespace Steam_Library_Manager.Framework
{
    internal class KVTextReader : StreamReader
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal static Dictionary<char, char> escapedMapping = new Dictionary<char, char>
        {
            { '\\', '\\' },
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' },
            // todo: any others?
        };

        public KVTextReader(KeyValue kv, Stream input) : base(input)
        {
            try
            {
                KeyValue currentKey = kv;

                do
                {
                    string s = ReadToken(out bool wasQuoted, out bool wasConditional);

                    if (string.IsNullOrEmpty(s))
                    {
                        break;
                    }

                    if (currentKey == null)
                    {
                        currentKey = new KeyValue(s);
                    }
                    else
                    {
                        currentKey.Name = s;
                    }

                    s = ReadToken(out wasQuoted, out wasConditional);

                    if (string.IsNullOrEmpty(s))
                    {
                        break;
                    }

                    if (wasConditional)
                    {
                        // bAccepted = ( s == "[$WIN32]" );

                        // Now get the '{'
                        s = ReadToken(out wasQuoted, out wasConditional);
                    }

                    if (s.StartsWith("{") && !wasQuoted)
                    {
                        // header is valid so load the file
                        currentKey.RecursiveLoadFromBuffer(this);
                    }
                    else
                    {
                        throw new Exception("LoadFromBuffer: missing {");
                    }

                    currentKey = null;
                }
                while (!EndOfStream);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        private void EatWhiteSpace()
        {
            while (!EndOfStream)
            {
                if (!char.IsWhiteSpace((char)Peek()))
                {
                    break;
                }

                Read();
            }
        }

        private bool EatCPPComment()
        {
            if (!EndOfStream)
            {
                char next = (char)Peek();
                if (next == '/')
                {
                    ReadLine();
                    return true;
                    /*
                     *  As came up in parsing the Dota 2 units.txt file, the reference (Valve) implementation
                     *  of the KV format considers a single forward slash to be sufficient to comment out the
                     *  entirety of a line. While they still _tend_ to use two, it's not required, and likely
                     *  is just done out of habit.
                     */
                }

                return false;
            }

            return false;
        }

        public string ReadToken(out bool wasQuoted, out bool wasConditional)
        {
            wasQuoted = false;
            wasConditional = false;

            while (true)
            {
                EatWhiteSpace();

                if (EndOfStream)
                {
                    return null;
                }

                if (!EatCPPComment())
                {
                    break;
                }
            }

            if (EndOfStream)
            {
                return null;
            }

            char next = (char)Peek();
            if (next == '"')
            {
                wasQuoted = true;

                // "
                Read();

                var sb = new StringBuilder();
                while (!EndOfStream)
                {
                    if (Peek() == '\\')
                    {
                        Read();

                        char escapedChar = (char)Read();
                        if (escapedMapping.TryGetValue(escapedChar, out char replacedChar))
                        {
                            sb.Append(replacedChar);
                        }
                        else
                        {
                            sb.Append(escapedChar);
                        }

                        continue;
                    }

                    if (Peek() == '"')
                    {
                        break;
                    }

                    sb.Append((char)Read());
                }

                // "
                Read();

                return sb.ToString();
            }

            if (next == '{' || next == '}')
            {
                Read();
                return next.ToString();
            }

            bool bConditionalStart = false;
            int count = 0;
            var ret = new StringBuilder();
            while (!EndOfStream)
            {
                next = (char)Peek();

                if (next == '"' || next == '{' || next == '}')
                {
                    break;
                }

                if (next == '[')
                {
                    bConditionalStart = true;
                }

                if (next == ']' && bConditionalStart)
                {
                    wasConditional = true;
                }

                if (char.IsWhiteSpace(next))
                {
                    break;
                }

                if (count < 1023)
                {
                    ret.Append(next);
                }
                else
                {
                    throw new Exception("ReadToken overflow");
                }

                Read();
            }

            return ret.ToString();
        }
    }

    public class KeyValue
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private enum Type : byte
        {
            None = 0,
            String = 1,
            Int32 = 2,
            Float32 = 3,
            Pointer = 4,
            WideString = 5,
            Color = 6,
            UInt64 = 7,
            End = 8,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValue"/> class.
        /// </summary>
        /// <param name="name">The optional name of the root key.</param>
        /// <param name="value">The optional value assigned to the root key.</param>
        public KeyValue(string name = null, string value = null)
        {
            Name = name;
            Value = value;

            Children = new List<KeyValue>();
        }

        /// <summary>
        /// Represents an invalid <see cref="KeyValue"/> given when a searched for child does not exist.
        /// </summary>
        public readonly static KeyValue Invalid = new KeyValue();

        /// <summary>
        /// Gets or sets the name of this instance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of this instance.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the children of this instance.
        /// </summary>
        public List<KeyValue> Children { get; private set; }

        /// <summary>
        /// Gets or sets the child <see cref="KeyValue" /> with the specified key.
        /// When retrieving by key, if no child with the given key exists, <see cref="Invalid" /> is returned.
        /// </summary>
        public KeyValue this[string key]
        {
            get
            {
                var child = Children
                    .FirstOrDefault(c => string.Equals(c.Name, key, StringComparison.OrdinalIgnoreCase));

                if (child == null)
                {
                    return Invalid;
                }

                return child;
            }
            set
            {
                var existingChild = Children
                    .FirstOrDefault(c => string.Equals(c.Name, key, StringComparison.OrdinalIgnoreCase));

                if (existingChild != null)
                {
                    // if the key already exists, remove the old one
                    Children.Remove(existingChild);
                }

                // ensure the given KV actually has the correct key assigned
                value.Name = key;

                Children.Add(value);
            }
        }

        /// <summary>
        /// Returns the value of this instance as a string.
        /// </summary>
        /// <returns>The value of this instance as a string.</returns>
        public string AsString()
        {
            return Value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as an unsigned byte.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an unsigned byte.</returns>
        public byte AsUnsignedByte(byte defaultValue = default(byte))
        {
            if (!byte.TryParse(Value, out byte value))
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as an unsigned short.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an unsigned short.</returns>
        public ushort AsUnsignedShort(ushort defaultValue = default(ushort))
        {
            if (!ushort.TryParse(Value, out ushort value))
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as an integer.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an integer.</returns>
        public int AsInteger(int defaultValue = default(int))
        {
            if (!int.TryParse(Value, out int value))
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as an unsigned integer.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an unsigned integer.</returns>
        public uint AsUnsignedInteger(uint defaultValue = default(uint))
        {
            if (!uint.TryParse(Value, out uint value))
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as a long.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as a long.</returns>
        public long AsLong(long defaultValue = default(long))
        {
            if (!long.TryParse(Value, out long value))
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as an unsigned long.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an unsigned long.</returns>
        public ulong AsUnsignedLong(ulong defaultValue = default(ulong))
        {
            if (!ulong.TryParse(Value, out ulong value))
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as a float.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as a float.</returns>
        public float AsFloat(float defaultValue = default(float))
        {
            if (!float.TryParse(Value, out float value))
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as a boolean.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as a boolean.</returns>
        public bool AsBoolean(bool defaultValue = default(bool))
        {
            if (!int.TryParse(Value, out int value))
            {
                return defaultValue;
            }

            return value != 0;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as an enum.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an enum.</returns>
        public T AsEnum<T>(T defaultValue = default(T))
            where T : struct
        {
            if (!Enum.TryParse<T>(Value, out var value))
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Name} = {Value}";
        }

        /// <summary>
        /// Attempts to load the given filename as a text <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>a <see cref="KeyValue"/> instance if the load was successful, or <c>null</c> on failure.</returns>
        /// <remarks>
        /// This method will swallow any exceptions that occur when reading, use <see cref="ReadAsText"/> if you wish to handle exceptions.
        /// </remarks>
        public static KeyValue LoadAsText(string path)
        {
            return LoadFromFile(path, false);
        }

        /// <summary>
        /// Attempts to load the given filename as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>a <see cref="KeyValue"/> instance if the load was successful, or <c>null</c> on failure.</returns>
        [Obsolete("Use TryReadAsBinary instead. Note that TryLoadAsBinary returns the root object, not a dummy parent node containg the root object.")]
        public static KeyValue LoadAsBinary(string path)
        {
            var kv = LoadFromFile(path, true);
            if (kv == null)
            {
                return null;
            }

            var parent = new KeyValue();
            parent.Children.Add(kv);
            return parent;
        }

        /// <summary>
        /// Attempts to load the given filename as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <param name="keyValue">The resulting <see cref="KeyValue"/> object if the load was successful, or <c>null</c> if unsuccessful.</param>
        /// <returns><c>true</c> if the load was successful, or <c>false</c> on failure.</returns>
        public static bool TryLoadAsBinary(string path, out KeyValue keyValue)
        {
            keyValue = LoadFromFile(path, true);
            return keyValue != null;
        }

        private static KeyValue LoadFromFile(string path, bool asBinary)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using (var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var kv = new KeyValue();

                    if (asBinary)
                    {
                        if (!kv.TryReadAsBinary(input))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (!kv.ReadAsText(input))
                        {
                            return null;
                        }
                    }

                    return kv;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to create an instance of <see cref="KeyValue"/> from the given input text.
        /// </summary>
        /// <param name="input">The input text to load.</param>
        /// <returns>a <see cref="KeyValue"/> instance if the load was successful, or <c>null</c> on failure.</returns>
        /// <remarks>
        /// This method will swallow any exceptions that occur when reading, use <see cref="ReadAsText"/> if you wish to handle exceptions.
        /// </remarks>
        public static KeyValue LoadFromString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var kv = new KeyValue();

                try
                {
                    if (!kv.ReadAsText(stream))
                    {
                        return null;
                    }

                    return kv;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Populate this instance from the given <see cref="Stream"/> as a text <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        /// <returns><c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        public bool ReadAsText(Stream input)
        {
            Children = new List<KeyValue>();

            new KVTextReader(this, input);

            return true;
        }

        /// <summary>
        /// Opens and reads the given filename as text.
        /// </summary>
        /// <seealso cref="ReadAsText"/>
        /// <param name="filename">The file to open and read.</param>
        /// <returns><c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        public bool ReadFileAsText(string filename, bool FirstTry = true)
        {
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return ReadAsText(fs);
                }
            }
            catch (IOException)
            {
                return ReadFileAsText(filename);
            }
            catch (Exception)
            {
                if (FirstTry)
                    return ReadFileAsText(filename);
                else
                    return false;
            }
        }

        internal void RecursiveLoadFromBuffer(KVTextReader kvr)
        {
            while (true)
            {
                // bool bAccepted = true;

                // get the key name
                string name = kvr.ReadToken(out bool wasQuoted, out bool wasConditional);

                if (string.IsNullOrEmpty(name))
                {
                    throw new Exception("RecursiveLoadFromBuffer: got EOF or empty keyname");
                }

                if (name.StartsWith("}") && !wasQuoted) // top level closed, stop reading
                {
                    break;
                }

                KeyValue dat = new KeyValue(name)
                {
                    Children = new List<KeyValue>()
                };
                Children.Add(dat);

                // get the value
                string value = kvr.ReadToken(out wasQuoted, out wasConditional);

                if (wasConditional && value != null)
                {
                    // bAccepted = ( value == "[$WIN32]" );
                    value = kvr.ReadToken(out wasQuoted, out wasConditional);
                }

                if (value == null)
                {
                    throw new Exception("RecursiveLoadFromBuffer:  got NULL key");
                }

                if (value.StartsWith("}") && !wasQuoted)
                {
                    throw new Exception("RecursiveLoadFromBuffer:  got } in key");
                }

                if (value.StartsWith("{") && !wasQuoted)
                {
                    dat.RecursiveLoadFromBuffer(kvr);
                }
                else
                {
                    if (wasConditional)
                    {
                        throw new Exception("RecursiveLoadFromBuffer:  got conditional between key and value");
                    }

                    dat.Value = value;
                    // blahconditionalsdontcare
                }
            }
        }

        /// <summary>
        /// Saves this instance to file.
        /// </summary>
        /// <param name="path">The file path to save to.</param>
        /// <param name="asBinary">If set to <c>true</c>, saves this instance as binary.</param>
        public void SaveToFile(string path, bool asBinary)
        {
            try
            {
                using (var f = File.Create(path))
                {
                    SaveToStream(f, asBinary);
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        /// <summary>
        /// Saves this instance to a given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save to.</param>
        /// <param name="asBinary">If set to <c>true</c>, saves this instance as binary.</param>
        public void SaveToStream(Stream stream, bool asBinary)
        {
            try
            {
                if (asBinary)
                {
                    RecursiveSaveBinaryToStream(stream);
                }
                else
                {
                    RecursiveSaveTextToFile(stream);
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        private void RecursiveSaveBinaryToStream(Stream f)
        {
            try
            {
                RecursiveSaveBinaryToStreamCore(f);
                f.WriteByte((byte)Type.End);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        private void RecursiveSaveBinaryToStreamCore(Stream f)
        {
            try
            {
                // Only supported types ATM:
                // 1. KeyValue with children (no value itself)
                // 2. String KeyValue
                if (Children.Any())
                {
                    f.WriteByte((byte)Type.None);
                    f.WriteNullTermString(Name, Encoding.UTF8);
                    foreach (var child in Children)
                    {
                        child.RecursiveSaveBinaryToStreamCore(f);
                    }
                    f.WriteByte((byte)Type.End);
                }
                else
                {
                    f.WriteByte((byte)Type.String);
                    f.WriteNullTermString(Name, Encoding.UTF8);
                    f.WriteNullTermString(Value ?? string.Empty, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        private void RecursiveSaveTextToFile(Stream stream, int indentLevel = 0)
        {
            try
            {
                // write header
                WriteIndents(stream, indentLevel);
                WriteString(stream, Name, true);
                WriteString(stream, "\n");
                WriteIndents(stream, indentLevel);
                WriteString(stream, "{\n");

                // loop through all our keys writing them to disk
                foreach (KeyValue child in Children)
                {
                    if (child.Value == null)
                    {
                        child.RecursiveSaveTextToFile(stream, indentLevel + 1);
                    }
                    else
                    {
                        WriteIndents(stream, indentLevel + 1);
                        WriteString(stream, child.Name, true);
                        WriteString(stream, "\t\t");
                        WriteString(stream, EscapeText(child.AsString()), true);
                        WriteString(stream, "\n");
                    }
                }

                WriteIndents(stream, indentLevel);
                WriteString(stream, "}\n");
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        private static string EscapeText(string value)
        {
            foreach (var kvp in KVTextReader.escapedMapping)
            {
                var textToReplace = new string(kvp.Value, 1);
                var escapedReplacement = @"\" + kvp.Key;
                value = value.Replace(textToReplace, escapedReplacement);
            }

            return value;
        }

        private void WriteIndents(Stream stream, int indentLevel)
        {
            WriteString(stream, new string('\t', indentLevel));
        }

        private static void WriteString(Stream stream, string str, bool quote = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes((quote ? "\"" : "") + str.Replace("\"", "\\\"") + (quote ? "\"" : ""));
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Populate this instance from the given <see cref="Stream"/> as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        /// <returns><c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        [Obsolete("Use TryReadAsBinary instead. Note that TryReadAsBinary returns the root object, not a dummy parent node containg the root object.")]
        public bool ReadAsBinary(Stream input)
        {
            var dummyChild = new KeyValue();
            Children.Add(dummyChild);
            return dummyChild.TryReadAsBinary(input);
        }

        /// <summary>
        /// Populate this instance from the given <see cref="Stream"/> as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        /// <returns><c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        public bool TryReadAsBinary(Stream input)
        {
            return TryReadAsBinaryCore(input, this, null);
        }

        private static bool TryReadAsBinaryCore(Stream input, KeyValue current, KeyValue parent)
        {
            current.Children = new List<KeyValue>();

            while (true)
            {
                var type = (Type)input.ReadByte();

                if (type == Type.End)
                {
                    break;
                }

                current.Name = input.ReadNullTermString(Encoding.UTF8);

                switch (type)
                {
                    case Type.None:
                        {
                            var child = new KeyValue();
                            var didReadChild = TryReadAsBinaryCore(input, child, current);
                            if (!didReadChild)
                            {
                                return false;
                            }
                            break;
                        }

                    case Type.String:
                        {
                            current.Value = input.ReadNullTermString(Encoding.UTF8);
                            break;
                        }

                    case Type.WideString:
                        {
                            System.Diagnostics.Debug.WriteLine("KeyValue", "Encountered WideString type when parsing binary KeyValue, which is unsupported. Returning false.");
                            return false;
                        }

                    case Type.Int32:
                    case Type.Color:
                    case Type.Pointer:
                        {
                            current.Value = Convert.ToString(input.ReadInt32());
                            break;
                        }

                    case Type.UInt64:
                        {
                            current.Value = Convert.ToString(input.ReadUInt64());
                            break;
                        }

                    case Type.Float32:
                        {
                            current.Value = Convert.ToString(input.ReadFloat());
                            break;
                        }

                    default:
                        {
                            return false;
                        }
                }

                if (parent != null)
                {
                    parent.Children.Add(current);
                }
                current = new KeyValue();
            }

            return true;
        }
    }

    internal static class StreamHelpers
    {
        private static readonly byte[] data = new byte[8];

        public static short ReadInt16(this Stream stream)
        {
            stream.Read(data, 0, 2);

            return BitConverter.ToInt16(data, 0);
        }

        public static ushort ReadUInt16(this Stream stream)
        {
            stream.Read(data, 0, 2);

            return BitConverter.ToUInt16(data, 0);
        }

        public static int ReadInt32(this Stream stream)
        {
            stream.Read(data, 0, 4);

            return BitConverter.ToInt32(data, 0);
        }

        public static uint ReadUInt32(this Stream stream)
        {
            stream.Read(data, 0, 4);

            return BitConverter.ToUInt32(data, 0);
        }

        public static ulong ReadUInt64(this Stream stream)
        {
            stream.Read(data, 0, 8);

            return BitConverter.ToUInt64(data, 0);
        }

        public static float ReadFloat(this Stream stream)
        {
            stream.Read(data, 0, 4);

            return BitConverter.ToSingle(data, 0);
        }

        public static string ReadNullTermString(this Stream stream, Encoding encoding)
        {
            int characterSize = encoding.GetByteCount("e");

            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    byte[] data = new byte[characterSize];
                    stream.Read(data, 0, characterSize);

                    if (encoding.GetString(data, 0, characterSize) == "\0")
                    {
                        break;
                    }

                    ms.Write(data, 0, data.Length);
                }

                return encoding.GetString(ms.ToArray());
            }
        }

        public static void WriteNullTermString(this Stream stream, string value, Encoding encoding)
        {
            var dataLength = encoding.GetByteCount(value);
            var data = new byte[dataLength + 1];
            encoding.GetBytes(value, 0, value.Length, data, 0);
            data[dataLength] = 0x00; // '\0'

            stream.Write(data, 0, data.Length);
        }

        private static readonly byte[] discardBuffer = new byte[2 << 12];

        public static void ReadAndDiscard(this Stream stream, int len)
        {
            while (len > discardBuffer.Length)
            {
                stream.Read(discardBuffer, 0, discardBuffer.Length);
                len -= discardBuffer.Length;
            }

            stream.Read(discardBuffer, 0, len);
        }
    }
}