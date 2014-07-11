-- VERSION 0.1

USE $MAIN$;

-- Initializing t2portal data model ... \ 

/*****************************************************************************/

-- Creating usr_github table
CREATE TABLE usr_github (
    id_usr int unsigned NOT NULL COMMENT 'FK: User',
    username varchar(50) COMMENT 'Username on github',
    CONSTRAINT pk_usrgithub PRIMARY KEY (id_usr),
    CONSTRAINT fk_usrgithub_usr FOREIGN KEY (id_usr) REFERENCES usr(id) ON DELETE CASCADE
) Engine=InnoDB COMMENT 'User github';
-- RESULT

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

INSERT IGNORE INTO usrcert (id_usr) SELECT id from usr;

/*****************************************************************************/

UPDATE type SET keyword='wps' WHERE class='Terradue.Portal.WpsProcessOffering, Terradue.Portal';

/*****************************************************************************/

INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-admin-usr', 'string', 'OpenNebula admin username', 'Enter the value of the Opennebula admin username', 'portal', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-admin-pwd', 'string', 'OpenNebula admin password', 'Enter the value of the Opennebula admin password', 'portaltest', '0');

/*****************************************************************************/

