USE $MAIN$;

/*****************************************************************************/

-- Adding discourse SSO ... \
INSERT INTO config (name, id_section, pos, internal, type, caption, hint, value, optional) VALUES ('discourse-sso-secret', NULL, NULL, '0', 'string', 'discourse sso secret', 'discourse sso secret', 'T3rr4du3Discours3', '1');
INSERT INTO config (name, id_section, pos, internal, type, caption, hint, value, optional) VALUES ('discourse-sso-callback', NULL, NULL, '0', 'string', 'discourse sso callback', 'discourse sso callback', 'http://discuss.terradue.com/session/sso_login', '1');

-- RESULT

/*****************************************************************************/
