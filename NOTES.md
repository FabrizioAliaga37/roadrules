# NOTES

2026-11-03 - .NET SDK 8.0.424, dotnet-ef 8.0.31, Linux (WSL), command line.

## Evening 1 - setup, model, migration, Jurisdictions page

**EF Core package versions**
- Saw: `dotnet add package Microsoft.EntityFrameworkCore.Sqlite` exactly as in the guide fails with: `NU1202: Package Microsoft.EntityFrameworkCore.Sqlite 10.0.14 is not compatible with net8.0`.
- Why: with no version, NuGet picks the latest, EF Core 10, and that only targets net10.0. The guide says "works on 8, 9 and 10", but only if the package versions match the SDK.
- Fix: `--version 8.0.*` on both packages, then pinned them to 8.0.33 in the csproj. SqlServer gets the same treatment on evening 3.

**dotnet ef version warning**
- Saw: "The Entity Framework tools version '8.0.31' is older than that of the runtime '8.0.33'" on `migrations add`.
- Why: the global dotnet-ef tool was installed before the 8.0.33 packages came out.
- Fix: harmless, the migration still worked. `dotnet tool update --global dotnet-ef --version 8.0.33` removes it.

**EffectiveFrom stored as TEXT**
- Saw: in roadrules.db `EffectiveFrom` is TEXT ('2026-01-01'), and the migration has `type: "TEXT"` plus `Sqlite:Autoincrement` on the Ids.
- Why: SQLite has no date type, so EF Core stores DateOnly as an ISO string.
- Matters on evening 3: SQL Server needs a real `date` column, so the migration has to be regenerated, and the extra rows only convert cleanly because the dates are ISO format.
- Checked: 5 Jurisdictions, 15 AxleLimits, SUM(MaxKg) = 252,300 for the seeded rows.

**No sqlite3 CLI on this machine**
- Saw: `sqlite3: command not found`.
- Fix (for now): checked the db with Python's built-in sqlite3 module. Need the CLI for the dump in step 7: `sudo apt install sqlite3`.

