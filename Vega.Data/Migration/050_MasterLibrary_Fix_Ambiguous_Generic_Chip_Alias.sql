-- Generic metric aliases must resolve to one canonical chip package.
-- VAR0603 has its own explicit package name and must not claim generic 0603.
DELETE FROM MasterLibrary_PackageAliases
WHERE lower(Alias) = '0603'
  AND PackageId IN (SELECT Id FROM PackageDefinition WHERE PackageName = 'VAR0603');
