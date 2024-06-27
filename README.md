# hh-analyzer

**hh-analyzer** - это C# BackgroundService, который собирает и анализирует данные о ключевых навыках по заданным профессиям, получаемым от спроектированного нами api [TakeJobOffer](https://github.com/KovalNikitok/TakeJobOffer), с использованием стороннего API [HeadHunter](https://api.hh.ru/openapi/redoc). После анализа данные отправляются в [наш собственный сервис](https://github.com/KovalNikitok/TakeJobOffer).

## Требования
- .NET 6.0 или выше
- Docker
- Зарегистрируйте приложение на [HeadHunter](https://dev.hh.ru/admin)
- Разверните свой сервис '[TakeJobOffer](https://github.com/KovalNikitok/TakeJobOffer)'

## Установка
1. Клонируйте репозиторий:
    ```sh
    git clone https://github.com/KovalNikitok/hh-analyzer.git
    ```
2. Перейдите в папку проекта:
    ```sh
    cd hh-analyzer
    ```
3. Восстановите зависимости:
    ```sh
    dotnet restore
    ```
4. Пропишите в вайле конфигурации `appsettings.json` ваши token и UserAgent, полученные из личного кабинета вашего приложения на сайте [HeadHunter](https://dev.hh.ru/admin)

## Конфигурация
1. Настройте параметры приложения в `appsettings.json`:
    ```json
    {
      "HHApiSettings": {
        "ConnectionString": "https://api.hh.ru",
        "AccessToken": "your_api_token",
        "Agent": "your_user_agent"
      },
      "TakeJobOfferApiSettings": {
        "ConnectionString": "https://your_takejoboffer_api_domain/api"
      }
    }
    ```
    
## Использование
1. Запустите приложение:
    ```sh
    dotnet run
    ```
2. Приложение начнет собирать данные, анализировать их и отправлять результаты в ваш сервис.
3. Цикл работы приложения составляет 1 день, промежутки между отправкой каждого навыка в базу данных [вашего сервиса](https://github.com/KovalNikitok/TakeJobOffer) составляют 100 миллисекунд, а для каждой профессии - 3 секунды для распределния нагрузки

## Схема работы
1. Приложение обращается к [api.hh.ru](https://api.hh.ru/openapi/redoc) и получает данные по заданным профессиям.
2. Данные парсятся и анализируются для выявления ключевых навыков по профессиям.
3. Составляется словарь с количеством упоминаний для конкретных профессий.
4. Обработанные данные отправляются в ваш сервис через [API takejoboffer](https://github.com/KovalNikitok/TakeJobOffer).

## Авторы
- [Коваль Никита](https://github.com/KovalNikitok)

## Лицензия
Этот проект лицензируется на условиях лицензии MIT. Подробнее см. файл [LICENSE](https://github.com/KovalNikitok/hh-analyzer/blob/master/LICENSE.txt).