public static class Queries
{
    public const string GetApplications = @"
        SELECT 
            ЗАЯВКА.ID_Заявки,
            ГРАЖДАНИН.Фамилия + ' ' + ГРАЖДАНИН.Имя AS Гражданин,
            СТАТУС.Название AS Статус,
            МЕРОПРИЯТИЕ.Название AS Мероприятие,
            ЗАЯВКА.Дата_Создания
        FROM dbo.ЗАЯВКА
        INNER JOIN dbo.ГРАЖДАНИН ON ЗАЯВКА.ID_Гражданина = ГРАЖДАНИН.ID_Гражданина
        INNER JOIN dbo.СТАТУС ON ЗАЯВКА.ID_Статуса = СТАТУС.ID_Статуса
        INNER JOIN dbo.МЕРОПРИЯТИЕ ON ЗАЯВКА.ID_Мероприятия = МЕРОПРИЯТИЕ.ID_Мероприятия;
    ";

    public const string GetEvents = @"
        SELECT 
            М.ID_Мероприятия,
            ПОЛ.Имя + ' ' + ПОЛ.Фамилия AS Пользователь,
            ТИП.Название AS Тип_Мероприятия, -- замените 'Метод' на фактическое название столбца с именем типа
            М.Название,
            М.Описание,
            М.Дата_Мероприятия,
            М.Бюджет
        FROM dbo.МЕРОПРИЯТИЕ М
        INNER JOIN dbo.ПОЛЬЗОВАТЕЛЬ ПОЛ ON М.ID_Пользователя = ПОЛ.ID_Пользователя
        INNER JOIN dbo.ТИП_МЕРОПРИЯТИЯ ТИП ON М.ID_Типа = ТИП.ID_Типа;
    ";

    public const string GetCitizen = @"
        SELECT * FROM [dbo].[ГРАЖДАНИН];
    ";

    public const string GetUsers = @"
        SELECT
            ПОЛ.ID_Пользователя,
            ПОЛ.Имя,
            ПОЛ.Фамилия,
            ПОЛ.Отчество,
            ПОЛ.Логин,
            РОЛЬ.Название AS Роль_Пользователя
        FROM dbo.ПОЛЬЗОВАТЕЛЬ ПОЛ
        INNER JOIN dbo.РОЛЬ РОЛЬ ON ПОЛ.ID_Роли = РОЛЬ.ID_Роли;
    ";

    public const string GetStatus = @"
        SELECT * FROM [dbo].[СТАТУС];
    ";

    public const string GetTypeEvent = @"
        SELECT * FROM [dbo].[ТИП_МЕРОПРИЯТИЯ];
    ";

    public const string GetRoleUsers = @"
        
    ";
}
