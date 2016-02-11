
define({
	
	// The Connect2id server authorisation session endpoint details
	authzSession: {
		baseURL: "https://sso.terradue.com/c2id/authz-sessions/rest/v1",
		accessToken: "ztucZS1ZyFKgh0tUEruUtiSTXhnexmd6",
		id: null // Will store the authorisation session ID
	},
	
	// The Connect2id server subject session endpoint details
	subjectSession: {
		baseURL: "https://sso.terradue.com/c2id/session-store/rest/v1",
		accessToken: "ztucZS1ZyFKgh0tUEruUtiSTXhnexmd6",
		id: null // Will store the session ID from the session cookie
	},

	// The LdapAuth service details for verifying the username and password against
	// an LDAP directory
	ldapAuth: {
		url: "https://sso.terradue.com/ldapauth/",
		apiKey: "f70defbeb88141f88138bea52b6e1b9c"
	},
	
	// The subject (user) session ID and details
	subject:{
		id: null,		// Will store the subject ID
		name: null,		// Will store the subject's name
		email: null,	// Will store the subject's email address
	}
	
});