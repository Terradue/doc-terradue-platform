

function loginFormInteractions(){
	var $username = $('#login-username');
	var $password = $('#login-password');
	var $submit = $('#login-button');
	
	var updateSubmitStatus = function(){
		showLoginError(''); // remove login error
		$submit.prop('disabled', ($username.val()=='' || $password.val()==''));
	};
	
	$username.keyup(updateSubmitStatus);
	$password.keyup(updateSubmitStatus);

}

function showLoginError(text){
	$('#loginError').text(text);
}

var firstTime=true;

//Initialises the OpenID Connect login page. Fetches the session cookie and
//the URL query string containing the encoded OpenID Connect request
$(document).ready(init);

function init() {
	
	if (firstTime){
		cycleBackground();
		initDebuggerShortcut();
		loginFormInteractions();
		firstTime=false;
	}

	clearUI();
	resetGlobalVars();
	attachEventHandlers();

	// Get the query string with the encoded OpenID Connect authentication
	// request
	var queryString = location.search;
	log("Query string: " + (queryString ? queryString : "none"));

	if (! queryString) {
		var msg = "Invalid OpenID Connect authentication request: Missing query string";
		log(msg);
		displayErrorMessage({"error_description":msg});
		return;
	}

	// Get the session cookie
	subjectSession.id = $.cookie("sid");
	log("Found session cookie: " + (subjectSession.id ? subjectSession.id : "none"));

	// Begins a Connect2id authorisation session by sending the OpenID Connect
	// query string for decoding along with the session cookie value is found
	var request = {};
	request.query = queryString;

	if (subjectSession.id) {
		request.sub_sid = subjectSession.id;
	}

	var requestTxt = JSON.stringify(request);

	log("Making authorization session begin request to the Connect2id server: " + requestTxt);

	$.ajax(authzSession.baseURL + "?ajax=true",{
		type : "POST",
		headers : {"Authorization" : "Bearer " + authzSession.accessToken},
		contentType : "application/json;charset=UTF-8",
		data : requestTxt,
		dataType : "json",
		success : switchForm,
		error : handleAuthzSessionError,
		statusCode : { 204 : redirectWithAuthzResponse }
	});
}


//Clears the UI
function clearUI() {

    $("#subject-name").hide();
    $("#subject-name").html("");

    $("#subject-email").hide();
    $("#subject-email").html("");

    $("#logout-button").hide();

    $(".loginStuff").hide();
    $("#login-username").val("");
    $("#login-password").val("");

    $(".consentStuff").hide();
    $("#client-metadata td").html("not specified");
    $("#authz-details td").html("none");

    $('#loginError').empty();
    $("#errorMessage").hide();

}


//Displays an error message returned by the Connect2id server
function displayErrorMessage(form, isModal) {

    if (form.error) {
        $(".error-code").text("[ " + form.error + " ] ").html();
    }

    if (form.error_description) {
        $(".error-description").text(form.error_description).html();
    } else {
        $(".error-description").text("Invalid OpenID Connect request").html();
    }
    
    if (isModal)
    	$('#errorMessageModal').modal();
    else
    	$('#errorMessage').show();
}

//Resets the global vars
function resetGlobalVars() {

    authzSession.id = null;
    subjectSession.id = null;
    subject = {};
}


//Attach event handlers
function attachEventHandlers() {
    $("#login-button").off().click(authenticateSubject);
    $("#logout-button").off().click(logoutSubject);
    $("#consent-button").off().click(submitConsent);
    $("#deny-button").off().click(denyAuthorization);
}


//The user denies the authorisation request
function denyAuthorization() {

	log("Authorization denied, submitting request to Connect2id server...");

	$.ajax(authzSession.baseURL + "/" + authzSession.id + "?ajax=true",{
		type:"DELETE",
		headers : {"Authorization" : "Bearer " + authzSession.accessToken},
		error : handleAuthzSessionError,
		statusCode : { 204 : redirectWithAuthzResponse }
	});
}


