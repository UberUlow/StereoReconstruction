using System;
using NLog;

namespace StereoReconstruction.Common.Logging
{
    /// <summary>
    /// Класс логирования
    /// </summary>
    public static class Tracer
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Логирование любой информации
        /// </summary>
        /// <param name="message">Сообщение</param>
        public static void Info(string message)
        {
            logger.Info(message);
        }

        /// <summary>
        /// Логирование ошибок
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Ошибка</param>
        public static void Error(string message, Exception ex)
        {
            logger.ErrorException(message, ex);
        }
    }
}
