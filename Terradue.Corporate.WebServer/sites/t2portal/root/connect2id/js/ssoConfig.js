// The Connect2id server authorisation session endpoint details
var authzSession = {};
authzSession.baseURL = "https://sso.terradue.com/c2id/authz-sessions/rest/v1";
authzSession.accessToken = "ztucZS1ZyFKgh0tUEruUtiSTXhnexmd6";
authzSession.id; // Will store the authorisation session ID


// The Connect2id server subject session endpoint details
var subjectSession = {};
subjectSession.baseURL = "https://sso.terradue.com/c2id/session-store/rest/v1";
subjectSession.accessToken = "ztucZS1ZyFKgh0tUEruUtiSTXhnexmd6";
subjectSession.id; // Will store the session ID from the session cookie


// The LdapAuth service details for verifying the username and password against
// an LDAP directory
var ldapAuth = {};
ldapAuth.url = "https://sso.terradue.com/ldapauth/";
ldapAuth.apiKey = "f70defbeb88141f88138bea52b6e1b9c";


// The subject (user) session ID and details
var subject = {};
subject.id;    // Will store the subject ID
subject.name;  // Will store the subject's name
subject.email; // Will store the subject's email address
