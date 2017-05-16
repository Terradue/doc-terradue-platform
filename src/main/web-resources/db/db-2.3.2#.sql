USE $MAIN$;

/*****************************************************************************/

-- Adding usr links
ALTER TABLE usr
ADD COLUMN links TEXT NULL DEFAULT NULL;
-- RESULT

-- Add no plan role
INSERT INTO role (identifier, name, description) VALUES ('plan_NoPlan', 'No Plan', 'no plan user');
-- RESULT

/*****************************************************************************/