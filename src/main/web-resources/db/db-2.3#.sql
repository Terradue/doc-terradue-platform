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

/*****************************************************************************/