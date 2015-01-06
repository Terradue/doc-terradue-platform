-- VERSION 0.1

USE $MAIN$;

-- Initializing t2portal data model ... \ 

/*****************************************************************************/

UPDATE type SET custom_class = 'Terradue.Corporate.Controller.UserT2, Terradue.Corporate.WebServer' WHERE class = 'Terradue.Portal.User, Terradue.Portal';

/*****************************************************************************/

INSERT IGNORE INTO usrcert (id_usr) SELECT id from usr;

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
UPDATE config SET value='http://ldap.terradue.int:8095/crowd/rest' WHERE name='Crowd-api-url';
UPDATE config SET value='enguecrowd' WHERE name='Crowd-app-name';
UPDATE config SET value='enguecrowd' WHERE name='Crowd-app-pwd';

