namespace hh_analyzer.Application.Abstractions
{
    /// <summary>
    /// Интерфейс для реализации сервиса для работы с HH API
    /// </summary>
    public interface IHHApiSerice
    {
        /// <summary>
        /// Реализация паттерна 'Фасад' для получения навыков профессии с количеством их упоминаний по конкертной профессии
        /// </summary>
        /// <param name="name">Название профессии</param>
        /// <param name="description">Описание профессии</param>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns>Словарь с названиями навыков и количеством их упоминаний</returns>
        /// <exception cref="ArgumentNullException">'name' is null</exception>
        /// <exception cref="NullReferenceException">Не найдено профессии/вакансий/скиллов</exception>
        Task<Dictionary<string, int>?> GetSkillsWithMentionCountFacade(
            string name, string? description, CancellationToken cancellationToken);

        /// <summary>
        /// Метод для освобождения ресурсов
        /// </summary>
        void Dispose();
    }
}