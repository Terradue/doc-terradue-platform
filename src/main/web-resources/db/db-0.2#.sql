USE $MAIN$;

/*****************************************************************************/

-- Adding LDAP config... \
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('ldap-authEndpoint', 'string', 'LDAP authentication endpoint', 'Enter the value of the LDAP authentication endpoint', 'https://sso.terradue.com/ldapauth/', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('ldap-apikey', 'string', 'LDAP API Key', 'Enter the value of the LDAP API key', 'f70defbeb88141f88138bea52b6e1b9c', '0');
-- RESULT

-- Adding SSO config... \
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-clientId', 'string', 'Terradue SSO Client Id', 'Enter the value of the client identifier of the Terradue SSO', "", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-clientSecret', 'string', 'Terradue SSO Client Secret', 'Enter the value of the client secret password of the Terradue SSO', "", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-authorizationEndpoint', 'string', 'Terradue SSO Authorization Endpoint url', 'Enter the value of the url of the Authorization Endpoint of the Terradue SSO', "https://www.terradue.com/t2api/oauth", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-tokenEndpoint', 'string', 'Terradue SSO Token Endpoint url', 'Enter the value of the url of the Token Endpoint of the Terradue SSO', "https://sso.terradue.com/c2id/token", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-userInfoEndpoint', 'string', 'Terradue SSO User Info Endpoint url', 'Enter the value of the url of the User Info Endpoint of the Terradue SSO', "https://sso.terradue.com/c2id/userinfo", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-scopes', 'string', 'Terradue SSO default scopes', 'Enter the value of the default scopes of the Terradue SSO', "openid", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-callback', 'string', 'Terradue SSO callback url', 'Enter the value of the callback url of the Terradue SSO', "https://www.terradue.com/t2api/oauth/cb", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-authEndpoint', 'string', 'Terradue SSO Authentication Endpoint url', 'Enter the value of the url of the Authentication Endpoint of the Terradue SSO', "https://sso.terradue.com/c2id/authz-sessions/rest/v1", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-loginEndpoint', 'string', 'Terradue SSO Login Endpoint url', 'Enter the value of the url of the Login Endpoint of the Terradue SSO', "https://www.terradue.com/login/consent", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-consentEndpoint', 'string', 'Terradue SSO Consent Endpoint url', 'Enter the value of the url of the Consent Endpoint of the Terradue SSO', "https://www.terradue.com/login", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-apiAccessToken', 'string', 'Terradue SSO API Access Token', 'Enter the value of the API Access token of the Terradue SSO', "ztucZS1ZyFKgh0tUEruUtiSTXhnexmd6", '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-configUrl', 'string', 'Terradue SSO Configuration url', 'Enter the value of the url of the Configuration of the Terradue SSO', "https://sso.terradue.com/c2id//.well-known/openid-configuration", '0');
-- RESULT

-- Update config..\
UPDATE config SET value='Dear $(USERNAME), You have made a password reset request for the $(PORTAL).\\nPlease go to this link to set your new password:\n$(LINK)' WHERE name='EmailSupportResetPasswordBody';
-- RESULT

/*****************************************************************************/