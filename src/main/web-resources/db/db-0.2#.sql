USE $MAIN$;

/*****************************************************************************/

-- Adding SSO config... \
UPDATE config SET value='' WHERE name='sso-clientId';
UPDATE config SET value='' WHERE name='sso-clientSecret';
UPDATE config SET value='https://sso.terradue.com/c2id-login-page-js' WHERE name='sso-authorizationEndpoint';
UPDATE config SET value='https://sso.terradue.com/c2id/token' WHERE name='sso-tokenEndpoint';
UPDATE config SET value='https://sso.terradue.com/c2id/userinfo' WHERE name='sso-userInfoEndpoint';
UPDATE config SET value='openid,email,profile' WHERE name='sso-scopes';
UPDATE config SET value='' WHERE name='sso-callback';
-- RESULT

/*****************************************************************************/