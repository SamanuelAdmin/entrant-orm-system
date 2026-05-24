CREATE OR REPLACE TRIGGER trg_comp_educ_inst_view_ins
INSTEAD OF INSERT ON comp_educ_inst_view
FOR EACH ROW
BEGIN
    -- Insert record into base table using the name from the view
    INSERT INTO CompEducInst (Name)
    VALUES (:NEW.Name);
END;
/

CREATE OR REPLACE TRIGGER trg_hei_view_ins
INSTEAD OF INSERT ON hei_view
FOR EACH ROW
BEGIN
    -- Insert record into base table using the name from the view
    INSERT INTO Hei (Name)
    VALUES (:NEW.Name);
END;
/

CREATE OR REPLACE TRIGGER trg_spec_view_ins
INSTEAD OF INSERT ON spec_view
FOR EACH ROW
BEGIN
    -- Insert record into base table using the name from the view
    INSERT INTO Spec (Name)
    VALUES (:NEW.Name);
END;
/

CREATE OR REPLACE TRIGGER trg_ent_test_view_ins
INSTEAD OF INSERT ON ent_test_view
FOR EACH ROW
BEGIN
    -- Insert record into base table using the name from the view
    INSERT INTO EntTest (Name)
    VALUES (:NEW.Name);
END;
/

CREATE OR REPLACE TRIGGER trg_hei_spec_view_ins
INSTEAD OF INSERT ON hei_spec_view
FOR EACH ROW
BEGIN
    -- Insert foreign keys into the junction table
    INSERT INTO HeiSpec (HeiId, SpecId)
    VALUES (:NEW.HeiId, :NEW.SpecId);
END;
/

CREATE OR REPLACE TRIGGER trg_spec_test_view_ins
INSTEAD OF INSERT ON spec_test_view
FOR EACH ROW
BEGIN
    -- Insert foreign keys into the junction table
    INSERT INTO SpecTest (HeiSpecId, TestId)
    VALUES (:NEW.HeiSpecId, :NEW.TestId);
END;
/

CREATE OR REPLACE TRIGGER trg_entrant_view_ins
INSTEAD OF INSERT ON entrant_view
FOR EACH ROW
BEGIN
    -- Insert fields from the view, using SYSDATE for FDate and a placeholder for Addr
    INSERT INTO Entrant (FullName, BDate, CEI, FDate, DT, Addr, Phone, HeiSpecId)
    VALUES (
        :NEW.FullName, 
        :NEW.BDate, 
        :NEW.CEI, 
        SYSDATE, 
        :NEW.DT, 
        'Not Specified', 
        :NEW.Phone, 
        :NEW.HeiSpecId
    );
END;
/
