-- The 10 AxleLimits rows that existed only in SQLite (Ids 16-25), fixed to run on SQL Server.
-- Raw dump: sqlite3 roadrules.db ".mode insert AxleLimits" "SELECT * FROM AxleLimits WHERE Id > 15;"
-- Fixes:
--   1. Added the column list. sqlite3 3.45 dumps "INSERT INTO AxleLimits VALUES(...)" with none, and
--      SQL Server only accepts explicit identity values when a column list is given.
--   2. SET IDENTITY_INSERT ON/OFF around the inserts, so the rows keep their original Ids.
--   3. Names: this sqlite3 version did not double-quote them, so nothing to change; brackets would also work.
--   4. Dates stay as '2026-07-01' strings; SQL Server converts them to date because the format is ISO.
SET IDENTITY_INSERT AxleLimits ON;

INSERT INTO AxleLimits (Id, JurisdictionId, AxleGroup, AxleCount, MaxKg, EffectiveFrom) VALUES
(16, 1, 'Single', 1, 9500, '2026-07-01'),
(17, 1, 'Tandem', 2, 17500, '2026-07-01'),
(18, 2, 'Single', 1, 9200, '2026-07-01'),
(19, 3, 'Tridem', 3, 24500, '2026-07-01'),
(20, 4, 'Tandem', 2, 17200, '2026-07-01'),
(21, 5, 'Single', 1, 9300, '2026-07-01'),
(22, 5, 'Tandem', 2, 17100, '2026-07-01'),
(23, 2, 'Tridem', 3, 26500, '2026-07-01'),
(24, 3, 'Single', 1, 9150, '2026-07-01'),
(25, 4, 'Tridem', 3, 24100, '2026-07-01');

SET IDENTITY_INSERT AxleLimits OFF;

-- Reconcile (step 7e): must equal the SQLite baseline of 25 rows, SUM(MaxKg) 416350, 5 jurisdictions.
SELECT COUNT(*) AS Rows, SUM(MaxKg) AS SumMaxKg FROM AxleLimits;
SELECT COUNT(*) AS Jurisdictions FROM Jurisdictions;
SELECT * FROM AxleLimits WHERE Id IN (16, 25);
