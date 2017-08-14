USE $MAIN$;

/*****************************************************************************/

-- Adding usr links
ALTER TABLE usr
ADD COLUMN links TEXT NULL DEFAULT NULL;
-- RESULT

-- Add no plan role
INSERT INTO role (identifier, name, description) VALUES ('plan_NoPlan', 'No Plan', 'no plan user');
-- RESULT

-- CONFIG
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-eosso-secret', 'string', 'Eosso secret password', 'Eosso secret password', '7q1fdfb2-ee27-443c-bb00-acc0e6a863s1', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('sso-eosso-endpoint', 'string', 'Eosso secret Endpoint', 'Eosso secret Endpoint', 'https://geohazards-tep.eo.esa.int/t2api/t2sso', '0');
-- RESULT

-- auth
INSERT INTO auth (`identifier`, `name`, `description`, `type`, `enabled`, `activation_rule`, `normal_rule`, `refresh_period`) VALUES ('eosso', 'Eosso authentication', 'Eosso authentication', 'Terradue.Corporate.Controller.EossoAuthenticationType, Terradue.Corporate.WebServer', '1', '2', '2', '0');
-- RESULT

/*****************************************************************************/