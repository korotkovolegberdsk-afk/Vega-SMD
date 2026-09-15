INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'Datasheet', 'TDK SMD Inductor Catalogue',
       'https://product.tdk.com/en/search/inductor/inductor/smd/catalog',
       'TDK official SMD inductor catalogue: EIA 1810 package reference.'
FROM PackageDefinition p
WHERE p.PackageName='IND1810'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='TDK SMD Inductor Catalogue');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'Datasheet', 'TDK SMD Inductor Catalogue',
       'https://product.tdk.com/en/search/inductor/inductor/smd/catalog',
       'TDK official SMD inductor catalogue: EIA 2827 package reference.'
FROM PackageDefinition p
WHERE p.PackageName='IND2827'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='TDK SMD Inductor Catalogue');