//Submits the subject consent to the Connect2id server to obtain the final
//OpenID Connect authentication response and redirect back to the client
function submitConsent() {

	var consentedScopeValues = [];
	$(".scope-value:checked").each(function() {
		consentedScopeValues.push(this.value);
	});

	log("Consented scope values: " + consentedScopeValues);
	var consentedClaims = [];

	$(".claim:checked").each(function() {
		consentedClaims.push(this.value);
	});

	log("Consented claims: " + consentedClaims);

	// Compose the final authorisation from the consent form and other details
	var authz = {};
	authz.scope = consentedScopeValues; // May add additional scope values here
	authz.claims = consentedClaims; // May add additional claims here
	authz.preset_claims = {}; // Optional preset claims to return with ID token
	authz.preset_claims.id_token = {};
	authz.preset_claims.id_token.ip_address = "10.20.30.40";
	authz.preset_claims.userinfo = {}; // Optional preset claims to return with UserInfo
	authz.preset_claims.userinfo.groups = ["admin", "audit"];
	authz.audience = []; // Optional custom audience values
	authz.long_lived = true;
	authz.issue_refresh_token = true;
	authz.access_token = {};
	authz.access_token.encoding = "SELF_CONTAINED"; // IDENTIFIER or SELF_CONTAINED
	authz.access_token.lifetime = 600; // 10 minutes

	var requestTxt = JSON.stringify(authz);

	log("Submitting authorization to Connect2id server: " + requestTxt);

	$.ajax(authzSession.baseURL + "/" + authzSession.id + "?ajax=true",{
		type : "PUT",
		headers : {"Authorization" : "Bearer " + authzSession.accessToken},
		contentType : "application/json;charset=UTF-8",
		data : requestTxt,
		dataType : "json",
		error : handleAuthzSessionError,
		statusCode : { 204 : redirectWithAuthzResponse }
	});
}


//Authenticates the subject by checking the entered username and password with
//the LdapAuth JSON-RPC 2.0 service
function authenticateSubject() {

	var username = $("#login-username").val();
	var password = $("#login-password").val();
	
	log("Entered username: " + username);
	log("Entered password: " + password);
	
	if (username.length === 0 || password.length === 0) {
		showLoginError('You must enter a username and a password');
		$("#login-username").focus();
		return false;
	}
	
	// Clear login form
	$("#login-username").val("");
	$("#login-password").val("");

	// Make a user.auth request to the LdapAuth JSON-RPC 2.0 service
	var request = {};
	request.method = "user.authGet";
	request.params = {};
	request.params.username = username;
	request.params.password = password;

	if (ldapAuth.apiKey)
		request.params.apiKey = ldapAuth.apiKey;

	request.id = 1;
	request.jsonrpc = "2.0";

	var requestTxt = JSON.stringify(request);

	log("Making " + request.method + " request to the LdapAuth service: " + requestTxt);

	$.ajax(ldapAuth.url, {
		type : "POST",
		contentType : "application/json;charset=UTF-8",
		data : requestTxt,
		dataType : "json",
		success : handleLdapAuthResult,
		error : handleLdapAuthError,
		accepts: 'application/json'
	});
	
	return false; // avoid submit
}


//Logs out the user and returns to the login screen
function logoutSubject() {
	log("Submitting subject logout request to Connect2id server");
	$.ajax(subjectSession.baseURL + "/" + subjectSession.id,{
		type : "DELETE",
		headers : { "Authorization" : "Bearer " + subjectSession.accessToken},
		success : init,
		error : handleLogoutError
	});
}


//Handles HTTP error responses from the Connect2id session store
function handleLogoutError(jqXHR, textStatus, errorThrown) {

    var msg = "Logout error: " + textStatus + ": " + JSON.stringify(jqXHR);
    log(msg);
    displayErrorMessage({"error_description":msg});
}


