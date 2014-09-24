-- VERSION 0.1

USE $MAIN$;

-- Initializing t2portal data model ... \ 

/*****************************************************************************/

-- Adding privileges for data packages ... \
SET @priv_pos = (SELECT MAX(pos) FROM priv);
INSERT INTO priv (id_type, operation, pos, name) VALUES
    (@type_id, 'v', @priv_pos + 1, 'resourceset: view'),
    (@type_id, 'c', @priv_pos + 2, 'resourceset: create'),
    (@type_id, 'm', @priv_pos + 3, 'resourceset: change'),
    (@type_id, 'M', @priv_pos + 4, 'resourceset: control'),
    (@type_id, 'd', @priv_pos + 5, 'resourceset: delete');
-- RESULT

/*****************************************************************************/

UPDATE type SET custom_class = 'Terradue.Corporate.Controller.UserT2, Terradue.Corporate.Controller' WHERE class = 'Terradue.Portal.User, Terradue.Portal';

/*****************************************************************************/

UPDATE type SET keyword='wps' WHERE class='Terradue.Portal.WpsProcessOffering, Terradue.Portal';

/*****************************************************************************/
