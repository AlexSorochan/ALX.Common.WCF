namespace ALX.Common.WCF
{
    /// <summary>
    /// Заголовок исходящего сообщения
    /// </summary>
    public class SimpleOutMessageHeader
    {
        /// <summary>
        /// Пространство имен заголовка
        /// </summary>
        public string NameSpace { get; }

        /// <summary>
        /// Имя заголовка
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Значение заголовка
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Простой заголовок исходящего сообщения
        /// </summary>
        /// <param name="nameSpace">Пространство имен заголовка</param>
        /// <param name="name">Имя заголовка</param>
        /// <param name="value">Значение заголовка</param>
        public SimpleOutMessageHeader(string nameSpace, string name, object value)
        {
            NameSpace = nameSpace;
            Name = name;
            Value = value;
        }
    }
}
