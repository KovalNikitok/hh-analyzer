using hh_analyzer.Contracts;

namespace hh_analyzer.Application.Abstractions
{
    /// <summary>
    /// Интерфейс для реализации сервиса для работы с TakeJobOffer API
    /// </summary>
    public interface ITakeJobOfferApiService
    {
        /// <summary>
        /// Метод для получения профессий
        /// </summary>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns>Список профессий</returns>
        Task<List<ProfessionRequest?>?> GetProfessionsAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Метод для получения навыков профессии с названием навыка и количеством упоминаний
        /// </summary>
        /// <param name="profession">Профессия</param>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns>Список навыков профессии</returns>
        Task<List<ProfessionSkillWithNameRequest?>?> GetProfessionSkillWithName(ProfessionRequest profession, CancellationToken cancellationToken);
        /// <summary>
        /// Метод для получения всех навыков
        /// </summary>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns>Список навыков профессии</returns>
        Task<List<SkillRequest?>?> GetSkillsAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Метод для получения навыка по его названию
        /// </summary>
        /// <param name="name">Название навыка</param>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns>Навык</returns>
        Task<SkillRequest?> GetSkillByNameAsync(string name, CancellationToken cancellationToken);
        /// <summary>
        /// Метод для отправки навыка по профессии
        /// </summary>
        /// <param name="profession">Профессия</param>
        /// <param name="ps">Навык по профессии и количество его упоминаний по вакансиям</param>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns></returns>
        Task SendNewProfessionSkillAsync(ProfessionRequest profession, ProfessionSkillResponse ps, CancellationToken cancellationToken);
        /// <summary>
        /// Метод для обновления навыка по профессии с количеством его уопминаний по вакансиям
        /// </summary>
        /// <param name="profession">Профессия</param>
        /// <param name="ps">Навык по профессии и количество его упоминаний по вакансиям</param>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns></returns>
        Task SendUpdatedProfessionSkillAsync(ProfessionRequest profession, ProfessionSkillResponse ps, CancellationToken cancellationToken);
        /// <summary>
        /// Метод для создания нового навыка
        /// </summary>
        /// <param name="skill">Навык с его названием</param>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns>Guid созданного навыка</returns>
        Task<Guid> SendNewSkillAsync(SkillResponse skill, CancellationToken cancellationToken);
        /// <summary>
        /// Метод для удаления навыка по профессии
        /// </summary>
        /// <param name="profession">Профессия</param>
        /// <param name="ps">Навык по профессии с названием и количестов его упоминаний по вакансиям</param>
        /// <param name="cancellationToken">Токен для прерывания запроса</param>
        /// <returns></returns>
        Task RemoveProfessionSkillAsync(ProfessionRequest profession,ProfessionSkillRequest ps,CancellationToken cancellationToken);
        /// <summary>
        /// Метод для освобождения ресурсов
        /// </summary>
        void Dispose();
    }
}