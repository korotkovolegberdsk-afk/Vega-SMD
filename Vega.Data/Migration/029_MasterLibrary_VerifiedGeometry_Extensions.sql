-- Extends verified manufacturer lead geometry without changing existing rows.
-- Pitch remains available for backward compatibility; the nullable fields are
-- axis-specific and must be populated only when present in a source drawing.

ALTER TABLE PackageLeadGeometry ADD COLUMN PitchAlongRow REAL;
ALTER TABLE PackageLeadGeometry ADD COLUMN RowSpacing REAL;
ALTER TABLE PackageLeadGeometry ADD COLUMN TerminalSpan REAL;
