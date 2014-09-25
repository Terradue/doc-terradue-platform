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

INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-xmlrpc-url', 'string', 'OpenNebula XML RPC proxy url', 'Enter the value of the Opennebula XML RPC proxy server url', 'http://cloud-dev.terradue.int:2633/RPC2', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-admin-usr', 'string', 'OpenNebula admin username', 'Enter the value of the Opennebula admin username', 'serveradmin', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-admin-pwd', 'string', 'OpenNebula admin password', 'Enter the value of the Opennebula admin password', 'f4b887a18de059129df8a265176f80bc479439a6', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-access', 'string', 'OpenNebula access url', 'Enter the value of the Opennebula access url', 'https://cloud-dev.terradue.int', '0');
