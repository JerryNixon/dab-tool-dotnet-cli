;WITH types AS 
(    
    SELECT DISTINCT system_type_id,  
    CASE   
        WHEN system_type_id IN (34, 35, 99, 173, 165, 167, 175, 231, 239) THEN 'string'    
        WHEN system_type_id IN (36, 189) THEN 'Guid'    
        WHEN system_type_id IN (48) THEN 'byte'    
        WHEN system_type_id IN (52) THEN 'short'   
        WHEN system_type_id IN (56) THEN 'int'
        WHEN system_type_id IN (58, 61) THEN 'DateTime' 
        WHEN system_type_id IN (59, 62, 106, 108, 122) THEN 'decimal'  
        WHEN system_type_id IN (60, 127) THEN 'double'  
        WHEN system_type_id IN (98) THEN 'object'  
        WHEN system_type_id IN (104) THEN 'bool'   
        WHEN system_type_id IN (241) THEN 'Xml'    
        ELSE 'unknown'    
    END AS net_type  
    FROM sys.types  
)    
, procedures AS
(    
    SELECT  
    s.name AS schema_name,  
    p.name AS object_name,  
    STRING_AGG(REPLACE(d.name, '@', ''), ',') AS parameter_names,   
    STRING_AGG(t.name, ',') AS parameter_sql_types,  
    STRING_AGG(CONCAT(ttypes.net_type, CASE WHEN t.is_nullable = 1 THEN '?' END), ',') AS parameter_net_types 
    FROM sys.procedures p  
    JOIN sys.schemas s ON p.schema_id = s.schema_id 
    LEFT JOIN sys.parameters d ON p.object_id = d.object_id AND d.parameter_id > 0
    LEFT JOIN sys.types t ON d.system_type_id = t.system_type_id   
    LEFT JOIN types AS ttypes ON ttypes.system_type_id = t.system_type_id    
    WHERE p.is_ms_shipped = 0   
    GROUP BY s.name, p.name
)    
SELECT    
    p.schema_name,  
    p.object_name,  
    'procedure' AS object_type,  
    p.parameter_names,
    p.parameter_sql_types,
    p.parameter_net_types,
    STRING_AGG(r.name, ',') AS column_names,
    STRING_AGG(TYPE_NAME(r.system_type_id), ',') AS column_sql_types,    
    STRING_AGG(CONCAT(rtypes.net_type, CASE WHEN r.is_nullable = 1 THEN '?' END), ',') AS column_net_types,   
    STRING_AGG('false', ',') AS primary_key,   
    STRING_AGG('true', ',') AS is_computed,
    STRING_AGG(CASE WHEN r.name LIKE '%id' OR r.name LIKE 'id%' OR r.name LIKE '%pk%' OR r.name LIKE '%uid%' OR r.name LIKE '%key%' THEN 'true' ELSE 'false' END, ',') AS inferred_key
FROM procedures AS p
CROSS APPLY sys.dm_exec_describe_first_result_set(N'EXEC ' + QUOTENAME(p.schema_name) + '.' + QUOTENAME(p.object_name), NULL, 0) AS r 
JOIN types AS rtypes ON rtypes.system_type_id = r.system_type_id 
GROUP BY 
    p.schema_name,  
    p.object_name,  
    p.parameter_names,
    p.parameter_sql_types,
    p.parameter_net_types
 
UNION
    
SELECT
    s.name AS schema_name,
    t.name AS object_name,
    'table' AS object_type,
    NULL, NULL, NULL,
    STRING_AGG(c.name, ',') AS column_names,
    STRING_AGG(TYPE_NAME(c.system_type_id), ',') AS column_sql_types,
    STRING_AGG(CONCAT(ctypes.net_type, CASE WHEN c.is_nullable = 1 THEN '?' END), ',') AS column_net_types,    
    STRING_AGG(CASE WHEN ic.column_id IS NOT NULL THEN 'true' ELSE 'false' END, ',') AS primary_key,
    STRING_AGG(CASE WHEN cc.name IS NOT NULL THEN 'true' WHEN c.is_identity = 1 THEN 'true' ELSE 'false' END, ',') AS is_computed,
    STRING_AGG(CASE WHEN c.name LIKE '%id' OR c.name LIKE 'id%' OR c.name LIKE '%pk%' OR c.name LIKE '%uid%' OR c.name LIKE '%key%' THEN 'true' ELSE 'false' END, ',') AS inferred_key
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
JOIN sys.columns c ON t.object_id = c.object_id AND PATINDEX('%[^a-zA-Z0-9_]%', c.name) = 0
JOIN types AS ctypes ON ctypes.system_type_id = c.system_type_id 
LEFT JOIN sys.indexes i ON i.object_id = t.object_id AND i.is_primary_key = 1
LEFT JOIN sys.index_columns ic ON ic.object_id = t.object_id AND ic.index_id = i.index_id AND ic.column_id = c.column_id
LEFT JOIN sys.computed_columns cc ON c.object_id = cc.object_id AND c.column_id = cc.column_id
WHERE c.graph_type IS NULL 
GROUP BY 
    s.name,
    t.name

UNION
 
SELECT    
    s.name AS schema_name,    
    v.name AS object_name,    
    'view' AS object_type,    
    NULL, NULL, NULL,    
    STRING_AGG(c.name, ',') AS column_names,
    STRING_AGG(TYPE_NAME(c.system_type_id), ',') AS column_sql_types,
    STRING_AGG(CONCAT(ctypes.net_type, CASE WHEN c.is_nullable = 1 THEN '?' ELSE '' END), ',') AS column_net_types,    
    STRING_AGG('false', ',') AS primary_key,
    STRING_AGG(CASE WHEN cc.name IS NOT NULL THEN 'true' ELSE 'false' END, ',') AS is_computed,
    STRING_AGG(CASE WHEN c.name LIKE '%id' OR c.name LIKE 'id%' OR c.name LIKE '%pk%' OR c.name LIKE '%uid%' OR c.name LIKE '%key%' THEN 'true' ELSE 'false' END, ',') AS inferred_key
FROM sys.views v    
JOIN sys.schemas s ON v.schema_id = s.schema_id   
JOIN sys.columns c ON v.object_id = c.object_id   
JOIN types AS ctypes ON ctypes.system_type_id = c.system_type_id 
LEFT JOIN sys.key_constraints pkc ON c.object_id = pkc.parent_object_id AND c.column_id = pkc.unique_index_id 
LEFT JOIN sys.computed_columns cc ON c.object_id = cc.object_id AND c.column_id = cc.column_id
GROUP BY 
    s.name,
    v.name 