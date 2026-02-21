# 🤖 RoboClean Cloud

## Облачный сервис для управления роботами-пылесосами

[](https://dotnet.microsoft.com/)
[](https://www.postgresql.org/)
[](https://www.docker.com/)
[](https://xunit.net/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 📋 Содержание

- [О проекте](#о-проекте)
- [Архитектура](#архитектура)
- [Технологический стек](#технологический-стек)
- [Структура проекта](#структура-проекта)
- [Установка и запуск](#установка-и-запуск)
- [API Endpoints](#api-endpoints)
- [Тестирование](#тестирование)
- [Docker](#docker)
- [Разработка](#разработка)

## 🎯 О проекте

**RoboClean Cloud (проект)** — это облачное решение для управления роботами-пылесосами. Проект реализован с использованием **Clean Architecture**, **Domain-Driven Design** и **CQRS**.

### Основные возможности

- ✅ Управление роботами (регистрация, мониторинг)
- ✅ Запуск и контроль уборки
- ✅ Гибкое расписание с CRON
- ✅ История и аналитика уборок
- ✅ Многопользовательский режим
- ✅ Health Checks и метрики

## 🏗 Архитектура

Проект разделен на 4 основных слоя:


<img src="file:///G:/GB/Testing/lesson10/Robo.png" title="" alt="" width="425">

## 🛠 Технологический стек

| Компонент   | Технология            | Версия |
| ----------- | --------------------- | ------ |
| Платформа   | .NET                  | 10.0   |
| API         | ASP.NET Core          | 10.0   |
| ORM         | Entity Framework Core | 10.0   |
| CQRS        | MediatR               | 12.4   |
| БД          | PostgreSQL            | 15     |
| Кэш         | Redis                 | 7      |
| Логирование | Serilog               | 4.3    |
| Тесты       | xUnit, Moq            | -      |
| Контейнеры  | Docker                | -      |

## 📁 Структура проекта

RoboCleanCloud/
├── src/
│ ├── RoboCleanCloud.Domain/ # Сущности, Value Objects, Enums
│ ├── RoboCleanCloud.Application/ # Use Cases, DTOs, Interfaces
│ ├── RoboCleanCloud.Infrastructure/ # Репозитории, EF Core, Сервисы
│ └── RoboCleanCloud.Api/ # Controllers, BFF, Middleware
├── tests/
│ ├── RoboCleanCloud.UnitTests/ # Модульные тесты
│ └── RoboCleanCloud.IntegrationTests/ # Интеграционные тесты
├── infrastructure/
│ └── docker/
│ ├── Dockerfile
│ └── docker-compose.yml
├── RoboCleanCloud.sln
└── README.md
text

## 🚀 Установка и запуск

### Предварительные требования

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [PostgreSQL 15](https://www.postgresql.org/download/) (или через Docker)

### Быстрый старт

# 1. Клонирование репозитория

git clone [https://github.com/AnatoliiMer/RoboCleanCloud.git](https://github.com/AnatoliiMer/RoboCleanCloud.git)
cd RoboCleanCloud

# 2. Запуск PostgreSQL и Redis через Docker

docker-compose up -d postgres redis

# 3. Применение миграций

dotnet ef database update --project RoboCleanCloud.Infrastructure --startup-project RoboCleanCloud.Api

# 4. Запуск API

dotnet run --project RoboCleanCloud.Api

API будет доступно: http://localhost:5180
Swagger UI: http://localhost:5180/swagger
🌐 API Endpoints
Robots API (/api/v1/robots)
Метод    Endpoint    Описание
POST    /    Регистрация робота
GET    /    Список роботов
GET    /{id}    Информация о роботе
PUT    /{id}    Обновление робота
DELETE    /{id}    Удаление робота
Cleaning API (/api/v1/cleaning)
Метод    Endpoint    Описание
POST    /start    Запуск уборки
POST    /{robotId}/stop    Остановка
GET    /sessions    История уборок
🧪 Тестирование
bash

# Модульные тесты

dotnet test RoboCleanCloud.UnitTests

# Интеграционные тесты

dotnet test RoboCleanCloud.IntegrationTests

# Все тесты

dotnet test

🐳 Docker
Сборка образа
bash

docker build -t roboclean-api -f RoboCleanCloud.Api/Dockerfile .

Запуск всех сервисов
bash

docker-compose up -d

📊 Мониторинг

    Health Checks: http://localhost:5180/health
    
    Метрики: http://localhost:5180/metrics
    
    Grafana: http://localhost:3000
    
    Prometheus: http://localhost:9090

MIT License. Подробнее в файле LICENSE.

Made with ❤️ for robot cleaning enthusiasts
