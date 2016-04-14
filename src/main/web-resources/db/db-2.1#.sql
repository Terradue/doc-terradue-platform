USE $MAIN$;

/*****************************************************************************/

-- Adding discourse SSO ... \
INSERT INTO config (name, id_section, pos, internal, type, caption, hint, value, optional) VALUES ('discourse-sso-secret', NULL, NULL, '0', 'string', 'discourse sso secret', 'discourse sso secret', 'T3rr4du3Discours3', '1');
INSERT INTO config (name, id_section, pos, internal, type, caption, hint, value, optional) VALUES ('discourse-sso-callback', NULL, NULL, '0', 'string', 'discourse sso callback', 'discourse sso callback', 'https://discuss.terradue.com/session/sso_login', '1');
-- RESULT

-- Adding zendesk SSO ... \
INSERT INTO config (name, id_section, pos, internal, type, caption, hint, value, optional) VALUES ('zendesk-sso-secret', NULL, NULL, '0', 'string', 'zendesk sso secret', 'zendesk sso secret', '5Gjz1rGjFlPRcqrFXUAOfCA9kRJamuoK0xvu745eNuIl9ItJ', '1');
INSERT INTO config (name, id_section, pos, internal, type, caption, hint, value, optional) VALUES ('zendesk-sso-callback', NULL, NULL, '0', 'string', 'zendesk sso callback', 'zendesk sso callback', 'https://terradue.zendesk.com/access/jwt', '1');
-- RESULT

/*****************************************************************************/
