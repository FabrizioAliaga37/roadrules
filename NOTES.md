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

**Checked working**
- /counter renders with the Interactive Server markers and blazor.web.js (`--all-interactive` puts @rendermode on Routes in App.razor).
- /jurisdictions shows 5 rows (AB, BC, MB, ON, QC), each with 3 limits.
- The Calculator nav link 404s until the page is added in evening 2. That's expected, since the guide adds both links in step 3.
