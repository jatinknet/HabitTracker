# HabitFlow — Habit Tracker

A modern, interactive habit tracker built with **Blazor Server**, **Entity Framework Core**, and **SQL Server**, inspired by the monthly habit-tracker spreadsheet.

## Features

- **Interactive monthly grid** — click any day cell to mark a habit complete/incomplete (mirrors the original spreadsheet's week color-bands: Week 1 orange, Week 2 blue, Week 3 red, Week 4 green, Week 5 purple).
- **Add / Edit / Delete habits** via a modal — custom name, icon (Bootstrap Icons), color, and monthly goal.
- **Inline goal editing** — change a habit's monthly goal directly in the grid.
- **Live progress bars** per habit (completed / goal).
- **Streak tracking** — current streak and longest streak per habit, computed server-side.
- **Weekly summary donut charts** (Chart.js) — completion % per week, plus a total monthly progress donut.
- **Dashboard page** — best habit, habit that needs work, longest streak, active habit count, a horizontal bar chart comparing all habits, and a 6-month completion trend line chart.
- **Month navigation** — browse any past/future month; history is preserved per day.
- Fully responsive **Bootstrap 5** UI with a custom modern theme (dark gradient sidebar, cards, rounded checkboxes).

## Tech Stack

| Layer | Technology |
|---|---|
| UI | Blazor Server (.NET 8, Interactive Server render mode) |
| Styling | Bootstrap 5 + Bootstrap Icons + custom CSS |
| Charts | Chart.js (via JS interop) |
| Data access | Entity Framework Core 8 |
| Database | Microsoft SQL Server |

## Project Structure

```
HabitTracker/
├── Components/
│   ├── App.razor              # HTML root, CDN links (Bootstrap, Chart.js)
│   ├── Routes.razor           # Router
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor      # Sidebar navigation
│   └── Pages/
│       ├── Home.razor         # Main tracker grid (route: /)
│       ├── Dashboard.razor    # Analytics dashboard (route: /dashboard)
│       ├── HabitFormModal.razor
│       └── Error.razor
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── Habit.cs
│   ├── HabitEntry.cs
│   └── StatsModels.cs         # DTOs for weekly/monthly/dashboard stats
├── Services/
│   ├── IHabitService.cs
│   └── HabitService.cs        # All business logic: streaks, week grouping, stats
├── wwwroot/
│   ├── css/app.css
│   └── js/charts.js           # Chart.js render helpers
├── Program.cs
├── appsettings.json
└── HabitTracker.csproj
```

## Setup Instructions

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, SQL Server Express, or a full instance)
- (Optional) Visual Studio 2022 17.8+ or VS Code with the C# Dev Kit

### 2. Configure the connection string

Edit `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=HabitTrackerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Adjust `Server=` for your instance (e.g. `(localdb)\\mssqllocaldb`, a full server name, or use SQL auth with `User Id=...;Password=...`).

### 3. Install EF Core tools (if you don't have them)

```bash
dotnet tool install --global dotnet-ef
```

### 4. Restore packages

```bash
cd HabitTracker
dotnet restore
```

### 5. Create and apply the migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates the `Habits` and `HabitEntries` tables and seeds two starter habits ("Drink 16 fl oz of Water" and "Play Tennis"), matching the reference spreadsheet.

> Note: `Program.cs` also calls `db.Database.Migrate()` automatically on startup, so as long as a migration exists, the database will be created/updated the first time you run the app.

### 6. Run the app

```bash
dotnet run
```

Then open the URL shown in the console (typically `https://localhost:5001` or similar).

## Extending it further

- **Authentication** — add ASP.NET Core Identity if you want multi-user support (each `Habit` would need a `UserId` foreign key).
- **Notifications/reminders** — hook into a background service (`IHostedService`) to send daily reminder emails/push notifications for incomplete habits.
- **Export** — add a CSV/Excel export button on the Dashboard using the same query logic in `HabitService.GetMonthlyDataAsync`.
- **Dark mode** — the CSS uses CSS custom properties (`:root` variables in `app.css`), so a dark theme toggle can be added by swapping variable values with a `data-theme` attribute.
