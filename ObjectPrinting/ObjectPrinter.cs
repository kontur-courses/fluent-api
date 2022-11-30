namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        /// <summary>
        /// Создаёт сериализатор для выбранного типа
        /// </summary>
        /// <typeparam name="T">Тип, для которого задаётся правило сериализации</typeparam>
        /// <returns>Объект сериализатора с настройками по умолчанию</returns>
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}