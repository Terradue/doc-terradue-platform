USE $MAIN$;

/*****************************************************************************/

-- CONFIG
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('jiraUsername', 'string', 'Jira Username', 'Jira Username', '', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('jiraPassword', 'string', 'Jira Password', 'Jira Password', '', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('jiraReleaseBaseUrl', 'string', 'Jira Release Base Url', 'Jira Release Base Url', 'https://projects.terradue.com/secure/ReleaseNote.jspa', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('discussCategoryId-ellipRelease', 'string', 'discuss Category for ellip release', 'discuss Category for ellip release', '', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('discussBaseUrl', 'string', 'discuss Base Url', 'discuss Base Url', 'http://discuss.terradue.com', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('discussApiKey', 'string', 'discuss Api key', 'discuss Api key', '', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('discussApiUsername', 'string', 'discuss Api username', 'discuss Api username', '', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-clientId', 'string', 'Coresyf Client Id', 'Enter the value of the client identifier for Coresyf', "@!17A5.AB35.0843.41D6!0001!14BA.D05E!0008!A4EB.D4B3.C87D.7C78", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-clientSecret', 'string', 'Coresyf Client Secret', 'Enter the value of the client secret password of Coresyf', "dGVycmFkdWU=", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-tokenEndpoint', 'string', 'Coresyf Token Endpoint url', 'Enter the value of the url of the Token Endpoint of Coresyf', "https://gluu.coresyf.eu/oxauth/restv1/token", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-userInfoEndpoint', 'string', 'Coresyf User Info Endpoint url', 'Enter the value of the url of the User Info Endpoint of Coresyf', "https://gluu.coresyf.eu/oxauth/restv1/userinfo", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-scopes', 'string', 'Coresyf default scopes', 'Enter the value of the default scopes of Coresyf', "openid,Co-ReSyF", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-callback', 'string', 'Coresyf callback url', 'Enter the value of the callback url of Coresyf', "https://www.terradue.com/t2api/coresyf/cb", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-authEndpoint', 'string', 'Coresyf Authentication Endpoint url', 'Enter the value of the url of the Authentication Endpoint of Coresyf', "https://gluu.coresyf.eu/oxauth/restv1/authorize", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-logoutEndpoint', 'string', 'Coresyf Logout Endpoint url', 'Enter the value of the url of the Logout Endpoint of Coresyf', 'https://gluu.coresyf.eu/oxauth/restv1/end_session', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-ldapDomain', 'string', 'Coresyf users Ldap domain', 'Coresyf users Ldap domain', 'coresyf.writer', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('coresyf-ldapDomainParent', 'string', 'Coresyf users Ldap domain parent', 'Coresyf users Ldap domain parent', 'coresyf', '0');
-- RESULT

-- auth
INSERT INTO auth (`identifier`, `name`, `description`, `type`, `enabled`, `activation_rule`, `normal_rule`, `refresh_period`) VALUES ('coresyf', 'Coresyf authentication', 'Coresyf authentication', 'Terradue.Corporate.Controller.CoresyfAuthenticationType, Terradue.Corporate.WebServer', '1', '2', '2', '0');
-- RESULT

/*****************************************************************************/