drop function public.get_table_metadata;
CREATE  function public.get_table_metadata(_table_name varchar)
    RETURNS TABLE (
          column_name information_schema.sql_identifier   -- also visible as OUT param in function body
        , data_type information_schema.sql_identifier
        , is_primary_key boolean
        , is_nullable boolean        
        , is_self_referencing boolean
        , character_maximum_length information_schema.cardinal_number
        , ordinal_position information_schema.cardinal_number)
    LANGUAGE plpgsql AS
    
 $func$
begin
RETURN QUERY


SELECT
    c.column_name as column_name,
    c.udt_name as data_type,
    CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END as is_primary_key,
    CASE WHEN c.is_nullable = 'YES' THEN true ELSE false END as is_nullable,
    CASE WHEN c.is_self_referencing = 'YES' THEN true ELSE false END as is_self_referencing,
    c.character_maximum_length,
    c.ordinal_position


FROM information_schema.columns c
         LEFT JOIN (
    SELECT ku.column_name
    FROM information_schema.table_constraints tc
             JOIN information_schema.key_column_usage ku
                  ON tc.constraint_name = ku.constraint_name
    WHERE tc.constraint_type = 'PRIMARY KEY'
      AND tc.table_name = _table_name
) pk ON c.column_name = pk.column_name
WHERE c.table_name = _table_name;

end
$func$;

alter function get_table_metadata owner to postgres;

select * from get_table_metadata('kt_subscription');



create function public.get_table_relationships(p_table_name text)
    returns TABLE(source_table text, 
                  target_table text, 
                  fk_column text, 
                  pk_column text, 
                  relationship_type text, 
                  junction_table text,
                  target_fk_column text,
                  target_pk_column text,
                  relationship_name text,
                  is_self_referencing boolean,
                  self_referencing_alias text)
    language sql
as
$$
WITH RECURSIVE fk_tree AS (
    -- Self-referencing relationships (non-recursive term)
    SELECT
        tc.table_name::text COLLATE "C" as source_table,
        ccu.table_name::text COLLATE "C" as target_table,
        kcu.column_name::text COLLATE "C" as fk_column,
        ccu.column_name::text COLLATE "C" as pk_column,
        CASE
            WHEN tc.table_name = ccu.table_name THEN 'SelfReferencing'
            ELSE 'OneToMany'
            END::text COLLATE "C" as relationship_type,
        NULL::text COLLATE "C" as junction_table,
        NULL::text COLLATE "C" as target_fk_column,
        NULL::text COLLATE "C" as target_pk_column,
        CASE
            WHEN tc.table_name = ccu.table_name THEN kcu.column_name || '_parent'
            ELSE tc.table_name || '_' || ccu.table_name
            END::text COLLATE "C" as relationship_name,
        (tc.table_name = ccu.table_name) as is_self_referencing,
        CASE
            WHEN tc.table_name = ccu.table_name THEN 'parent'::text COLLATE "C"
            ELSE NULL::text COLLATE "C"
            END as self_referencing_alias
    FROM information_schema.table_constraints tc
             JOIN information_schema.key_column_usage kcu
                  ON tc.constraint_name = kcu.constraint_name
             JOIN information_schema.constraint_column_usage ccu
                  ON ccu.constraint_name = tc.constraint_name
    WHERE tc.constraint_type = 'FOREIGN KEY'
      AND tc.table_name = p_table_name

    UNION ALL

    -- Many-to-Many relationships (recursive term)
    SELECT DISTINCT
        source.source_table,
        target_fk.target_table::text COLLATE "C",
        source.fk_column,
        source.pk_column,
        'ManyToMany'::text COLLATE "C",
        source.target_table,
        target_fk.fk_column::text COLLATE "C",
        target_pk.column_name::text COLLATE "C",
        (source.source_table || '_' || target_fk.target_table)::text COLLATE "C",
        false,
        NULL::text COLLATE "C"
    FROM fk_tree source
             JOIN (
        SELECT
            tc.table_name as source_table,
            ccu.table_name as target_table,
            kcu.column_name as fk_column,
            ccu.column_name as pk_column
        FROM information_schema.table_constraints tc
                 JOIN information_schema.key_column_usage kcu
                      ON tc.constraint_name = kcu.constraint_name
                 JOIN information_schema.constraint_column_usage ccu
                      ON ccu.constraint_name = tc.constraint_name
        WHERE tc.constraint_type = 'FOREIGN KEY'
    ) target_fk ON source.target_table = target_fk.source_table
             JOIN information_schema.columns target_pk
                  ON target_pk.table_name = target_fk.target_table
                      AND target_pk.is_nullable = 'NO'
                      AND target_pk.column_name = target_fk.pk_column
    WHERE source.relationship_type IN ('OneToMany', 'SelfReferencing')
)
SELECT DISTINCT *
FROM fk_tree;
$$;

alter function get_table_relationships(text) owner to postgres;


select * from get_table_relationships('kt_subscription');

select * from public.get_table_metadata('kt_subscription');




-- column_name   varchar   -- also visible as OUT param in function body
--         , data_type   varchar
--         , is_primary_key boolean
--         , is_nullable boolean)



SELECT    
    c.column_name as column_name,
    c.udt_name as data_type,
    CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END as is_primary_key,
    CASE WHEN c.is_nullable = 'YES' THEN true ELSE false END as is_nullable,
    CASE WHEN c.is_self_referencing = 'YES' THEN true ELSE false END as is_self_referencing,
    c.character_maximum_length,
    c.ordinal_position


FROM information_schema.columns c
         LEFT JOIN (
    SELECT ku.column_name
    FROM information_schema.table_constraints tc
             JOIN information_schema.key_column_usage ku
                  ON tc.constraint_name = ku.constraint_name
    WHERE tc.constraint_type = 'PRIMARY KEY'
      AND tc.table_name = _table_name
) pk ON c.column_name = pk.column_name
WHERE c.table_name = _table_name;