//Handles the subject authentication result from the LdapAuth service,
//requests the Connect2id consent form on success
function handleLdapAuthResult(response) {

	if (response.error) {
		// Authentication failed
		log("Authentication failed: " + JSON.stringify(response));
		showLoginError(response.error.message);
		return;
	}

	log("Subject successfully authenticated: " + JSON.stringify(response));

	subject.id = response.result.attributes.userID;
	log("Subject ID: " + subject.id);

	subject.name = response.result.attributes.name;
	log("Subject name: " + subject.name);
	$("#subject-name").text(subject.name).show();

	subject.email = response.result.attributes.email[0];
	log("Subject email: " + subject.email);
	$("#subject-email").text(subject.email).show();

	// Hide the login form
	$(".loginStuff").hide();
	
	// Submit the subject authentication to the Connect2id server to create a
	// new subject session and receive the consent form
	var request = {};
	request.sub = subject.id;
	request.acr = "1"; // Optional Authentication Context Class Reference (ACR)
	request.amr = [ "ldap" ]; // Optional Authentication Method References (AMR)
	request.data = {}; // Store optional session data about the subject
	request.data.name = subject.name;
	request.data.email = subject.email;
	
	 var requestTxt = JSON.stringify(request);
	
	 log("Submitting subject authentication to Connect2id server: " + requestTxt);
	
	 $.ajax(authzSession.baseURL + "/" + authzSession.id + "?ajax=true", {
		type : "PUT",
		headers : {"Authorization" : "Bearer " + authzSession.accessToken},
		contentType : "application/json;charset=UTF-8",
		data : requestTxt,
		dataType : "json",
		success : switchForm,
		error : handleAuthzSessionError,
		statusCode : { 204 : redirectWithAuthzResponse }
	});
}


//Handles HTTP error responses from the LdapAuth service
function handleLdapAuthError(jqXHR, textStatus, errorThrown) {

	var msg = "LdapAuth error: " + textStatus + ": " + JSON.stringify(jqXHR);
	log(msg);
	displayErrorMessage({"error_description":msg});
}


//Handles Connect2id authorisation session error responses
function handleAuthzSessionError(jqXHR, textStatus, errorThrown) {

    var msg = "Authorization session error: " + textStatus + ": " + JSON.stringify(jqXHR);
	log(msg);
	displayErrorMessage({"error_description":msg});
}


//Redirects back to the client with the authorisation response encoded in the
//query string / fragment
function redirectWithAuthzResponse(data, textStatus, jqXHR) {

	var location = jqXHR.getResponseHeader("Location");

	if (! location) {
		log("Missing Location response header");
		return;
	}

	log("Received redirection URI: " + location);

	window.location.replace(location);
}


//Handles the Connect2id authorisation session response by presenting a login
//form or a consent form depending on whether the subject has a valid session
function switchForm(form) {

	if (form == null) {
		// Work around unintended callback on 204 status code redirection
		return;
	}

	if (form.error) {
		// An error message is received, no redirection is performed
		displayErrorMessage(form);
		return;
	}

	// Save the authorisation session ID for later use
	authzSession.id = form.sid;

	log("Started Connect2id authorisation session with ID " + authzSession.id);

	log("Received " + form.type + " form: " + JSON.stringify(form));

	if (form.display == "popup") {
		sizeWindow();
	}

	if (form.type == "auth") {
		// No subject session is present, display login form first
		displayLoginForm(form);
	} else if (form.type == "consent") {
		// Subject session is present, jump to consent form
		displayConsentForm(form);
	} else {
		// An error message is assumed
		bootbox.alert("Unexpected form type: " + form.type);
	}
}


//Displays the login form the get the subject's username and password
function displayLoginForm(form) {
    log("Displaying login form...");
    $(".loginStuff").fadeIn();
    $("#login-username").focus();
}


