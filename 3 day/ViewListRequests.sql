CREATE OR REPLACE VIEW viewlistrequests AS
SELECT 
    requestid,
    requesttype,
    status,
    rejectionreason,
    startdate,
    enddate,
    visitpurpose,
    targetdepartment,
    note,
    createdat,
    visitor_lastname,
    visitor_firstname,
    visitor_passportdata
FROM visitrequests;