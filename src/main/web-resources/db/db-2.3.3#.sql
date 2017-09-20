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
-- RESULT

/*****************************************************************************/