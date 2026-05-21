CREATE OR REPLACE VIEW entrant_view AS
SELECT 
  e.FullName,
  e.CEI,
  e.DT,
  e.Phone,
  e.HeiSpecId,
  e.BDate
FROM Entrant e; 

CREATE OR REPLACE VIEW spec_test_view AS
SELECT
  st.HeiSpecId,
  st.TestId
FROM SpecTest st; 

CREATE OR REPLACE VIEW hei_spec_view AS
SELECT
  hs.HeiId,
  hs.SpecId
FROM HeiSpec hs;

CREATE OR REPLACE VIEW ent_test_view AS
SELECT 
  et.Name
FROM EntTest et;

CREATE OR REPLACE VIEW spec_view AS
SELECT 
  s.Name
FROM Spec s;

CREATE OR REPLACE VIEW hei_view AS
SELECT 
  h.Name
FROM Hei h;

CREATE OR REPLACE VIEW comp_educ_inst_view AS
SELECT
  cei.Name
FROM CompEducInst cei;
