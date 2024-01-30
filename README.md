# JsonMart

JsonMart - это веб-приложение для управления заказами, продуктами и пользователями, построенное на ASP.NET Core. Проект предоставляет REST API для создания, обновления, удаления и получения данных о продуктах, заказах и пользователях.

## Тестирование API

Для тестирования и изучения API проекта можно воспользоваться Swagger по следующему URL:

[Swagger для тестирования API](http://vitvik-001-site1.htempurl.com/swagger/index.html)


## Основные Функции

- **Управление Заказами**: Позволяет создавать, обновлять, удалять и получать заказы. Также поддерживает оплату заказов.
- **Управление Продуктами**: Включает в себя функционал для работы с продуктами - их создание, обновление, удаление и получение списка доступных продуктов.
- **Управление Пользователями**: Предоставляет возможность создания новых пользователей, получения информации о пользователях и управления их балансом.

## Технологии

- ASP.NET Core
- Entity Framework Core
- REST API
- MySQL

## Контроллеры

- `OrdersController`: Основной контроллер для управления заказами.
- `ProductsController`: Контроллер для работы с продуктами.
- `UsersController`: Контроллер для управления пользователями и их балансом.
- `DebugController`: Дополнительный контроллер для тестирования функционала.

## Модели Данных

- `Product`: Модель для продуктов, содержит информацию о продукте, включая цену и количество.
- `Order`: Модель заказа, связывает продукты и пользователя, который сделал заказ.
- `User`: Модель пользователя, содержит информацию о пользователе, включая его заказы и баланс.

- ## Схема базы данных

  <img width="631" alt="Схема" src="https://github.com/Vitaly086/JsonMart/assets/93872632/50e0440f-32b7-40a6-91ec-ad99df12803e">

На схеме представлена структура базы данных, состоящая из четырёх основных сущностей:

- `Users` - хранит данные пользователей, включая их уникальные идентификаторы, имена и баланс.
- `Products` - содержит информацию о продуктах, такую как их идентификаторы, названия, цены, описания и количество доступного на складе.
- `Orders` - описывает заказы, ссылаясь на идентификаторы пользователей и включая данные о дате заказа и его статусе.
- `OrderProducts` - промежуточная таблица, которая указывая количество каждого продукта в заказе.



