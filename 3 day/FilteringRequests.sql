DROP FUNCTION IF EXISTS filteringrequests(character varying, character varying, character varying);

CREATE OR REPLACE FUNCTION filteringrequests(
    v_type VARCHAR DEFAULT NULL,
    v_dept VARCHAR DEFAULT NULL,
    v_stat VARCHAR DEFAULT NULL
)
RETURNS TABLE (
    requestid INTEGER,
    requesttype VARCHAR,
    status VARCHAR,
    rejectionreason TEXT,
    startdate DATE,
    enddate DATE,
    visitpurpose TEXT,
    targetdepartment VARCHAR,
    note TEXT,
    createdat TIMESTAMP,
    visitor_lastname VARCHAR,
    visitor_firstname VARCHAR,
    visitor_passportdata VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        r.requestid,
        r.requesttype,
        r.status,
        r.rejectionreason,
        r.startdate,
        r.enddate,
        r.visitpurpose,
        r.targetdepartment,
        r.note,
        r.createdat,
        r.visitor_lastname,
        r.visitor_firstname,
        r.visitor_passportdata
    FROM visitrequests r
    WHERE 
        (v_type IS NULL OR r.requesttype = v_type)
        AND (v_dept IS NULL OR r.targetdepartment = v_dept)
        AND (v_stat IS NULL OR r.status = v_stat)
    ORDER BY r.createdat DESC;
END;
$$ LANGUAGE plpgsql;