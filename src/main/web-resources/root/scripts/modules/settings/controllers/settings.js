
define([
	'jquery',
	'can',
	'bootbox',
	'utils/baseControl',
	'configurations/settings',
	'utils/helpers',
	'modules/settings/models/profile',
	'modules/settings/models/certificate',
	'modules/settings/models/oneConfig',
	'modules/settings/models/github',
	'modules/settings/models/oneUser',
	'canpromise',
	'messenger',
	'bootstrapFileUpload',
	'jqueryValidate',
	'ajaxFileUpload',
], function($, can, bootbox, BaseControl, Config, Helpers, ProfileModel, CertificateModel, OneConfigModel, GithubModel, OneUserModel){
	var SettingsControl = BaseControl(
		{
			defaults: { fade: 'slow' },
		},{
			indexDependency: { url: 'modules/settings/views/index.html' },
			
			// init
			init: function (element, options) {
				console.log("settingsControl.init");
				this.isLoginPromise = App.Login.isLoggedDeferred;
				this.githubPromise = GithubModel.findOne();
			},
			
			initSubmenu: function(item){
				this.element
					.find('nav.submenu li.active')
					.removeClass('active');
				this.element
					.find('nav.submenu li.'+item)
					.addClass('active');				
			},
			
			// actions
			
			index: function (options) {
				console.log("App.controllers.Settings.index");
				this.view(this.indexDependency);
			},
			
			profile: function(options) {
				var self = this;
				console.log("App.controllers.Settings.profile");
				this.isLoginPromise.then(function(user){
					self.view({
						url: 'modules/settings/views/profile.html',
						selector: Config.subContainer,
						dependency: self.indexDependency,
						data: user,
						fnLoad: function(){
							self.initSubmenu('profile');
						}
					});
				});
			},
			
			certificate: function(options) {
				var self = this;
				console.log("App.controllers.Settings.certificate");
				this.certificateData = new can.Observe({});
				this.isLoginPromise.then(function(user){
					self.certificateData.attr({
						user: user,
						profileComplete: (user.attr('Affiliation') && user.attr('Email') && user.attr('FirstName') && user.attr('LastName') && user.attr('Country')),
					});
					self.view({
						url: 'modules/settings/views/certificate.html',
						selector: Config.subContainer,
						dependency: self.indexDependency,
						data: self.certificateData,
						fnLoad: function(){
							self.initSubmenu('certificate');
							
							if (user.CertSubject) {
								// get certificate info
								CertificateModel.findOne({}, function(cert){
									App.Login.User.current.attr("CertSubject", cert.Subject);
									self.certificateData.attr('certificate', cert);
								}, function(res){
									if (res.status=='404')
										self.initCertUpload();
								});					
							} else
								self.initCertUpload();
						}
					});
				});
			},
			
			cloud: function(options) {
				var self = this;
				console.log("App.controllers.Settings.cloud");
				this.isLoginPromise.then(function(user){
					OneConfigModel.findAll().then(function(_oneSettings){
						oneSettings = Helpers.keyValueArrayToJson(_oneSettings);
						OneUserModel.findOne().then(function(oneUser){
							self.view({
								url: 'modules/settings/views/cloud.html',
								selector: Config.subContainer,
								data: {
									oneSettings: oneSettings,
									oneUser: oneUser,
									sunstoneOk: user.CertSubject == user.OnePassword,
									user: user,
									onePasswordOk: user.OnePassword,
									oneCertOk: user.CertSubject
								},
								dependency: self.indexDependency,
								fnLoad: function(){
									self.initSubmenu('cloud');
								}
							});
						});
					});
				});
			},
			
			github: function(options) {
				var self = this;
				console.log("App.controllers.Settings.github");
				self.isLoginPromise.then(function(userData){
					self.githubPromise.then(function(githubData){
						self.githubData = githubData;
						self.view({
							url: 'modules/settings/views/github.html',
							selector: Config.subContainer,
							dependency: self.indexDependency,
							data: {
								user: userData,
								github: githubData,
							},
							fnLoad: function(){
								self.initSubmenu('github');
							}
						});
					});
				});
			},
			

			
			// profile			
			'.settings-profile .submit click': function(){
				// get data
				var usr = Helpers.retrieveDataFromForm('.settings-profile form',
						['FirstName','LastName','Email','Affiliation','Country','EmailNotification']);
				
				// update
				App.Login.User.current.attr(usr); 
				// save
				new ProfileModel(App.Login.User.current.attr())
					.save()
					.then(function(createdNews){
						Messenger().post({
							message: 'Profile successfully saved.',
							type: 'success',
							//showCloseButton: true,
							hideAfter: 4,
						});
					});
				
				return false;
			},
			
			//github
			'.settings-github .usernameForm .submit click': function(){
				// get data
				var githubName = Helpers.retrieveDataFromForm('.modifyGithubName',	'GithubName');
				
				this.githubPromise.then(function(githubData){
					githubData.attr('Name', githubName);
					// update
					new GithubModel({
							Name: githubName,
							Id: githubData.attr('Id'),
						})
						.save()
						.done(function(githubDataNew){
							githubData.attr(githubDataNew.attr(), true);
							Messenger().post({
								message: 'Github Username saved.',
								type: 'success',
								hideAfter: 4,
							});
						});
				});
				
				return false;
			},
			
			'.settings-github .usernameForm .cancel click': function(){
				$('.settings-github .githubName').css('display', 'inline-block');
				$('.settings-github .modifyGithubName').hide();
			},
			
			'.settings-github .showModifyGithubName click': function(){
				$('.settings-github .githubName').hide();
				$('.settings-github .modifyGithubName').show();
			},
			
			'.settings-github .githubKeyPanel .addPublicKey click': 'addPublicKey',
			
			addPublicKey: function(){
				var self = this;
				$('.githubKeyPanel').mask('wait');
				GithubModel.postSshKey(function(){
					$('.githubKeyPanel').unmask();
					self.githubData.attr('HasSSHKey', 'true');
				}, function(xhr){
					$('.githubKeyPanel').unmask();
					if (xhr.responseJSON.ResponseStatus.Message == "Invalid token"){
						bootbox.prompt({
							title:'Insert your github password to obtain the token', 
							inputType: 'password',
							callback: function(githubPwd){
								$('.githubKeyPanel').mask('wait');
								GithubModel.getGithubToken(githubPwd, function(ris){
									$('.githubKeyPanel').unmask();
//									bootbox.alert("<i class='fa fa-warning'></i> The Github Password is not valid.");
//									alert("OK! reload addPublicKey");
									self.addPublicKey();
								},function(){
									$('.githubKeyPanel').unmask();
									bootbox.alert("<i class='fa fa-warning'></i> The Github Password is not valid.");
								});
							}
						});
					} else
						bootbox.alert("Generic error.");						
				});
			},
			
			// certificate
			'#submitCertRequest click': function(){		
				var self = this;
				this.askPassword("Password for certificate", function(password){
					new CertificateModel({
							password: password
						})
						.save()
						.done(function(userCert){
							App.Login.User.current.attr("CertSubject", userCert.Subject);
							self.certificate();
						})
						.fail(function(){
							// TODO
							alert("error");
						});
				});
			},
			
			'#removeCertificate click': function(){
				var self = this;
				bootbox.confirm('Are you sure you want to remove your current certificate?', function(result) {
					if (result){
						new CertificateModel({
							Id: App.Login.User.current.Id,
						})
						.destroy()
						.done(function(){
							App.Login.User.current.attr("CertSubject", null);
							self.certificate();
						})
						.fail(function(){
							// TODO
							alert("error");
						});
					}
				});
			},
			
			'#showEncodedPem click': function(element){
				$('.encodedPem').show('slow');
				element.hide();
			},
			
			/////
			askPassword: function(title, event) {

				$('#askPasswordModal').remove();

				var passwordModal = $(''
				+ '<div class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">'
				+ '<form name="passForm" id="passForm" method="post" action="/t2api/cert">'
				+ '<div class="modal-header">'
				+ '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">x</button>'
				+ '<h4 id="askPasswordTitle">' + title + '</h4>'
				+ '</div>'
				+ '<div class="modal-body">'
				+ '<p>You must now enter a password to protect the key of your certificate</p><br />'
				+ '<div class="fieldContainer">'
				+ '<div class="fieldDescription">Password</div>'
				+ '<input class="password" name="password" id="password" type="password" placeholder="Your password"/>'
				+ '</div>'
				+ '<br/>'
				+ '<div class="fieldContainer">'
				+ '<div class="fieldDescription">Confirm your password</div>'
				+ '<input name="password_confirm" id="password_confirm" type="password" placeholder="Confirm your password" />'
				+ '</div>'
				+ '<span class="label label-important">Important</span> This password is not recoverable. Please choose it wisely!'
				+ "<br>It must have:<ul>"
				+ "<li>length at least 8 characters</li>"
				+ "<li>at least one uppercase character</li>"
				+ "<li>at least one lowercase character</li>"
				+ "<li>at least one number</li>"
				+ "<li>at least one special character</li>"
				+ "</ul><span class='label label-important' id='askPasswordErrors'></span>"
				+ '</div>'
				+ '<div class="modal-footer">'
				+ '<button class="btn" data-dismiss="modal" aria-hidden="true">Cancel</button>'
				+ '<button type="submit" value="OK" class="btn btn-primary">OK</button>'
				+ '</div></form>'
				+ '</div>').attr('id','askPasswordModal').appendTo('body');  

				// Validator extensions
				jQuery.validator.addMethod(
					"atLeastOneUpper",
					function(value, element) {
						return this.optional(element) || new RegExp("[A-Z]").test(value);
					},
					"* atLeastOneUpper"
				);

				jQuery.validator.addMethod(
					"atLeastOneLower",
					function(value, element) {
						return this.optional(element) || new RegExp("[a-z]").test(value);
					},
					"* atLeastOneUpper"
				);

				jQuery.validator.addMethod(
					"atLeastOneNumber",
					function(value, element) {
						return this.optional(element) || new RegExp("[\\d]").test(value);
					},
					"* atLeastOneUpper"
				);

				jQuery.validator.addMethod(
					"atLeastOneSpecialChar",
					function(value, element) {
						return this.optional(element) || new RegExp("[\\W]").test(value);
					},
					"atLeastOneSpecialChar"
				);

				jQuery.validator.addMethod(
					"noSpecialChars",
					function(value, element) {
						return this.optional(element) || !(new RegExp("[\\W]").test(value));
					},
					"Please remove special characters"
				);

				var PASSWORD_MIN_LENGTH = "Password must be at least 8 characters"
			    var PASSWORD_REQUIRED = "Create a password";
			    var PASSWORD_AT_LEAST_ONE_UPPER_CASE = "Password must include at least one uppercase character";
			    var PASSWORD_AT_LEAST_ONE_LOWER_CASE = "Password must include at least one lowercase character";
			    var PASSWORD_AT_LEAST_ONE_NUMBER = "Password must include at least one number";
			    var PASSWORD_AT_LEAST_SPECIAL_CHAR = "Password must include at least one special character";
			    var PASSWORD_CONFIRM = "Passwords do not match!";

				var stop = false;	

				var pwdRule = {
					required: true,
					minlength: 8,
					atLeastOneUpper: true,
					atLeastOneLower: true,
					atLeastOneNumber: true,
					atLeastOneSpecialChar: true,
				};

				$('#passForm').validate({
					rules: {
						password: pwdRule,
						password_confirm: {
							equalTo: "#password",
						},
					},
					messages: {
						password: {
							required: PASSWORD_REQUIRED,
							atLeastOneUpper: PASSWORD_AT_LEAST_ONE_UPPER_CASE,
							atLeastOneLower: PASSWORD_AT_LEAST_ONE_LOWER_CASE,
							atLeastOneNumber: PASSWORD_AT_LEAST_ONE_NUMBER,
							atLeastOneSpecialChar: PASSWORD_AT_LEAST_SPECIAL_CHAR,
							minlength: PASSWORD_MIN_LENGTH,
						},
						password_confirm: PASSWORD_CONFIRM,
					},
					submitHandler: function() {
						$("#askPasswordModal").modal('hide');
						return event($("#password_confirm").val());
					}
				});

				$("#askPasswordModal").modal();
			},
			
			// init cert upload
			initCertUpload: function(){
				var self = this;
				this.certificateData.attr('certRequest', true);
				$("#formUpload").submit(function(){
					if ($("#fileUpload").val()==""){
						$("#selectFileError").show();
						return false;
					}			
					self.ajaxFileUpload();

					return false;
				});
			},
			
			ajaxFileUpload: function() {
				//starting setting some animation when the ajax starts and completes
				$("#loading").ajaxStart(function(){
					$(this).show();
				}).ajaxComplete(function(){
					$(this).hide();
				});
				
				var self = this;
				$.ajaxFileUpload({
					url:'/'+Config.api+'/cert/_upload?format=json', 
					secureuri:false,
					fileElementId:'fileUpload',
					success: function (data, status){
						var certificate = jQuery.parseJSON(data.body.innerText);
						if ( certificate.ResponseStatus && certificate.ResponseStatus.ErrorCode)
							self.errorUpload (data, status, null);
						else {
							self.certificateData.attr({
								certificate: certificate,
								resultMessageType: 'success',
								resultMessage: "",
								certRequest: false,
							});
							App.Login.User.current.attr('CertSubject', certificate.Subject);
						}
					},
					error: self.errorUpload
				});

			    return false;
			},
			
			errorUpload: function(data, status, e){
				var exception = jQuery.parseJSON(data.body.innerText);
				this.certificateData.attr({
					resultMessageType: 'error',
					resultMessage: "<strong>Error!</strong> Your certificate is not uploaded. " + exception.ResponseStatus.Message + "<br>"
						+ "<p>Please contact the administrator.</p>",
				});
			},
			
			
			/* cloud */
			'.createOneUser click': function(){				
				var self = this, user = App.Login.User.current;

				OneUserModel.createOneUser({
					Password: user.attr('CertSubject'),
				}, function(){
					App.Login.User.current.attr('OnePassword', user.attr('CertSubject'));
					self.cloud();
				}, function(){
					alert("error!");
				});
			},

			'.setCertificateAsOnePassword click': function(){
				var self = this, user = App.Login.User.current;
				OneUserModel.findOne().then(function(oneUser){
					OneUserModel.setOnePassword({
						Id: oneUser.attr('Id'),
						Password: user.attr('CertSubject'),
					}, function(){
						App.Login.User.current.attr('OnePassword', user.attr('CertSubject'));
						self.cloud();
					}, function(){
						alert("error!");
					});
				});
			}

		}
	);
		
	return new SettingsControl(Config.mainContainer, {});
	
});
