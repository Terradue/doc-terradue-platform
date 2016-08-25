USE $MAIN$;
/*****************************************************************************/

-- CONFIG
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('catalogue-BaseUrl', 'string', 'Catalogue Base Url', 'Enter the value of the Catalogue Base Url', 'https://data.terradue.com/catalogue', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('geoserver-BaseUrl', 'string', 'Geoserver Base Url', 'Enter the value of the Geoserver Base Url', 'https://geo.terradue.com/rest', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('geoserver-admin-usr', 'string', 'Geoserver Admin username', 'Enter the value of the Geoserver Admin username', '', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('geoserver-admin-pwd', 'string', 'Geoserver Admin password', 'Enter the value of the Geoserver Admin password', '', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('artifactory-APIurl', 'string', 'Artifactory API Url', 'Enter the value of the Artifactory API Url', 'https://store.terradue.com/api', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('artifactory-APIkey', 'string', 'Artifactory API Key', 'Enter the value of the Artifactory API Key', 'AKCp2V5pLBiabTT8RoSpP6gbsZFGFGNc2PoL6LeWZf2gyDMsqD8nuqcRaeNe7Cpco2hepyxte', '0');
-- RESULT