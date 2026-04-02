```markdown
# AutoChecker


## Запуск

```bash
dotnet run
```

## Налаштування

Створіть файл `appsettings.json` на основі `appsettings.example.json`:

```json
{
  "Services": {
    "Geocoding": "Mapbox",
    "RouteChecker": "Mapbox"
  },
  "Mapbox": {
    "AccessToken": "pk.your_mapbox_access_token_here"
  },
  "Logging": {
    "Enabled": false,
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConsoleMode": true
}
```

### Параметри

| Параметр | Опис | Можливі значення |
|----------|------|------------------|
| `Services:Geocoding` | Сервіс геокодування | `Mapbox`, `Nominatim` |
| `Services:RouteChecker` | Сервіс перевірки маршрутів | `Mapbox`, `Osrm` |
| `Logging:Enabled` | Увімкнення логування | `true`, `false` |
| `ConsoleMode` | Виведення інформації в консоль / варіант з контроллером | `true`, `false` |

## Ліміти сервісів

### Nominatim (OpenStreetMap)
- **URL**: https://nominatim.openstreetmap.org
- **Ліміт**: 1 запит/сек
- **Безкоштовно**: так
- **Потрібен ключ**: ні

### OSRM (Open Source Routing Machine)
- **URL**: https://project-osrm.org
- **Ліміт**: без обмежень (публічний API)
- **Безкоштовно**: так
- **Потрібен ключ**: ні

### Mapbox
- **URL**: https://docs.mapbox.com/api/
- **Ліміт**: 100 000 запитів/місяць
- **Безкоштовно**: так
- **Потрібен ключ**: так (реєстрація на mapbox.com)

