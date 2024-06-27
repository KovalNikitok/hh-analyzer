# hh-analyzer

**hh-analyzer** - ��� C# BackgroundService, ������� �������� � ����������� ������ � �������� ������� �� �������� ����������, ���������� �� ����������������� ���� api [TakeJobOffer](https://github.com/KovalNikitok/TakeJobOffer), � �������������� ���������� API [HeadHunter](https://api.hh.ru/openapi/redoc). ����� ������� ������ ������������ � [��� ����������� ������](https://github.com/KovalNikitok/TakeJobOffer).

## ����������
- .NET 6.0 ��� ����
- Docker
- ��������������� ���������� �� [HeadHunter](https://dev.hh.ru/admin)
- ���������� ���� ������ '[TakeJobOffer](https://github.com/KovalNikitok/TakeJobOffer)'

## ���������
1. ���������� �����������:
    ```sh
    git clone https://github.com/KovalNikitok/hh-analyzer.git
    ```
2. ��������� � ����� �������:
    ```sh
    cd hh-analyzer
    ```
3. ������������ �����������:
    ```sh
    dotnet restore
    ```
4. ��������� � ����� ������������ `appsettings.json` ���� token � UserAgent, ���������� �� ������� �������� ������ ���������� �� ����� [HeadHunter](https://dev.hh.ru/admin)

## ������������
1. ��������� ��������� ���������� � `appsettings.json`:
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
    
## �������������
1. ��������� ����������:
    ```sh
    dotnet run
    ```
2. ���������� ������ �������� ������, ������������� �� � ���������� ���������� � ��� ������.
3. ���� ������ ���������� ���������� 1 ����, ���������� ����� ��������� ������� ������ � ���� ������ [������ �������](https://github.com/KovalNikitok/TakeJobOffer) ���������� 100 �����������, � ��� ������ ��������� - 3 ������� ��� ������������ ��������

## ����� ������
1. ���������� ���������� � [api.hh.ru](https://api.hh.ru/openapi/redoc) � �������� ������ �� �������� ����������.
2. ������ �������� � ������������� ��� ��������� �������� ������� �� ����������.
3. ������������ ������� � ����������� ���������� ��� ���������� ���������.
4. ������������ ������ ������������ � ��� ������ ����� [API takejoboffer](https://github.com/KovalNikitok/TakeJobOffer).

## ������
- [������ ������](https://github.com/KovalNikitok)

## ��������
���� ������ ������������� �� �������� �������� MIT. ��������� ��. ���� [LICENSE](https://github.com/KovalNikitok/hh-analyzer/blob/master/LICENSE.txt).