**roadrules.db not ignored by git**
- Saw: the `dotnet new gitignore` template has no `*.db` rule, so roadrules.db would have been committed (the guide says it's handled).
- Why: the template only ignores specific .db files (Thumbs.db, *.VC.db and so on).
- Fix: added `*.db`, `*.db-shm`, `*.db-wal` to .gitignore.

**HTTPS redirection warning**
- Saw: "Failed to determine the https port for redirect" when running on an http-only URL.
- Why: UseHttpsRedirection is in the template but no https endpoint was configured.
- Fix: none needed. Harmless here, and expected again in the container (step 8).

**Database moved to App_Data/**
- Change: connection string is now `Data Source=App_Data/roadrules.db` instead of the guide's `roadrules.db`.
- Why: keeps the database file out of the project root.
- Catch: SQLite creates the .db file but not a missing folder ("unable to open database file"), so Program.cs calls `Directory.CreateDirectory` on `App_Data` before `Migrate()`. Temporary: goes away with SQLite in step 7.
- Checked: deleted the db and the folder, ran the app, and it rebuilt both from the migration (5 / 15 / 252,300). The file stays git-ignored, as step 9 asks.

**Template pages removed**
- Removed Counter.razor and Weather.razor and their nav links once the step 2 interactivity check was done. Error.razor stays (UseExceptionHandler points at it).

**Checked working**
- /counter renders with the Interactive Server markers and blazor.web.js (`--all-interactive` puts @rendermode on Routes in App.razor).
- /jurisdictions shows 5 rows (AB, BC, MB, ON, QC), each with 3 limits.
- The Calculator nav link 404s until the page is added in evening 2. That's expected, since the guide adds both links in step 3.

## Evening 2 - calculator service, page, tests

**Guide's Calculator.razor does not compile**
- Saw: `error CS0542: 'Calculator': member names cannot be the same as their enclosing type`.
- Why: a Razor file becomes a class named after the file, so Calculator.razor is class `Calculator`, and `@inject IWeightCalculator Calculator` adds a property with the same name.
- Fix: `@inject IWeightCalculator Calc` and `await Calc.CalculateAsync(...)`. Nothing else changed.

**Service error message has a suffix**
- Saw: with the Range temporarily set to 1-10, Ontario + 5 axles shows "No limit is defined for 5 axles in ON. (Parameter 'axleCount')".
- Why: `ArgumentOutOfRangeException.Message` appends the parameter name. Left as written in the guide.

**Old result stays on screen when validation fails**
- Saw: calculate Ontario + 2 (17,000 kg), then enter 5: the validation message appears but the 17,000 line is still shown underneath.
- Why: `result` is only cleared inside `Calculate`, and `OnValidSubmit` doesn't fire when validation fails. Left as written in the guide; an `OnInvalidSubmit` that clears it would fix it.

**UI and error handling pass (beyond the guide)**
- Saw: the pages were plain template output: "Hello, world!" on Home, a Microsoft "About" link, a full-width form, black validation text, a blank page for unknown URLs, and only the yellow "An unhandled error has occurred" bar when a page throws.
- Changed: Home has two cards linking to the pages. Jurisdictions and Calculator sit in cards. The calculator shows a message under each field, a "whole number" message instead of "The AxleCount field must be a number.", the result in a green box with the jurisdiction name, and service errors without the "(Parameter 'axleCount')" suffix. `OnInvalidSubmit` clears an old result. There's a "Page not found" page (Router `NotFound` plus `UseStatusCodePagesWithReExecute("/not-found")`). An `ErrorBoundary` in MainLayout shows "Something went wrong" with a Try again button.
- Kept: the same fields, Range(1, 3), service calls and exception types, so the steps and tests in the guide still apply.
- Gotcha: `class="..."` on `<ValidationMessage>` replaces its `validation-message` class, so the text loses its red colour. Styled in app.css instead.
- Checked: renamed the Jurisdictions table while the app was running. The page showed "Something went wrong", and Try again worked once the table was back.

**No sidebar, and "An unhandled error has occurred" on every page**
- Saw: on Windows, the nav showed as plain links with a stray checkbox at the top left, and the yellow error bar was always visible, with no exception in the log.
- Why: the app was running outside Development. The launch profiles set Development, but running RoadRules.exe or `--no-launch-profile` skips them. Outside Development a normal build doesn't serve `RoadRules.styles.css` (the scoped CSS from MainLayout.razor.css and NavMenu.razor.css), so it returns 404. That file is also where `#blazor-error-ui { display: none }` lives, so the bar showed even though nothing had failed.
- Fix: `builder.WebHost.UseStaticWebAssets()` in Program.cs. Checked: in Production all CSS returns 200, the sidebar is back and the bar is hidden. Published output (the Docker image in step 8) already includes the file.

**Scoped CSS ids differ between Windows and WSL builds**
- Saw: even with UseStaticWebAssets and RoadRules.styles.css returning 200, there was still no sidebar and the error bar was still showing.
- Why: CSS isolation tags elements with a hashed scope id (`b-xxxxxxxxxx`), and the hash comes out different on Windows and Linux. obj/ is shared, and the Windows build reused the scoped CSS a WSL build had generated. The dll used `b-1kxk1zgr16`/`b-xdie7x66ip`, the CSS used `b-dgter85r8x`/`b-uht0d7c2mp`, so no rule matched.
- Fix: fixed ids in RoadRules.csproj (`CssScope="rr-main-layout"` and `"rr-nav-menu"`), and deleted obj/Debug/net8.0/scopedcss. Both sides now produce the same ids.

**Running from Windows and WSL at the same time**
- Saw: `SQLite Error 10: 'disk I/O error'` at `Migrate()`, and `MSB3021 ... RoadRules.dll ... Access to the path is denied` on build.
- Why: the app was already running from Windows, which locks bin/RoadRules.dll and roadrules.db. SQLite locking doesn't work across the Windows/WSL boundary.
- Fix: run only one copy at a time against App_Data/roadrules.db.

**Checked working**
- `dotnet test`: 6 passed (3 Theory cases, 3 Facts). Changed the fake's tandem to 18000: 1 failed (Expected 17000, Actual 18000). Put it back: 6 passed.
- /calculator in a browser: no jurisdiction shows "Pick a jurisdiction."; Ontario + 2 axles gives 17,000 kg; Ontario + 5 is blocked by "Axle count must be between 1 and 3."; Range temporarily 1-10 + 5 axles shows the service's own message, then put back to 1-3.
- Dropdown lists the 5 jurisdictions by name (Alberta first). Nav: Home, Jurisdictions, Calculator; the Calculator link no longer 404s.

**Redesign**
- Asked for a more modern UI. Changed: dark sidebar with logo and icons (hamburger menu on mobile), a top bar with breadcrumb and an "illustrative data" badge, and a gradient header on Overview with live counts from the database (jurisdictions, limits, axle groups). The Jurisdictions table is in a card with code badges. The calculator has two columns: the form, with Single/Tandem/Tridem buttons that fill in the axle count, and a result panel with the kg value, the pounds equivalent, and a truck diagram with the axle group highlighted. 404 and error states match the rest.
- Kept: same pages, fields, Range(1, 3), Required, service calls and exception handling. The 6 tests still pass. Page titles are now `h1` (the template's FocusOnNavigate looks for `h1`). The Bootstrap in the template is 5.1, so the styling is plain CSS in app.css rather than 5.2+ variables.
- `#blazor-error-ui` styling moved to app.css, so the bar stays hidden even if the scoped CSS bundle fails again.
- Gotcha: after typing something that isn't a number, InputNumber keeps its "must be a whole number" message, even after the value is set from code. Clicking a Single/Tandem/Tridem button creates a fresh EditContext to clear it. A `@key` on the input did not clear it.
- Build hygiene: Visual Studio (Windows) and WSL must not build into the same obj/. Its paths are OS-specific (WSL build: `CS0234 ... 'Components' does not exist`, from D:/ paths in the generated editorconfig). Pick one side per build, or build a copy.

**Fixed table headers (project-wide rule)**
- Requirement: every table's header row stays visible while scrolling.
- How: wrap the table in `<div class="rr-table-wrap">`. The wrapper is the scroll box (capped at `max(320px, 100vh - 300px)`), and `thead th` inside it is `position: sticky; top: 0`. It works for both `.rr-table` and Bootstrap `.table`. The header line is an inset box-shadow, because a border on a sticky cell scrolls away, and Bootstrap 5.1's 2px line on top of tbody is removed.
- Why a wrapper and not page-level sticky: sticky sticks to the nearest scrolling ancestor, and the horizontal-scroll wrapper narrow screens need is one, so a header sticky to the page would never stick.
- Applied: Jurisdictions. **To do in steps 11-12:** the guide's new tables (`<table class="table">` at the history, comparison and admin steps) need the same wrapper.
- Checked: with 45 jurisdictions, and a 60-row Bootstrap table on a test page that isn't committed, at 1440px and 390px: after scrolling the table, the header stays in place and is the top element at that spot.

## Evening 3 - SQLite to SQL Server, Dockerfile, README

**7a. Extra rows and baseline**
- Inserted Ids 16-25 into App_Data/roadrules.db with the sqlite3 CLI (installed now: `apt install sqlite3`, 3.45.1).
- Baseline (SQLite): **25 rows, SUM(MaxKg) = 416,350, 5 jurisdictions.** This matches the figure worked out from the guide's data.

**sqlite3 dump has no quotes and no column list**
- Saw: `.mode insert AxleLimits` gives `INSERT INTO AxleLimits VALUES(16,1,'Single',1,9500,'2026-07-01');`. The guide says names come out in double quotes; with 3.45 they don't, and there is no column list either.
- Why it matters: SQL Server only allows explicit identity values with IDENTITY_INSERT ON and a column list ("An explicit value for the identity column ... can only be specified when a column list is used and IDENTITY_INSERT is ON").
- Fix: extra_rows.sql now has the column list, `SET IDENTITY_INSERT AxleLimits ON/OFF`, and the reconcile queries at the end. Dates stay as ISO strings, which SQL Server converts to `date`.

**7c. Provider switch**
- Added Microsoft.EntityFrameworkCore.SqlServer 8.0.33 (pinned to match, as with Sqlite on evening 1), changed UseSqlite to UseSqlServer, and set the connection string from the guide.
- Removed the `App_Data` folder creation from Program.cs, which was SQLite-only. The Sqlite package stays for now; nothing uses it.
- Deleted Migrations/ and regenerated `InitialSqlServer`. Differences from the SQLite migration:
  1. `EffectiveFrom` is `date` (was `TEXT`).
  2. Ids use `.Annotation("SqlServer:Identity", "1, 1")` (was `Sqlite:Autoincrement`, type `INTEGER`).
  3. `Code` is `nvarchar(450)`, the others `nvarchar(max)`. Code has the unique index, and SQL Server can't index `max` columns (900-byte key limit).
- `dotnet ef` warns that tools 8.0.31 are older than runtime 8.0.33 (same as evening 1); harmless.

**8. Dockerfile**
- Multi-stage as in the guide (sdk:8.0 build, aspnet:8.0 runtime, port 8080). The publish output includes wwwroot/RoadRules.styles.css, so the CSS problem from evening 2 can't happen in the container.
- The guide's .dockerignore `*.db` only matches the root of the build context (unlike .gitignore), so RoadRules/App_Data/roadrules.db would have gone into the image. Changed to `**/*.db` (plus -shm/-wal and `**/*.user`).

**Environment**
- The dev shell (WSL) is itself a container with no Docker CLI, so steps 7b, 7d-7f and the container run were done from Windows (Docker Desktop).

**7b-7f and 8 results (from the Windows run)**
- SQL Server container up: yes. `mcr.microsoft.com/mssql/server:2022-latest` pulled and started as roadrules-sql on Docker Desktop (Windows).
- Migration applied (InitialSqlServer, 15 seeded rows): yes. The app started against SQL Server and Migrate() created the database on first run.
- extra_rows.sql run: `docker cp extra_rows.sql roadrules-sql:/tmp/` then `sqlcmd ... -i /tmp/extra_rows.sql`. The guide's `-i /dev/stdin < extra_rows.sql` is bash only; PowerShell has no `<` redirect. Its output wasn't saved, but the reconcile below shows all 10 rows arrived.
- Reconcile against SQL Server: **COUNT 25, SUM(MaxKg) 416,350, Jurisdictions 5**, identical to the SQLite baseline. Nothing lost or duplicated.
- Spot check, field by field, both match the inserted values:
  - `16 | 1 | Single | 1 | 9500 | 2026-07-01`
  - `25 | 4 | Tridem | 3 | 24100 | 2026-07-01`
  - EffectiveFrom shows as a real `date`. sqlcmd prints AxleGroup very wide because it is `nvarchar(max)`.
- App on SQL Server, Ontario + 2 axles: **first run showed 17,500, not the 17,000 the guide expects.**
  - Why: Ontario now has two tandem rows (Id 2: 17,000 from 2026-01-01; Id 17: 17,500 from 2026-07-01). EfLimitSource only did `OrderBy(AxleCount)`, and WeightCalculator takes the first match. Ties come back in no guaranteed order: SQLite returned insert order (17,000), SQL Server returned 17,500 first. So the guide's "still returns 17,000" relied on undefined ordering.
  - Fix: `.ThenBy(l => l.EffectiveFrom)` in EfLimitSource (the line step 11 adds anyway), so the oldest row comes first and the result is the same every time. It still ignores dates, which is the deliberate gap step 11 closes.
  - After the fix: Ontario + 2 axles shows **17,000 kg** (≈ 37,479 lb), as the guide expects.
- docker build / run: the image built and ran with the connection string passed as `ConnectionStrings__Default` (host.docker.internal,1433). At http://localhost:8080/calculator, Ontario + 2 axles shows **17,000 kg**, so the container reaches SQL Server in the other container and the migration ran on startup.

## Evening 4 - effective-dated limits, history, comparison (steps 11-12)

**11a-11b. The rule moved into the calculator**
- `LimitRecord` and `WeightResult` carry `EffectiveFrom`. `ILimitSource.GetAllLimitsAsync` returns every row, history included, and selects nothing.
- `WeightCalculator.EffectiveLimitsAsync`: per axle group, the latest row with `EffectiveFrom <= asOf`. `CalculateAsync` takes an as-of date and uses it.
- Design note: with a table this size, loading one jurisdiction's rows and selecting in C# is the right trade, because the rule can be tested without SQL. For a large table the selection would go into the database, with `ROW_NUMBER() OVER (PARTITION BY AxleCount ORDER BY EffectiveFrom DESC)` or a filtered query per group.
- The evening 3 `.ThenBy(EffectiveFrom)` fix is now the guide's own line. Ordering no longer decides the result, because the rule picks by date.
- No migration: `dotnet ef migrations has-pending-model-changes` says "No changes have been made to the model since the last migration". EffectiveFrom was in the model from the start.

**11c. Calculator**
- "As of" date (InputDate, defaults to today). The result shows the date asked, "in effect since", and a link to that jurisdiction's history.
- The date box shows in the browser's locale (07/01/2026 here); the page shows dates as yyyy-MM-dd.

**11d. History page** (`/history/{Code}`)
- `OnParametersSetAsync`, not `OnInitializedAsync`: going from /history/ON to /history/QC keeps the same component and only changes the parameter. Checked: Blazor.navigateTo from ON to QC reloads the rows.
- Added beyond the guide: a status per row (In effect / Superseded / Scheduled), using `EffectiveLimitsAsync(Code, today)` so the rule isn't written twice; an empty state for an unknown code (/history/ZZ); and links from the Jurisdictions page (code badge and a "View history" button).

**12. Comparison page** (`/compare`)
- As in the guide: jurisdictions as rows, axle groups as columns, `@bind-Value:after="LoadAsync"` on the date. Added preset buttons for the guide's two check dates and today, and "since <date>" under each value.
- Both new tables use `.rr-table-wrap` (fixed header), the to-do from evening 2.

**11e. Tests: 10 passing**
- The 6 original tests pass `new DateOnly(2026, 3, 1)`. FakeLimitSource has dates plus Ontario's July tandem row. EffectiveDateTests adds 4: 2026-03-01 gives 17,000, 2026-07-01 gives 17,500, 2026-12-31 gives 17,500, and 2025-06-01 throws.

**Checked in a browser**
- WSL can't reach SQL Server, so this ran on a scratch copy switched to SQLite (EnsureCreated), loaded with the same 25 rows (SUM 416,350). The project itself stays on SQL Server.
- Compare: today and 2026-08-01 show Ontario tandem 17,500 (since 2026-07-01); 2026-03-01 shows 17,000 (since 2026-01-01); 2025-06-01 shows "–". The same results when the date is typed instead of picked.
- Calculator, Ontario with 2 axles: 2026-03-01 gives 17,000, 2026-07-01 and 2026-12-31 give 17,500, and 2025-06-01 gives "No limit is in effect for 2 axles in ON on 2025-06-01."
- History ON: July rows In effect, January rows Superseded, Tridem 24,000 In effect. No error bar and no failed requests.

