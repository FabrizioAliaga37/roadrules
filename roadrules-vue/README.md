# RoadRules (Vue)

The weight calculator built a second time as a Vue 3 page that talks to the RoadRules minimal API (guide step 13b).
It exists to be converted to Blazor: the Blazor version is `RoadRules.Client/Pages/CalculatorApi.razor` (`/calculator-api`),
and both call the same API, so they must give the same answers.

This app has no database or business logic of its own. Everything comes from the .NET app's `/api/...` endpoints.

## Requirements

- **Node.js 20.19+ or 22.12+** (Vite 8 does not support Node 18). Check with `node --version`.
  Install the LTS from https://nodejs.org or `winget install OpenJS.NodeJS.LTS`, then reopen the terminal.
- The **RoadRules .NET app running**, which itself needs SQL Server on `localhost,1433`.

## Run

1. Start the API. Either:
   - Visual Studio: set **RoadRules** (not RoadRules.Client) as the startup project, pick the **http** profile, press F5; or
   - terminal, from the repository root: `dotnet run --project RoadRules --launch-profile http`

   Note the `http://` address it listens on (default `http://localhost:5090`).

2. Start the Vue dev server in a second terminal:

   ```
   cd roadrules-vue
   npm install
   npm run dev
   ```

3. Open the URL Vite prints, normally **http://localhost:5173**.

If you upgraded Node after a first `npm install`, delete `node_modules` and run `npm install` again.

**Windows PowerShell: "npm.ps1 cannot be loaded because running scripts is disabled".** Either use `npm.cmd install` /
`npm.cmd run dev` (or run from Command Prompt), or allow local scripts once for your user:

```
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

## Configuration

### API address

Set in `src/App.vue`:

```js
const apiBase = 'http://localhost:5090'
```

If the .NET app runs on a different port, change it here and save; Vite reloads the page.
Prefer the `http://` address. An `https://` one only works if the dev certificate is trusted
(`dotnet dev-certs https --trust`).

### CORS (Vue port)

The API only accepts browser calls from `http://localhost:5173`. That's the `"vite"` CORS policy in `RoadRules/Program.cs`:

```csharp
p.WithOrigins("http://localhost:5173")
```

If Vite starts on another port (for example 5174 because 5173 is busy), every call fails with a CORS error in the browser
console (F12). Free port 5173, or change that line to the new port and restart the .NET app.

## What to test

With both apps running:

| # | Action | Expected |
|---|--------|----------|
| 1 | Open the page | Jurisdiction dropdown fills (loaded on mount from `/api/jurisdictions`) |
| 2 | No jurisdiction selected | **Calculate** is disabled, no validation message |
| 3 | Axles 0 or 4 | **Calculate** is disabled |
| 4 | Ontario, 1 axle | 9,500 kg, in effect since 2026-07-01 |
| 5 | Ontario, 2 axles | 17,500 kg, in effect since 2026-07-01 |
| 6 | Ontario, 3 axles | 24,000 kg, in effect since 2026-01-01 |
| 7 | Stop the .NET app, click Calculate | "Request failed" |
| 8 | Stop the .NET app, reload the page | Empty dropdown, **no error shown** (known flaw) |

The page sends no date, so the API uses **today**. Rows 4 to 6 are the limits in effect from 2026-07-01 onwards
(the July rows come from `extra_rows.sql`, step 11). `RoadRules.Tests/ParityTests.cs` checks the same cases on a fixed date,
2026-03-01, so it expects the older values: 9,000 / 17,000 / 24,000.

Run the same checks on `/calculator-api` in the .NET app. The numbers must match the Vue page.

## Behaviour to preserve in the Blazor conversion

What the component does that isn't obvious from the markup (guide 13b):

- The button is disabled by a computed property (`canSubmit`), not by validation messages.
- The axle input is bound as a number (`v-model.number`), not a string.
- The error message comes from the API's JSON body (`{ message }`) when there is one, and `"Request failed"` otherwise.
- Jurisdictions load once, on mount.
- A failed jurisdictions load leaves an empty dropdown with no message.
- The request sends no `asOf`, so the API uses today's date.

Deliberate differences in the Blazor version are recorded in `NOTES.md`.

## Files

| File | Purpose |
|------|---------|
| `src/App.vue` | Page shell (sidebar, top bar); sets the API address |
| `src/style.css` | Styles, using the same design tokens as the Blazor app's `app.css` |
| `src/components/Calculator.vue` | The calculator: dropdown, axle input, result, error |
| `src/main.js` | Mounts the app |
| `vite.config.js` | Vite + Vue plugin config |

`node_modules/` and `dist/` are ignored by `.gitignore`.

## Other commands

- `npm run build` builds a static bundle into `dist/`
- `npm run preview` serves the built `dist/` locally
