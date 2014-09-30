-- VERSION 0.1

USE $MAIN$;

-- Initializing t2portal data model ... \ 

-- Adding extended entity types for dataseries... \
CALL add_type($ID$, 'Terradue.Corporate.Controller.DataSeries, Terradue.Corporate.Controller', 'Terradue.Portal.Series, Terradue.Portal', 't2portal data series', 't2portal data series', NULL);
-- RESULT

/*****************************************************************************/
-- Adding entity base type for data packages ... \
CALL add_type($ID$, 'Terradue.Corporate.Controller.DataPackage, Terradue.Corporate.Controller', NULL, 'Data Package', 'Data Packages', 'datapackages');
SET @type_id = (SELECT LAST_INSERT_ID());
-- RESULT

-- Adding privileges for data packages ... \
SET @priv_pos = (SELECT MAX(pos) FROM priv);
INSERT INTO priv (id_type, operation, pos, name) VALUES
    (@type_id, 'v', @priv_pos + 1, 'DataPackage: view'),
    (@type_id, 'c', @priv_pos + 2, 'DataPackage: create'),
    (@type_id, 'm', @priv_pos + 3, 'DataPackage: change'),
    (@type_id, 'M', @priv_pos + 4, 'DataPackage: control'),
    (@type_id, 'd', @priv_pos + 5, 'DataPackage: delete');
-- RESULT

/*****************************************************************************/

UPDATE type SET custom_class = 'Terradue.Corporate.Controller.UserT2, Terradue.Corporate.Controller' WHERE class = 'Terradue.Portal.User, Terradue.Portal';

/*****************************************************************************/

INSERT IGNORE INTO usrcert (id_usr) SELECT id from usr;

/*****************************************************************************/

UPDATE type SET keyword='wps' WHERE class='Terradue.Portal.WpsProcessOffering, Terradue.Portal';

/*****************************************************************************/

INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-xmlrpc-url', 'string', 'OpenNebula XML RPC proxy url', 'Enter the value of the Opennebula XML RPC proxy server url', 'http://cloud-dev.terradue.int:2633/RPC2', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-admin-usr', 'string', 'OpenNebula admin username', 'Enter the value of the Opennebula admin username', 'serveradmin', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-admin-pwd', 'string', 'OpenNebula admin password', 'Enter the value of the Opennebula admin password', 'f4b887a18de059129df8a265176f80bc479439a6', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-access', 'string', 'OpenNebula access url', 'Enter the value of the Opennebula access url', 'https://cloud-dev.terradue.int', '0');

UPDATE config SET value='terradue.com' WHERE name='Github-client-name';
UPDATE config SET value='64e9f7050a5dba093679' WHERE name='Github-client-id';
UPDATE config SET value='3c84f555347681b0e0635ffbcbfb3cbbb3504b5e' WHERE name='Github-client-secret';
UPDATE config SET value='5dmloJh2jq9ldxN9nFZdA477Kb4XrwtZQR3hLsjl0eW2HlsS0N' WHERE name='Tumblr-apiKey';
UPDATE config SET value='zURZoLKwL1kW8l1v7ujkxDF8A' WHERE name='Twitter-consumerKey';
UPDATE config SET value='6KtHzvZRQlp2JBZ8Y2fL8dzKl39APmQllHUBtKNjTDBloamTEN' WHERE name='Twitter-consumerSecret';
UPDATE config SET value='1196754241-iuj7ZgIqZwk2YpsrWC9fLnmnUH6CjA4f5M9i6hI' WHERE name='Twitter-token';
UPDATE config SET value='aB92cfpkONwXOToA04ykA1Dnd6zP2Ui67y2CbkLI9mQ3R' WHERE name='Twitter-tokenSecret';
UPDATE config SET value='https://ca.terradue.com/gpodcs/cgi/certreq.cgi' WHERE name='CertificateRequestUrl';
UPDATE config SET value='https://ca.terradue.com/gpodcs/cgi/certdown.cgi' WHERE name='CertificateDownloadUrl';





