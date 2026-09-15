-- Catalogue series are not orderable manufacturer part numbers.
DELETE FROM ComponentDefinition
WHERE ManufacturerPartNumber IN ('MHQ0402PSA','MHQ0603P','MHQ1005P','MLG0402Q','MLG0603P','MLG1005S','MLJ1005H','MLJ1608W','MLF1005','MLF1608','MLF2012','B82422A','B82432C','B82422T','NLV25-EF','NLV32-EF');