//Displays the consent form to get the subject's scope and claims consent
function displayConsentForm(form) {

    // Set/update subject session cookie
    subjectSession.id = form.sub_session.sid;
    $.cookie("sid", subjectSession.id);
    log("Set session cookie: " + subjectSession.id);

    $("#logout-button").show();

    if (form.sub_session.data) {
    
        $("#subject-name").text(form.sub_session.data.name).show();
        $("#subject-email").text(form.sub_session.data.email).show();
    }

    log("Displaying consent form...");
    $(".consentStuff").fadeIn();

    // Display the OpenID Connect client details
    $("#client-name").text(form.client.name).html();
    $("#client-id").text(form.client.client_id).html();
    $("#client-uri").text(form.client.uri).html();
    $("#client-logo-uri").text(form.client.logo_uri).html();
    $("#client-policy-uri").text(form.client.policy_uri).html();
    $("#client-tos-uri").text(form.client.tos_uri).html();

    // Display the requested scope values
    if (form["scope"]["new"].length > 0) {

        $("#new-scope-values").html("");

        form["scope"]["new"].forEach(function(value){

            var disabled = (value=="openid")? "disabled" : "";
            var checked = (value=="openid")? "checked=\"true\"" : "";

            var el = "<input class=\"scope-value\" type=\"checkbox\" " +
                checked + " " +
                disabled +
                " value=\"" + value + "\"/>" +
                "&nbsp;" + value + " ";

            $("#new-scope-values").append(el);
        });
    }

    if (form["scope"]["consented"].length > 0) {

        $("#consented-scope-values").html("");

        form["scope"]["consented"].forEach(function(value){

            var disabled = (value=="openid")? "disabled" : "";

            var el = "<input class=\"scope-value\" type=\"checkbox\" " +
                "checked=\"true\" " +
                disabled +
                " value=\"" + value + "\"/>" +
                "&nbsp;" + value + " ";

             $("#consented-scope-values").append(el);
        });
    }


    // Display the requested claims (resolved from the scope values or explicit)
    if (form["claims"]["new"]["essential"].length > 0) {

        $("#new-essential-claims").html("");

        form["claims"]["new"]["essential"].forEach(function(claim){

            var el = "<input class=\"claim\" type=\"checkbox\" value=\"" + claim + "\"/>" +
                "&nbsp;" + claim + " ";

            $("#new-essential-claims").append(el);
        });
    }

    if (form["claims"]["new"]["voluntary"].length > 0) {

        $("#new-voluntary-claims").html("");

        form["claims"]["new"]["voluntary"].forEach(function(claim){

            var el = "<input class=\"claim\" type=\"checkbox\" value=\"" + claim + "\"/>" +
                "&nbsp;" + claim + " ";

            $("#new-voluntary-claims").append(el);
        });
    }

    // Display the requested claims (resolved from the scope values or explicit)
    if (form["claims"]["consented"]["essential"].length > 0) {

        $("#new-essential-claims").html("");

        form["claims"]["consented"]["essential"].forEach(function(claim){

            var el = "<input class=\"claim\" type=\"checkbox\" checked=\"true\" value=\"" + claim + "\"/>" +
                "&nbsp;" + claim + " ";

            $("#new-essential-claims").append(el);
        });
    }

    if (form["claims"]["consented"]["voluntary"].length > 0) {

        $("#consented-voluntary-claims").html("");

        form["claims"]["consented"]["voluntary"].forEach(function(claim){

            var el = "<input class=\"claim\" type=\"checkbox\" checked=\"true\" value=\"" + claim + "\"/>" +
                "&nbsp;" + claim + " ";

            $("#consented-voluntary-claims").append(el);
        });
    }
}


function sizeWindow() {

    var targetWidth = 800;
    var targetHeight = 600;

    var currentWidth = $(window).width();
    var currentHeight = $(window).height();

    window.resizeBy(targetWidth - currentWidth, targetHeight - currentHeight);

    $("body").css("overflow-y", "scroll");
}


//Logs a message to the console text area
function log(msg) {
	var txt = $("#console textarea");
	txt.val( txt.val() + msg + "\n");
}


function cycleBackground(){

	$('.page .pageBg.pageBgCurrent').css("display", "block");
	
	setInterval(function(){
		// get current
		var $current = $('.page .pageBg.pageBgCurrent');
		
		// get next
		var $next = $current.next();
		if ($next.length==0)
			$next = $('.page .pageBg').first();
		
		$next.css('z-index', '-1');
		$current.css('z-index', '-2');
		$next.fadeIn(1500, function(){
			$current.fadeOut().removeClass('pageBgCurrent');
		}).addClass('pageBgCurrent');   
		
	}, 5000);

}

function initDebuggerShortcut(){
	$(document).bind('keypress', function(e){
		//listen for CTRL+K
		if (e.ctrlKey && e.keyCode==11)
			$('#console').toggle();
	});
}
