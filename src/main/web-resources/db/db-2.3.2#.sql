USE $MAIN$;

/*****************************************************************************/

-- Adding usr links
ALTER TABLE usr
ADD COLUMN links TEXT NULL DEFAULT NULL;
-- RESULT

/*****************************************************************************/