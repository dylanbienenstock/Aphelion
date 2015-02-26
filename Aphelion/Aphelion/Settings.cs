using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Aphelion
{
    public static class Settings
    {
        private const string timeFormat = "MM/dd/yyyy h:mm tt";

        private static Dictionary<string, string> strings = new Dictionary<string, string>();
        private static Dictionary<string, double> numbers = new Dictionary<string, double>();                // type, discovery, value
        private static Dictionary<string, Tuple<string, string, object>> corrupt = new Dictionary<string, Tuple<string, string, object>>();
        private static bool corruptOnNotNull = false; // This is necessary because DateTime cannot be
        private static DateTime corruptOn;            // trusted to manage itself properly when it's null
        private static string file;

        public static void SetFile(string file, bool createIfNonexistant)
        {
            if (File.Exists(file))
            {
                Settings.file = file;
            }
            else if (createIfNonexistant)
            {
                File.WriteAllText(file, string.Empty);
            }
            else
            {
                throw new Exception("File does not exist");
            }
        }


        public static string GetValueAsString(string setting, string defaultValue)
        {
            if (strings.ContainsKey(setting))
            {
                return strings[setting];
            }
            else if (numbers.ContainsKey(setting))
            {
                return numbers[setting].ToString();
            }

            return defaultValue;
        }

        public static double? GetValueAsNumber(string setting, double? defaultValue)
        {
            if (numbers.ContainsKey(setting))
            {
                return numbers[setting];
            }

            return defaultValue;
        }

        private static void SetValueRaw<T>(string setting, T value)
        {
            if (typeof(T) == typeof(string))
            {
                numbers.Remove(setting);
                strings[setting] = value.ToString();
            }
            else if (typeof(T) == typeof(double) || typeof(T) == typeof(float) || typeof(T) == typeof(int))
            {
                strings.Remove(setting);
                numbers[setting] = double.Parse(value.ToString());
            }
        }

        public static void SetValue<T>(string setting, T value)
        {
            if (!string.IsNullOrWhiteSpace(file))
            {
                bool updateRequired = false;

                if (typeof(T) == typeof(string))
                {
                    if (GetValueAsString(setting, null) != value.ToString())
                    {
                        updateRequired = true;
                    }
                }
                else if (typeof(T) == typeof(double) || typeof(T) == typeof(float) || typeof(T) == typeof(int))
                {
                    if (GetValueAsNumber(setting, null) != double.Parse(value.ToString()))
                    {
                        updateRequired = true;
                    }
                }

                if (updateRequired)
                {
                    SetValueRaw<T>(setting, value);
                    WriteToFile();
                }
            }
            else
            {
                throw new Exception("No settings file set");
            }
        }

        public static void SetDefaultValue<T>(string setting, T value)
        {
            if (!string.IsNullOrWhiteSpace(file))
            {
                if (!Exists(setting))
                {
                    SetValueRaw<T>(setting, value);
                    WriteToFile();
                }
            }
            else
            {
                throw new Exception("No settings file set");
            }
        }

        public static bool Exists(string setting)
        {
            return (strings.ContainsKey(setting) || numbers.ContainsKey(setting));
        }

        public static Type GetType(string setting)
        {
            if (strings.ContainsKey(setting))
            {
                return typeof(string);
            }
            else if (numbers.ContainsKey(setting))
            {
                return typeof(double);
            }

            return null;
        }

        public static void Remove(string setting)
        {
            if (strings.ContainsKey(setting))
            {
                strings.Remove(setting);
            }
            else if (numbers.ContainsKey(setting))
            {
                numbers.Remove(setting);
            }
        }

        public static void ClearCorruptSettings()
        {
            corrupt.Clear();
            WriteToFile();
        }

        public static bool ReadFromFile()
        {
            bool notCorrupt = true;
            bool isSettingsFile = false;

            if (!string.IsNullOrWhiteSpace(file))
            {
                if (File.ReadAllBytes(file).Length > 0)
                {
                    try
                    {
                        using (XmlReader xml = XmlReader.Create(file))
                        {
                            while (xml.Read())
                            {
                                if (xml.IsStartElement())
                                {
                                    if (xml.Name == "settings")
                                    {
                                        isSettingsFile = true;

                                        if (xml.GetAttribute("corruptOn") != null)
                                        {
                                            corruptOnNotNull = true;
                                            corruptOn = DateTime.ParseExact(xml.GetAttribute("corruptOn"), timeFormat, null);
                                        }

                                        strings.Clear();
                                        numbers.Clear();
                                    }
                                    else if (isSettingsFile)
                                    {
                                        try
                                        {
                                            if (xml.Name == "string")
                                            {
                                                SetValueRaw<string>(xml.GetAttribute("key"), xml.GetAttribute("value"));
                                            }
                                            else if (xml.Name == "number")
                                            {
                                                SetValueRaw<double>(xml.GetAttribute("key"), double.Parse(xml.GetAttribute("value")));
                                            }
                                            else if (xml.Name == "corrupt")
                                            {
                                                corrupt[xml.GetAttribute("key")] = Tuple.Create(xml.GetAttribute("type"), xml.GetAttribute("discovery"), (object)xml.GetAttribute("value"));
                                            }
                                        }
                                        catch
                                        {
                                            corrupt[xml.GetAttribute("key")] = Tuple.Create(xml.Name, DateTime.Now.ToString(timeFormat), (object)xml.GetAttribute("value"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        corruptOnNotNull = true;
                        corruptOn = DateTime.Now;
                        notCorrupt = false;
                    }
                }
                else
                {
                    corruptOnNotNull = true;
                    corruptOn = DateTime.Now;
                    notCorrupt = false;
                }
            }

            if (!notCorrupt)
            {
                WriteBlankFile();
                ReadFromFile(); // Why do I have to do this again?
            }

            return notCorrupt;
        }

        private static void WriteBlankFile()
        {
            File.WriteAllText(file, string.Empty);

            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";

            using (XmlWriter xml = XmlWriter.Create(file, xmlSettings))
            {
                xml.WriteStartDocument();
                xml.WriteComment(" Time format: " + timeFormat + ' ');
                xml.WriteStartElement("settings");

                if (corruptOnNotNull)
                {
                    xml.WriteAttributeString("corruptOn", corruptOn.ToString(timeFormat));
                }
                
                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
        }

        public static void WriteToFile()
        {
            File.WriteAllText(file, string.Empty);

            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";

            using (XmlWriter xml = XmlWriter.Create(file, xmlSettings))
            {
                xml.WriteStartDocument();
                xml.WriteComment(" Time format: " + timeFormat + ' ');
                xml.WriteStartElement("settings");

                if (corruptOnNotNull)
                {
                    xml.WriteAttributeString("corruptOn", corruptOn.ToString(timeFormat));
                }

                foreach (KeyValuePair<string, string> setting in strings)
                {
                    xml.WriteStartElement("string");
                    xml.WriteAttributeString("key", setting.Key);
                    xml.WriteAttributeString("value", setting.Value);
                    xml.WriteEndElement();
                }

                foreach (KeyValuePair<string, double> setting in numbers)
                {
                    xml.WriteStartElement("number");
                    xml.WriteAttributeString("key", setting.Key);
                    xml.WriteAttributeString("value", setting.Value.ToString());
                    xml.WriteEndElement();
                }

                foreach (KeyValuePair<string, Tuple<string, string, object>> setting in corrupt)
                {
                    xml.WriteStartElement("corrupt");
                    xml.WriteAttributeString("type", setting.Value.Item1);
                    xml.WriteAttributeString("discovery", setting.Value.Item2);
                    xml.WriteAttributeString("key", setting.Key);
                    xml.WriteAttributeString("value", setting.Value.Item3.ToString());
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
        }
    }
}