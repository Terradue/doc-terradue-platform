
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
	'modules/settings/models/safe',
	'modules/users/models/plans',
	'modules/passwordreset/models/passwordreset',
	'zeroClipboard',
	'canpromise',
	'messenger',
	'jasnyBootstrap',//'bootstrapFileUpload',
	'jqueryValidate',
	'ajaxFileUpload',
	'droppableTextarea',
	'jqueryCopyableInput',
	'latinise'
], function($, can, bootbox, BaseControl, Config, Helpers, ProfileModel, CertificateModel, OneConfigModel, GithubModel, OneUserModel, SafeModel, PlansModel, PasswordResetModel, ZeroClipboard){
	
	window.ZeroClipboard = ZeroClipboard;
	// regexpr validator
	$.validator.addMethod('regExpr', function(value, element, regExprStr) {
		var regExpr = new RegExp(regExprStr);
		return regExpr.test(value);
	}, function(regExprStr){
		return 'Enter a value which suits the regexpr "'+regExprStr+'"';
	});
	
	var SettingsControl = BaseControl(
		{
			defaults: { fade: 'slow' },
		},{
			
			// init
			init: function (element, options) {
				console.log("settingsControl.init");
				var self = this;
				
				this.params = Helpers.getUrlParameters();
				this.data = new can.Observe({});
				this.keyData = new can.Observe({});
				this.accountData = new can.Observe({});
				this.isLoginPromise = App.Login.isLoggedDeferred;
				this.githubPromise = GithubModel.findOne();
				this.configPromise = $.get('/'+Config.api+'/config?format=json');

				self.isLoginPromise.then(function(user){
					self.data.attr({
						user: user,
						emailConfirmOK: user.AccountStatus>1 && self.params.emailConfirm=='ok',
						showSafe: (user.DomainId!=0 && user.AccountStatus!=1),
						showGithub: (user.DomainId!=0 && user.AccountStatus!=1),
						showCloud: (user.DomainId!=0 && user.AccountStatus!=1),
						profileNotComplete: !(user.FirstName && user.LastName && user.Affiliation && user.Country),
						emailNotComplete: (user.AccountStatus==1),
						sshKeyNotComplete: !(user.PublicKey),
						githubNotComplete: false
					});
					self.githubPromise.then(function(githubData){
						self.githubData = githubData;
						self.data.attr('githubNotComplete', !githubData.HasSSHKey);
					});
				}).fail(function(){
					self.data.attr('hideMenu', true);
					// access denied only if you haven't a token
					if (!self.params.token)
						self.accessDenied();
				});
			},
			
			indexDependency: function(){
				return {
					url: 'modules/settings/views/index.html',
					data: this.data
				};
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
				var self = this;
				this.isLoginPromise.then(function(user){
					self.view(self.indexDependency());
				});
			},
			
			profile: function(options) {
				var self = this;
								
				this.profileData = new can.Observe({});
				this.view({
					url: 'modules/settings/views/profile.html',
					selector: Config.subContainer,
					dependency: self.indexDependency(),
					data: this.profileData,
					fnLoad: function(){
						self.initSubmenu('profile');
					}
				});
				
				console.log("App.controllers.Settings.profile");
				this.isLoginPromise.then(function(user){
					var usernameDefault = (user.Username == user.Email);
					if(usernameDefault) user.Username = null;
					self.profileData.attr({
						user: user,
						profileNotComplete: !(user.FirstName && user.LastName && user.Affiliation && user.Country),
						nameMissing: !(user.FirstName && user.LastName),
						usernameNotSet: usernameDefault,
						emailNotComplete: (user.AccountStatus==1),
						sshKeyNotComplete: !(user.PublicKey)
					});
					
					self.initProfileValidation();
					self.usernameGeneration();

					self.element.find('.usernameInfo').tooltip({
						trigger: 'hover',
						title: 'Username on the Terradue Cloud Platform, used to login on your VMs.',
						placement:'right'
					});
					
					if (self.params.token && self.profileData.user.AccountStatus==1)
						self.manageEmailConfirm(self.params.token);
					
				}).fail(function(){
					if (self.params.token)
						self.manageEmailConfirm(self.params.token);
					else
						self.accessDenied();
				});
			},

			account: function(options) {
				var self = this;

				this.view({
					url: 'modules/settings/views/account.html',
					selector: Config.subContainer,
					dependency: self.indexDependency(),
					data: this.accountData,
					fnLoad: function(){
						self.initSubmenu('profile');
					}
				});
				
				console.log("App.controllers.Settings.account");
				this.isLoginPromise.then(function(user){
					self.accountData.attr('user',user);
				}).fail(function(){
					self.accessDenied();
				});
			},
			
			email: function(options) {
				var self = this;
				this.profileData = new can.Observe({});
				this.view({
					url: 'modules/settings/views/email.html',
					selector: Config.subContainer,
					dependency: self.indexDependency(),
					data: this.profileData,
					fnLoad: function(){
						self.initSubmenu('email');
					}
				});
				
				console.log("App.controllers.Settings.email");
				this.isLoginPromise.then(function(user){
					self.profileData.attr({
						user: user,
						profileNotComplete: !(user.FirstName && user.LastName && user.Affiliation && user.Country),
						nameMissing: !(user.FirstName && user.LastName),
						emailNotComplete: (user.AccountStatus==1),
						sshKeyNotComplete: !(user.PublicKey)
					});
					if (self.params.token && self.profileData.user.AccountStatus==1)
						self.manageEmailConfirm(self.params.token);
					
				}).fail(function(){
					if (self.params.token)
						self.manageEmailConfirm(self.params.token);
					else
						self.accessDenied();
				});
			},

			github: function(options) {
				var self = this;
				console.log("App.controllers.Settings.github");
				self.isLoginPromise.then(function(userData){

					self.params = Helpers.getUrlParameters();

					if (self.params.code && self.params.state && self.params.state=='t2corporateportalpostkey'){
						GithubModel.getGithubToken(self.params.code)
						.then(function(){
							if(self.params.state=='t2corporateportalpostkey')
								GithubModel.postSshKey()
									.then(function(){
										self.githubData.attr('HasSSHKey', 'true');
										self.data.attr({
											githubNotComplete: false
										});
									})
									.fail(function(){
										bootbox.alert("<i class='fa fa-warning'></i> Cannot post the ssh key.")
									});
						})
						.fail(function(){
							bootbox.alert("<i class='fa fa-warning'></i> Error during put your GitHub token.");
						});
					}

					self.githubPromise.then(function(githubData){
						self.view({
							url: 'modules/settings/views/github.html',
							selector: Config.subContainer,
							dependency: self.indexDependency(),
							data: {
								user: userData,
								github: githubData
							},
							fnLoad: function(){
								self.initSubmenu('github');
							}
						});
					});
				});
			},

			key: function(options) {
				var self = this;

				console.log("App.controllers.Settings.key");
				
				self.view({
					url: 'modules/settings/views/key.html',
					selector: Config.subContainer,
					dependency: self.indexDependency(),
					data: self.keyData,
					fnLoad: function(){
						self.initSubmenu('key');
					}
				});

				self.isLoginPromise.then(function(userData){
					self.keyData.attr(userData.attr());
					self.keyData.attr('PublicKeyBase64', btoa(userData.PublicKey))
					self.element.find('.copyPublicKeyBtn').copyableInput(userData.PublicKey, {
						isButton: true,
					});
					self.element.find('.downloadKey').tooltip({
						trigger: 'hover',
						title: 'Download',
						placement:'bottom'
					});
					self.element.find('.deletePublicKeyBtn').tooltip({
						trigger: 'hover',
						title: 'Delete',
						placement:'bottom'
					});
				});
			},

			// profile
			
			manageEmailConfirm: function(token){
				var self=this;
				$.getJSON('/t2api/user/emailconfirm?token='+token, function(){
					// reinit
					//document.location = '/portal/settings/profile?emailConfirm=ok';
					if (self.profileData.user)
						document.location = '/portal/settings/profile?emailConfirm=ok';
					else {
						// you are not connected, only show the message
						self.profileData.attr('emailConfirmOK', true);
						self.data.attr('emailConfirmOK', true);
					}
					
				}).fail(function(xhr){
					self.errorView({}, 'Unable to get the token.', Helpers.getErrMsg(xhr), true);
				});
			},
			
			initProfileValidation: function(){
				var self = this;
				var $form = this.element.find('form.profileForm').validate({
					rules: {
						FirstName: 'required',
						LastName: 'required',
						Username: {
							required: true,
							regExpr: '^[a-z][0-9a-z]{1,31}$',
							remote: {
						        url: "/t2api/user/ldap/available?format=json",
						        type: "GET",
						        processData: true,
						        data: {
						        	Username: function() {
						        		return self.element.find('input[name="Username"]').val();
						        	}
						        },
						        noStringify: true,
						        beforeSend: function(){
						        	self.profileData.attr('usernameLoader', true);
						        	self.element.find('input[name="Username"]').parent().find('label.error').empty();
						        },
						        complete: function(){
						        	self.profileData.attr('usernameLoader', false);
						        }
							}
						}
					},
					messages: {
						FirstName: '<i class="fa fa-times-circle"></i> Please insert your First Name',
						LastName: '<i class="fa fa-times-circle"></i> Please insert your Last Name',
						Username: {
							required: '<i class="fa fa-times-circle"></i> Please insert your Cloud Username',
							regExpr: '<i class="fa fa-times-circle"></i> The username should start by a letter, have letters and numbers and 32 chars.</span>',
							remote: '<i class="fa fa-times-circle"></i> This username is already taken, please choose another one.</span>'
						}
					},
					// set this class to error-labels to indicate valid fields
					success: function(label, element) {
						if ($(element).attr('name')=='Username')
							label.html('<span class="text-success"><i class="fa fa-check-circle"></i> The username is free and available.</span>');
					},

					submitHandler: function(form){
						var username = $(form).find('input[name="Username"]').val();
						bootbox.confirm('Your Cloud Username will be <b>'+username+'</b> and it cannot be changed. <br/>Are you sure?', function(confirmed){
							if (confirmed)
								self.profileSubmit();
						});
						return false;
					}
				});
			},
			
			usernameGeneration: function(){
				if (this.profileData.user.Username) // if is set do nothing
					return;
				
				var $firstName = this.element.find('input[name="FirstName"]');
				var $lastName = this.element.find('input[name="LastName"]');
				var $username = this.element.find('input[name="Username"]');
				var timeout;
				
				var setUsernameFn = function(e){
					if (e && e.keyCode && (e.keyCode==9 || e.keyCode==16))
						return;
					
					var firstName = $firstName.val().toLowerCase();;
					var lastName = $lastName.val().toLowerCase();;
					if (!firstName || !lastName)
						return;
					
					var firstChar = firstName.split(' ')[0][0];
					var lastNames = lastName.split(' ');
					var lastSurname = lastNames[lastNames.length-1];
					var Username = (firstChar.latinise() + lastSurname.latinise()).substring(0,32);
					
					$username.val(Username).valid();
				};
				
				$firstName.on('change', function(){console.log('change')});
				$firstName.keyup(setUsernameFn);
				$lastName.keyup(setUsernameFn);
				setUsernameFn();
			},
			
			profileSubmit: function(){
				// get data
				var self= this,
					usr = Helpers.retrieveDataFromForm('.settings-profile form',
						['FirstName','LastName','Username','Affiliation','Country','EmailNotification']);
				
				// update
				App.Login.User.current.attr(usr); 
				// save
				self.profileData.attr({saveLoading: true, saveSuccess: false, saveFail: false});
				new ProfileModel(App.Login.User.current.attr())
					.save()
					.then(function(createdUser){
						self.profileData.attr({
							saveSuccess: true,
							saveLoading: false, 
							profileNotComplete: !(createdUser.FirstName && createdUser.LastName && createdUser.Affiliation && createdUser.Country),
							usernameNotSet: createdUser.Username == createdUser.Email,
							nameMissing: !(createdUser.FirstName && createdUser.LastName),
						});
						self.data.attr({
							profileNotComplete: !(createdUser.FirstName && createdUser.LastName && createdUser.Affiliation && createdUser.Country)
						});

					}).fail(function(xhr){
						self.profileData.attr({saveLoading: false, saveFail: true, saveFailMessage: Helpers.getErrMsg(xhr)});
					});
				
				return false;
			},
			
			'.settings-profile .signIn click': function(){
				App.Login.openLoginForm();
			},

			// send email pending activation
			'a.sendConfirmationEmail click': function(elem){
				var $span = $('<span> <i class="fa fa-spin fa-spinner"></i></span>')
					.insertAfter(elem);
				//elem.remove();
				$.post('/'+Config.api+'/user/emailconfirm?format=json', {}, function(){
					$span.addClass('text-success').html('<br/><strong>Email sent!</strong>');
				}).fail(function(xhr){
					$span.addClass('text-danger').html('<br/><strong>Error: </strong>'+Helpers.getErrMsg(xhr));
				});
			},

			'.settings-account .passwordForm .submit click': function(){
				var self = this;
				var oldpassword = Helpers.retrieveDataFromForm('.passwordForm','oldpassword');
				var newpassword = Helpers.retrieveDataFromForm('.passwordForm','newpassword');
				self.accountData.attr({
					passwordLoading: true, 
				});
				PasswordResetModel.updatePassword(oldpassword,newpassword).then(function(){
					self.accountData.attr({
						passwordLoading: false, 
						passwordSaveSuccess: true,
					});
				}).fail(function(xhr){
					self.accountData.attr({
						passwordLoading: false, 
						passwordSaveFail: true,
						passwordErrorMessage: Helpers.getErrMsg(xhr, 'Unable to update the password. Please contact the Administrator.'),
					});
				});
			},

			'.settings-email .email-change .submit click': function(){
				var self = this;

				ProfileModel.updatePassword(oldpassword,newpassword).then(function(){
					self.accountData.attr({
						loading: false, 
						saveSuccess: true,
					});
				}).fail(function(xhr){
					self.accountData.attr({
						loading: false, 
						saveFail: true,
						errorMessage: Helpers.getErrMsg(xhr, 'Unable to update the password. Please contact the Administrator.'),
					});
				});
			},
			
			//github
			'.settings-github .usernameForm .submit click': function(){
				// get data
				var githubName = Helpers.retrieveDataFromForm('.modifyGithubName',	'GithubName');
				
				// TODO check!
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
				$('.settings-github .modifyGithubName').removeClass('hide');
			},
			
			'.settings-github .addPublicKey click': 'addPublicKey',
			addPublicKey: function(){
				var self = this;
				
				this.configPromise.then(function(_serviceConfig){
					var serviceConfig = Helpers.keyValueArrayToJson(_serviceConfig, 'Key', 'Value');

					self.githubData.attr("loading",true);
					GithubModel.postSshKey()
					.then(function(){
						self.githubData.attr('HasSSHKey', 'true');
						self.data.attr({
							githubNotComplete: false
						});
					})
					.fail(function(xhr){
						if (!xhr.responseJSON)
							xhr.responseJSON = JSON.parse(xhr.responseText)
							 
								// get github client id
								var githubClientId = serviceConfig['Github-client-id'];
								if (!githubClientId)
									return;
								
								// redirect to github
								window.location = 'https://github.com/login/oauth/authorize?client_id='+githubClientId
									+'&scope=write:public_key,repo&state=t2corporateportalpostkey&redirect_uri=' + document.location.href
								
					})
					.always(function(){
						self.githubData.attr("loading",true);
					});
				});
			},

			/* ssh key */
			'.settings-key .generateSafe click': function(){
				var self = this;
				var $dialog;
				var createSafeCallback = function(safe){
					
					// setup keyData
					self.keyData.attr({
						PublicKey: safe.PublicKey,
						PrivateKey: safe.PrivateKey,
						PublicKeyBase64: btoa(safe.PublicKey),
						PrivateKeyBase64: btoa(safe.PrivateKey)
					});
					
					// setup github data
					if (self.githubData)
						self.githubData.attr('CertPub', safe.PublicKey);
					
					// setup global data
					self.data.attr({
						sshKeyNotComplete: false,
						githubNotComplete: true //we just create a new keys so it cannot be already on github
					});
					
					// setup buttons
					self.element.find('.copyPublicKeyBtn').copyableInput(safe.PublicKey, {
						isButton: true,
					});
					
					self.element.find('.copyPrivateKeyBtn').copyableInput(safe.PrivateKey, {
						isButton: true,
					});
					self.element.find('.downloadKey').tooltip({
						trigger: 'hover',
						title: 'Download',
						placement:'bottom'
					});
				};
				
				var submitFormSafeCallback = function(){
                    var password = $('#safePassword').val();
                    if (password==''){
                    	bootbox.alert("<i class='fa fa-warning'></i> Password is empty.");
                    	return false;
                    };
                    
                    self.keyData.attr("loading",true);
                    SafeModel.create(password).then(createSafeCallback).fail(function(){
						bootbox.alert("<i class='fa fa-warning'></i> Error during safe creation.");
					}).always(function(){
						self.keyData.attr("loading",false);
					});
				};
				
				var message = "<div class='container-fluid'>"
					+ "<form class='createSafeForm'>"
					+ "<div class='form-group'>" 
					+ "<label for='password'>Password</label>"
					+ "<input type='password' class='form-control' name='password' id='safePassword' placeholder='Password'>"
					+ "</div>"
					+ "</form>"
					+ "</div>";

				var title = "This action requires your password.";
				if (this.keyData.PublicKey)
					title += "<br/><small><i>Please note that this will overwrite your current SSH key pair.</i></small>";
				$dialog = bootbox.dialog({
					title: title,
					message: message,
					buttons: {
	                    success: {
	                        label: "OK",
	                        className: "btn-default",
	                        callback: submitFormSafeCallback
                    	}
                    }
				});
				
				$dialog.find('form').submit(function(){
					submitFormSafeCallback();
					$dialog.modal('hide');
					return false;
				});
			},

			'.settings-key .deletePublicKeyBtn click': function(){
				var self = this;
				var title = "This action requires your password.";
				if(self.keyData.PublicKey)
					title += "<br/><small><i>Please note that this will delete your current SSH key pair from your VMs.</i></small>";
				var message = "<div class='container-fluid'>"
							+ "<form class='createSafeForm'>"
							+ "<div class='form-group'>" 
							+ "<label for='password'>Password</label>"
							+"<input type='password' class='form-control' name='password' id='safePassword' placeholder='Password'>"
							+ "</div>"
							+ "</form>"
							+ "</div>";
				bootbox.dialog({
					title: title,
					message: message,
					buttons: {
	                    success: {
	                        label: "OK",
	                        className: "btn-default",
	                        callback: function (a,b,c) {
	                            var password = $('#safePassword').val();
	                            if (password==''){
	                            	bootbox.alert("<i class='fa fa-warning'></i> Password is empty.");
	                            	return false;
	                            };
	                            self.keyData.attr("loading",true);
	                            SafeModel.delete(password).then(function(safe){
	                            	self.data.attr({
										sshKeyNotComplete: true,
										githubNotComplete: true//we just create a new keys so it cannot be already on github
									});
							    	self.keyData.attr("PublicKey",null);
							    	self.keyData.attr("PrivateKey",null);
							    	self.keyData.attr("PublicKeyBase64",null);
							    	self.keyData.attr("PrivateKeyBase64",null);

								}).fail(function(){
									bootbox.alert("<i class='fa fa-warning'></i> Error during ssh keys delete.");
								}).always(function(){
									self.keyData.attr("loading",false);
								});
                        	}
                    	}
                    }
				});
			}

		}
	);
		
	return new SettingsControl(Config.mainContainer, {});
	
});
