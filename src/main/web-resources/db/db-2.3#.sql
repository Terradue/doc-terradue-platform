USE $MAIN$;

/*****************************************************************************/

-- Adding EVEREST config... \
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('everest-clientId', 'string', 'Everest Client Id', 'Enter the value of the client identifier for Everest', "6Qvr7x2nAkFbzOntBJAoAo9gNmoa", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('everest-clientSecret', 'string', 'Everest Client Secret', 'Enter the value of the client secret password of Everest', "Osg2_eCwbifTlSRyUqimGmiXQkka", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('everest-tokenEndpoint', 'string', 'Everest Token Endpoint url', 'Enter the value of the url of the Token Endpoint of Everest', "https://sso.everest.psnc.pl/oauth2/token", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('everest-userInfoEndpoint', 'string', 'Everest User Info Endpoint url', 'Enter the value of the url of the User Info Endpoint of Everest', "https://sso.everest.psnc.pl/oauth2/userinfo?schema=openid", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('everest-scopes', 'string', 'Everest default scopes', 'Enter the value of the default scopes of Everest', "openid,profile", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('everest-callback', 'string', 'Everest callback url', 'Enter the value of the callback url of Everest', "http://127.0.0.1:8081/t2api/everest/cb", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('everest-authEndpoint', 'string', 'Everest Authentication Endpoint url', 'Enter the value of the url of the Authentication Endpoint of Everest', "https://sso.everest.psnc.pl/oauth2/authorize", '0');
-- RESULT

-- Adding SSO config... \
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-directAuthEndpoint', 'string', 'Terradue SSO Direct Authorization Endpoint url', 'Enter the value of the url of the Direct Authorization Endpoint of the Terradue SSO', "https://sso.terradue.com/c2id/direct-authz/rest/v2", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-sessionEndpoint', 'string', 'Terradue SSO Session Endpoint url', 'Enter the value of the url of the Session Endpoint of the Terradue SSO', "https://sso.terradue.com/c2id/session-store/rest/v2/sessions", '0');
-- RESULT

-- Adding Agent action...\
INSERT INTO action (`identifier`, `name`, `description`, `class`, `method`) VALUES ('cleanupCookie', 'Clean up cookies', 'This action clean the expired cookies stored in db', 'Terradue.Corporate.WebServer.Actions, Terradue.Corporate.WebServer', 'CleanDBCookies');
-- RESULT

-- Update Auth name...\
UPDATE auth SET `identifier`='ldap', `name`='Ldap authentication', `description`='Ldap authentication allows users to identify themselves using an external OAuth provider.', `type`='Terradue.Authentication.Ldap.LdapAuthenticationType, Terradue.Authentication.Ldap' WHERE `identifier`='oauth';
DELETE FROM auth WHERE identifier='oauth';
-- RESULT

-- Add Auth type
INSERT INTO auth (`identifier`, `name`, `description`, `type`, `enabled`, `activation_rule`, `normal_rule`, `refresh_period`) VALUES ('everest', 'Everest authentication', 'Everest authentication', 'Terradue.Corporate.Controller.EverestAuthenticationType, Terradue.Corporate.WebServer', '1', '2', '2', '0');
-- RESULT
/*****************************************************************************/

-- Create domain for existing users...\
INSERT IGNORE INTO domain (`name`, `description`) SELECT username, CONCAT('Domain of user ',username) FROM usr;
-- RESULT

-- Add Owner role ... \
INSERT INTO role (identifier, name, description) VALUES ('owner', 'owner', 'Default role for every user to be able to use his own domain');
SET @role_id = (SELECT LAST_INSERT_ID());

-- Assign owner role to existing users...\
SET @role_id = (SELECT id FROM role WHERE identifier='owner');
INSERT IGNORE INTO rolegrant (id_usr,id_role,id_domain) SELECT u.id,@role_id,d.id FROM usr as u LEFT JOIN domain AS d ON u.username=d.name;
-- RESULT

-- Add EVEREST domains...\
SET @role_id = (SELECT id FROM role WHERE identifier='starter');

-- Add domains...\
UPDATE domain set kind=1;
INSERT INTO domain (`name`, `description`, `kind`) VALUES ('everest-CNR', 'Domain of Thematic Group CNR for Everest',3);
INSERT INTO domain (`name`, `description`, `kind`) VALUES ('everest-INGV', 'Domain of Thematic Group INGV for Everest',3);
INSERT INTO domain (`name`, `description`, `kind`) VALUES ('everest-NERC', 'Domain of Thematic Group NERC for Everest',3);
INSERT INTO domain (`name`, `description`, `kind`) VALUES ('everest-SatCen', 'Domain of Thematic Group SatCen for Everest',3);
INSERT INTO domain (`name`, `description`, `kind`) VALUES ('everest-Citizens', 'Domain of Thematic Group Citizens for Everest',3);
INSERT INTO domain (`name`, `description`, `kind`) VALUES ('terradue', 'Domain of Terradue',4);
-- RESULT

