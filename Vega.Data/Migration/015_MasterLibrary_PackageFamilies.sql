INSERT OR IGNORE INTO PackageCategory (Code, Name, Description, SortOrder, IsActive) VALUES
('PASSIVE','Passive components','Resistors, capacitors and inductors',10,1),
('DISCRETE','Discrete semiconductors','Diodes and transistors',20,1),
('IC','Integrated circuits','Integrated-circuit packages',30,1),
('CONNECTOR','Connectors','Electrical interconnects',40,1),
('OPTO','Optoelectronics','LED and optical packages',50,1),
('ELECTROMECHANICAL','Electromechanical','Relays, crystals and transformers',60,1),
('CUSTOM','Custom','Customer-specific packages',90,1);

WITH families(Code, Name, CategoryCode, SortOrder) AS (VALUES
('CHIP','Chip components','PASSIVE',10),('MELF','MELF','DISCRETE',20),('SOD','Small outline diode','DISCRETE',30),('SOT','Small outline transistor','DISCRETE',40),
('SOP','Small outline package','IC',50),('SOIC','Small outline IC','IC',60),('SSOP','Shrink small outline','IC',70),('TSSOP','Thin shrink small outline','IC',80),('MSOP','Mini small outline','IC',90),('TSOP','Thin small outline','IC',100),
('QFP','Quad flat package','IC',110),('QFN','Quad flat no-lead','IC',120),('DFN','Dual flat no-lead','IC',130),('SON','Small outline no-lead','IC',140),('LGA','Land grid array','IC',150),('BGA','Ball grid array','IC',160),('CSP','Chip scale package','IC',170),('PLCC','Plastic leaded chip carrier','IC',180),('SOJ','Small outline J-lead','IC',190),('DIP','Dual in-line package','IC',200),('DPAK','Power package','DISCRETE',210),
('CONNECTOR','Connector','CONNECTOR',220),('LED','LED','OPTO',230),('CRYSTAL','Crystal','ELECTROMECHANICAL',240),('OSCILLATOR','Oscillator','ELECTROMECHANICAL',250),('ALUMINUM_CAP','Aluminum capacitor','PASSIVE',260),('TANTALUM_CAP','Tantalum capacitor','PASSIVE',270),('INDUCTOR','Inductor','PASSIVE',280),('TRANSFORMER','Transformer','ELECTROMECHANICAL',290),('RELAY','Relay','ELECTROMECHANICAL',300),('CUSTOM','Custom','CUSTOM',310))
INSERT OR IGNORE INTO PackageFamily(CategoryId, Code, Name, SortOrder, IsActive)
SELECT c.Id, f.Code, f.Name, f.SortOrder, 1 FROM families f JOIN PackageCategory c ON c.Code=f.CategoryCode;