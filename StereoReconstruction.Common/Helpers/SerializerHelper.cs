using System.IO;
using System.Xml.Serialization;

namespace StereoReconstruction.Common.Helpers
{
    /// <summary>
    /// Класс сериализации и десериализации данных
    /// </summary>
    public static class SerializerHelper
    {
        /// <summary>
        /// Сериализация данных
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="data">Данные</param>
        /// <param name="path">Путь к xml файлу</param>
        public static void SerializeToXml<T>(T data, string path)
        {

            XmlSerializer srzr = new XmlSerializer(typeof(T));
            using (StreamWriter sw = new StreamWriter(path))
            {
                srzr.Serialize(sw, data);
            }
        }

        /// <summary>
        /// Десериализация данных
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="path">Путь к xml файлу</param>
        /// <returns>Данные</returns>
        public static T DeserializeFromXml<T>(string path)
        {
            XmlSerializer srzr = new XmlSerializer(typeof(T));
            using (StreamReader sr = new StreamReader(path))
            {
                return (T)srzr.Deserialize(sr);
            }
        }
    }
}
