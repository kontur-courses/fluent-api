namespace ObjectPrinting
{
    /// <summary>Создаёт конфигурации для печати объектов</summary>
    public class ObjectPrinter
    {
        /// <summary>Создаёт конфигурацию печати для указанного типа</summary>
        /// <typeparam name="T">Тип печатаемого объекта</typeparam>
        /// <returns>Новая конфигурация с настройками по умолчанию</returns>
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}