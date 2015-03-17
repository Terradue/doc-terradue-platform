-- VERSION 0.1

USE $MAIN$;

-- Initializing t2portal data model ... \ 

/*****************************************************************************/

UPDATE type SET custom_class = 'Terradue.Corporate.Controller.UserT2, Terradue.Corporate.WebServer' WHERE class = 'Terradue.Portal.User, Terradue.Portal';

/*****************************************************************************/

INSERT IGNORE INTO usrcert (id_usr) SELECT id from usr;

/*****************************************************************************/

SET @type_id = (SELECT id FROM type WHERE class = 'Terradue.Cloud.OneCloudProvider, Terradue.Cloud');
INSERT INTO cloudprov (id_type, caption, address, web_admin_url) VALUES (@type_id, 'Terradue ONE server', 'http://cloud.terradue.int:2633/RPC2', 'http://cloud.terradue.int:2633/RPC2');
INSERT INTO onecloudprov (id, admin_usr, admin_pwd) VALUES (@@IDENTITY, 'serveradmin', '71f1fc3805e49031fb534606efcc8fc1eefa7d69');
SET @prov_id = (SELECT LAST_INSERT_ID());

/*****************************************************************************/

INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-default-provider', 'int', 'OpenNebula default provider', 'Enter the value of the identifier of the Opennebula default provider', @prov_id, '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-access', 'string', 'OpenNebula access url', 'Enter the value of the Opennebula access url', 'https://cloud-dev.terradue.int', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('One-GEP-grpID', 'int', 'Id of GEP group on ONE controller', 'Enter the Id of GEP group on ONE controller', '141', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('reCaptcha-secret', 'string', 'Google reCaptcha secret', 'Enter the name of the Google reCaptcha secret', '6Lc1ZgMTAAAAAIeEknASbDZ2Kn0N20Br-7a_jIAk', '0');

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
UPDATE config SET value='Terradue Support' WHERE name='MailSender';
UPDATE config SET value='support@terradue.com' WHERE name='MailSenderAddress';
UPDATE config SET value='relay.terradue.int' WHERE name='SmtpHostname';
UPDATE config SET value='Dear $(USERNAME)\n\nYour account has been created on $(SITEURL). We must verify your email authenticity. To do so, please click on the following link: $(ACTIVATIONURL)\nThank you.\n\nRegards\n\nTerradue Support Team' WHERE name='RegistrationMailBody';
UPDATE config SET value='$(BASEURL)/#!emailconfirm?token=$(TOKEN)' WHERE name='EmailConfirmationUrl';


/*****************************************************************************/

UPDATE auth SET `activation_rule`='2' WHERE `identifier`='umsso';

/*****************************************************************************/

-- Create roles ... \
INSERT into role (name, description) VALUES ('free', 'non paying user role');
INSERT into role (name, description) VALUES ('developer', 'paying developer user role');
INSERT into role (name, description) VALUES ('integrator', 'paying integrator user role');
INSERT into role (name, description) VALUES ('producer', 'paying producer user role');
-- RESULT

/*****************************************************************************/

-- Create roles privileges ... \
-- INSERT INTO role_priv (id_role, id_priv) SELECT role.id, priv.id FROM role INNER JOIN priv WHERE role.name ='integrator' AND priv.name IN ('Service: create');
-- RESULT