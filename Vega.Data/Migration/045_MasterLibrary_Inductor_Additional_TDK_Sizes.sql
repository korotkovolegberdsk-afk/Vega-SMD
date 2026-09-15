-- Additional EIA sizes listed by TDK for SMD inductors.
INSERT OR IGNORE INTO PackageDefinition(
    PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,
    Length,Width,Height,BodyLength,BodyWidth,PadCount,LeadCount,IPCName,JEDECName,
    MirtecAoiClass,Description,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT 'IND1810','IND1810','IND1810','INDUCTOR','Inductor',c.Id,f.Id,
       4.50,2.40,1.20,4.50,2.40,2,0,'INDUCTOR','4524',
       'INDUCTOR','Standard chip inductor package',
       'TDK SMD inductor selection guide: EIA 1810 nominal body size.',
       1,datetime('now'),datetime('now')
FROM PackageCategory c JOIN PackageFamily f ON f.CategoryId=c.Id AND f.Code='INDUCTOR'
WHERE c.Code='PASSIVE';

INSERT OR IGNORE INTO PackageDefinition(
    PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,
    Length,Width,Height,BodyLength,BodyWidth,PadCount,LeadCount,IPCName,JEDECName,
    MirtecAoiClass,Description,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT 'IND2827','IND2827','IND2827','INDUCTOR','Inductor',c.Id,f.Id,
       7.80,2.70,2.00,7.80,2.70,2,0,'INDUCTOR','7827',
       'INDUCTOR','Standard chip inductor package',
       'TDK SMD inductor selection guide: EIA 2827 nominal body size.',
       1,datetime('now'),datetime('now')
FROM PackageCategory c JOIN PackageFamily f ON f.CategoryId=c.Id AND f.Code='INDUCTOR'
WHERE c.Code='PASSIVE';

INSERT INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT p.Id,'IND-'||substr(p.PackageName,4),'Standard','TDK inductor size reference'
FROM PackageDefinition p
WHERE p.PackageName IN ('IND1810','IND2827')
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageAliases a WHERE a.PackageId=p.Id AND a.Alias='IND-'||substr(p.PackageName,4));
