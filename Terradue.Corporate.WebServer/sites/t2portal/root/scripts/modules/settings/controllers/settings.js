
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
	//'canpromise',
	'messenger',
	'jasnyBootstrap',//'bootstrapFileUpload',
	'jqueryValidate',
	'ajaxFileUpload',
	'droppableTextarea',
	'jqueryCopyableInput',
	'latinise',
	'jqueryCookie'
	
], function($, can, bootbox, BaseControl, Config, Helpers, ProfileModel, CertificateModel, OneConfigModel, GithubModel, OneUserModel, SafeModel, PlansModel, PasswordResetModel, ZeroClipboard){
	
window.ZeroClipboard = ZeroClipboard;
// regexpr validator
$.validator.addMethod('regExpr', function(value, element, regExprStr) {
	var regExpr = new RegExp(regExprStr);
	return regExpr.test(value);
}, function(regExprStr){
	return 'Enter a value which suits the regexpr "'+regExprStr+'"';
});

var DEFAULT_PASSWORD_RULES = {
		required: true,
		minlength: 8,
		atLeastOneUpper: true,
		atLeastOneLower: true,
		atLeastOneNumber: true,
		atLeastOneSpecialChar: true,
		noOtherSpecialChars: true,
};
var DEFAULT_PASSWORD_MESSAGES = {
	required: 'Insert a password',
	minlength: 'Password must be at least 8 characters',
	atLeastOneUpper: 'Password must include at least one uppercase character',
	atLeastOneLower: 'Password must include at least one lowercase character',
	atLeastOneNumber: 'Password must include at least one number',
	atLeastOneSpecialChar: 'Password must include at least one special character in the list !@#$%^&*()_+',
	noOtherSpecialChars: 'Password can\'t include special characters different from the list !@#$%^&*()_+',
};
var DEFAULT_PASSWORD_POPOVER_SETTINGS = {
	trigger: 'focus',
	placement: 'left',
	title: 'Password',
	html: true,
	content: 'It must have:<ul>'
		+'<li>at least 8 characters</li>'
		+'<li>at least one uppercase character</li>'
		+'<li>at least one lowercase character</li>'
		+'<li>at least one number</li>'
		+'<li>at least one special character, chosen from the list: ! @ # $ % ^ & * ( ) _ +</li>'
		+'<li>no other special characters are permitted</li>'
		+'</ul>',
}


var SettingsControl = BaseControl(
	{
		defaults: { fade: 'slow' },
	},{
		
		// init
		init: function (element, options) {
			console.log("settingsControl.init");
			var self = this;
			
			Helpers.addPasswordValidationMethods();
			
			this.params = Helpers.getUrlParameters();
			this.data = new can.Observe({});
			this.githubData = new can.Observe({});
			this.keyData = new can.Observe({});
			this.isLoginPromise = App.Login.isLoggedDeferred;
			this.githubPromise = GithubModel.findOne();
			this.fullUserPromise = ProfileModel.getFullUser(true);
			this.configPromise = $.get('/'+Config.api+'/config?format=json');

			self.data.attr({loading: true});

			self.fullUserPromise.then(function(user){
				var usernameDefault = (user.Username == null || user.Username == user.Email);
				var accountEnabled = user.AccountStatus == 4;
				self.data.attr({
					user: user,
					emailConfirmOK: user.AccountStatus>1 && self.params.emailConfirm=='ok',
					showSafe: (user.DomainId!=0 && user.AccountStatus!=1),
					showGithub: (user.DomainId!=0 && user.AccountStatus!=1),
					showCloud: (user.DomainId!=0 && user.AccountStatus!=1),
					showCatalogue: user.Level == 4 || user.Plan == "Explorer" || user.Plan == "Scaler" || user.Plan == "Premium",
					showStorage: user.Level == 4 || user.Plan == "Explorer" || user.Plan == "Scaler" || user.Plan == "Premium",
					showFeatures: user.Level == 4 || user.Plan == "Explorer" || user.Plan == "Scaler" || user.Plan == "Premium",
					profileNotComplete: !(user.FirstName && user.LastName && user.Affiliation && user.Country),
					emailNotComplete: (user.AccountStatus==1),
					usernameNotSet: usernameDefault,
					emailNotComplete: !accountEnabled,
					ldapCreated: user.HasLdapAccount,
					loading: false
				});
			}).fail(function(){
				self.data.attr({'hideMenu': true, loading: false});
				// access denied only if you haven't a token
				if (!self.params.token)
					self.accessDenied();
			});

			self.fullUserPromise.then(function(user){
				self.data.attr({
					ldapNotCreated: !user.HasLdapAccount
				});
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
				.find('.submenu li.active')
				.removeClass('active');
			this.element
				.find('.submenu li.'+item)
				.addClass('active');				
		},
		
		// overwrite accessDenied default function
		accessDenied: function(){
			document.location = '/t2api/oauth'
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
			
			console.log("App.controllers.Settings.profile");
			
			// first wait user is ready
			this.isLoginPromise.then(function(user){
				// create the view
				var usernameDefault = (user.Username == null || user.Username == user.Email);
				if(usernameDefault) user.Username = null;

				self.profileData = new can.Observe({
					user: user,
					profileNotComplete: !(user.FirstName && user.LastName && user.Affiliation && user.Country),
					nameMissing: !(user.FirstName && user.LastName),
					usernameNotSet: usernameDefault
				});
				self.view({
					url: 'modules/settings/views/profile.html',
					selector: Config.subContainer,
					dependency: self.indexDependency(),
					data: self.profileData,
					fnLoad: function(){
						self.initSubmenu('profile');

						self.initProfileValidation();
						self.usernameGeneration();
						
						self.element.find('.usernameInfo').tooltip({
							trigger: 'hover',
							title: 'Username on the Terradue Cloud Platform, used to login on your VMs.',
							placement:'right'
						});
					}
				});
				
			}).fail(function(){
				self.accessDenied();
			});
		},

		account: function(options) {
			var self = this;
			this.accountData = new can.Observe({});
			this.params = Helpers.getUrlParameters();
			this.element.html('<br/><br/><p style="text-align:center"><i class="fa fa-spin fa-spinner"></i> Loading account data...</p>');
			
			console.log("App.controllers.Settings.account");
			// first wait user is ready
			this.fullUserPromise.then(function(user){
				var usernameDefault = (user.Username == null || user.Username == user.Email);
				self.accountData.attr({
					user: user,
					usernameSet: !(user.Email == user.Username),
					emailNotComplete: (user.AccountStatus==1),
					emailConfirmOK: user.AccountStatus>1 && self.params.emailConfirm=='ok',
					usernameNotSet: usernameDefault
				});

				self.view({
					url: 'modules/settings/views/account.html',
					selector: Config.subContainer,
					dependency: self.indexDependency(),
					data: self.accountData,
					fnLoad: function(){
						self.initSubmenu('account');
						self.initAccount(); // in this way we are sure that the view
							// contains the user data, because user is inside accountData
							// before to start the view
					}
				});

				if (self.params.token && user.AccountStatus==1)
					self.manageEmailConfirm(self.params.token);
				
			}).fail(function(){
				if (self.params.token){
					self.view({
						url: 'modules/settings/views/account.html',
						selector: Config.subContainer,
						dependency: self.indexDependency(),
						data: self.accountData,
						fnLoad: function(){
							self.initSubmenu('account');
							self.manageEmailConfirm(self.params.token);
						}
					});						
				}
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
								})
								.fail(function(){
									bootbox.alert("<i class='fa fa-warning'></i> Cannot post the ssh key.")
								});
					})
					.fail(function(){
						bootbox.alert("<i class='fa fa-warning'></i> Error during put your GitHub token.");
					});
				}

				self.githubPromise.then(function(data){
					self.githubData.attr(data.attr());
					self.githubData.attr('loaded',true);
					self.view({
						url: 'modules/settings/views/github.html',
						selector: Config.subContainer,
						dependency: self.indexDependency(),
						data: {
							user: userData,
							github: self.githubData
						},
						fnLoad: function(){
							self.initSubmenu('github');
						}
					});
				});
			});
		},

		plan: function(options) {
			var self = this;
			self.profileData = new can.Observe({});
			console.log("App.controllers.Settings.plan");
			
			self.view({
				url: 'modules/settings/views/plan.html',
				selector: Config.subContainer,
				dependency: self.indexDependency(),
				data: self.profileData,
				fnLoad: function(){
					self.initSubmenu('plan');
				}
			});

			this.isLoginPromise.then(function(user){
				self.profileData.attr({
					user: user
				});
				
			}).fail(function(){
				self.accessDenied();
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

			self.fullUserPromise.then(function(user){
				self.keyData.attr({user:user});
				self.keyData.user.attr('PublicKeyBase64', btoa(user.PublicKey));
				self.keyData.attr('loaded', true);
			});
		},

		apikey: function(options) {
			var self = this;
			self.apikeyData = new can.Observe({});
			console.log("App.controllers.Settings.ApiKey");
			
			self.view({
				url: 'modules/settings/views/apikey.html',
				selector: Config.subContainer,
				dependency: self.indexDependency(),
				data: self.apikeyData,
				fnLoad: function(){
					self.initSubmenu('apikey');
				}
			});

			self.fullUserPromise.then(function(user){
				self.apikeyData.attr({
					user: user,
					loaded: true
				});
			});
		},

		catalogue: function(options) {
			var self = this;
			console.log("App.controllers.Settings.catalogue");
			
			// load storage list only the first time
			if (!this.catalogueData){
				this.catalogueData = new can.Observe({loading: true});
				this.loadCatalogueIndex();
			}

			this.view({
				url: 'modules/settings/views/catalogue.html',
				selector: Config.subContainer,
				dependency: self.indexDependency(),
				data: this.catalogueData,
				fnLoad: function(){
					self.initSubmenu('catalogue');
				}
			});
		},

		storage: function(options) {
			var self = this;
			
			console.log("App.controllers.Settings.storage");
			
			// load storage list only the first time
			if (!this.storageData){
				this.storageData = new can.Observe({loading: true});
				this.loadStorage();
			}
			
			this.view({
				url: 'modules/settings/views/storage.html',
				selector: Config.subContainer,
				dependency: self.indexDependency(),
				data: this.storageData,
				fnLoad: function(){
					self.initSubmenu('storage');
				}
			});
		},
		features: function(options) {
			var self = this;
			self.featuresData = new can.Observe({loading: true});
			console.log("App.controllers.Settings.features");
			
			this.view({
				url: 'modules/settings/views/features.html',
				selector: Config.subContainer,
				dependency: self.indexDependency(),
				data: this.featuresData,
				fnLoad: function(){
					self.initSubmenu('features');
				}
			});
			this.loadFeatures();
		},
		
		// profile	
		manageEmailConfirm: function(token){
			var self=this;
			$.getJSON('/t2api/user/emailconfirm?token='+token, function(){
				self.accountData.attr('emailConfirmOK', true);
				self.accountData.attr('emailNotComplete', false);
				self.data.attr('emailNotComplete', false);
				
				
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
						regExpr: '^[a-z_][a-z0-9_-]{1,30}[$]?$',
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
						regExpr: '<i class="fa fa-times-circle"></i> The username is not valid.</span>',
						remote: '<i class="fa fa-times-circle"></i> This username is already taken, please choose another one.</span>'
					}
				},
				// set this class to error-labels to indicate valid fields
				success: function(label, element) {
					if ($(element).attr('name')=='Username')
						label.html('<span class="text-success"><i class="fa fa-check-circle"></i> The username is free and available.</span>');
				},

				submitHandler: function(form){
					var $username = $(form).find('input[name="Username"]');
					if ($username.length)
						bootbox.confirm('Your Cloud Username will be <b>'+$username.val()+'</b> and it cannot be changed. <br/>Are you sure?', function(confirmed){
							if (confirmed)
								self.profileSubmit();
						});
					else
						self.profileSubmit();
					return false;
				}
			});

			this.element.find('form.profileForm .UsernameNotSet').popover({
			trigger: 'focus',
			placement: 'left',
			title: 'Username',
			html: true,
			content: 'It must:<ul>'
				+'<li>begin with a lower case letter or an underscore</li>'
				+'<li>followed by lower case letters, digits, underscores, or dashes</li>'
				+'<li>have a maximum of 32 characters</li>'
				+'</ul>',
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
					self.data.attr({
						usernameNotSet: createdUser.Username == createdUser.Email
					});
					self.profileData.attr({
						saveSuccess: true,
						saveLoading: false, 
						profileNotComplete: !(createdUser.FirstName && createdUser.LastName && createdUser.Affiliation && createdUser.Country),
						usernameNotSet: createdUser.Username == createdUser.Email,
						nameMissing: !(createdUser.FirstName && createdUser.LastName),
					});
					var usernameDefault = (createdUser.Username == null || createdUser.Username == createdUser.Email);
					self.data.attr({
						profileNotComplete: !(createdUser.FirstName && createdUser.LastName && createdUser.Affiliation && createdUser.Country),
						usernameNotSet: usernameDefault
					});

				}).fail(function(xhr){
					self.profileData.attr({saveLoading: false, saveFail: true, saveFailMessage: Helpers.getErrMsg(xhr)});
				});
			
			return false;
		},
		
		'.settings-profile .signIn click': function(){
			App.Login.openLoginForm();
		},
		//createLdapAccount
		'.settings-account .createLdapAccount click': function(){
			var accountData = this.accountData;
			var data = this.data;
			
			var $modal = this.element.find('.settings-account .createLdapAccountModal').modal('show');
			
			if (!this.alreadyShowAccountForm){
				this.alreadyShowAccountForm = true;
				
				var $password = this.element.find('.createLdapAccountForm input[name="password"]')
					.popover(DEFAULT_PASSWORD_POPOVER_SETTINGS);
				
				this.element.find('.createLdapAccountForm').validate({
					rules: {
						password: DEFAULT_PASSWORD_RULES,
						password2: {
							equalTo: '[name="password"]',
						}
					},
					messages : {
						newpassword: DEFAULT_PASSWORD_MESSAGES,
						password2: 'Password does not match the confirm password.',
					},

					submitHandler: function(){
						var password = $password.val();
						$modal.modal('hide');
						accountData.attr('createLdapAccountloading', true);
						ProfileModel.createLdapAccount(password).then(function(usr){
							accountData.attr('createLdapAccountloading', false);
							accountData.attr('user',usr);
							data.attr('ldapCreated', true);
						}).fail(function(){
							bootbox.alert("<i class='fa fa-warning'></i> Password is empty.");
							accountData.attr('createLdapAccountloading', false);
						});
						return false;
					}
				});
			} else
				this.element.find('.createLdapAccountForm input').each(function(){
					$(this).val('').valid();
				})
		},

		// account
		initAccount: function(){
			var self = this;
			
			// change email form
			this.element.find('form.changeEmailForm').validate({
				rules: {
					Email: {
						required: true,
						email: true
					}
				},

				submitHandler: function(form){
					var email = $(form).find('input[name="Email"]').val();
					self.changeEmail(email);
					return false;
				}
			});
			
			// change password form
			this.element.find('form.changePasswordForm').validate({
				rules: {
					oldpassword: 'required',
					newpassword: DEFAULT_PASSWORD_RULES,
					newpassword2: {
						equalTo: '[name="newpassword"]',
					}
				},
				messages : {
					newpassword: DEFAULT_PASSWORD_MESSAGES,
					newpassword2: 'Password does not match the confirm password.',
				},

				submitHandler: function(form){
					self.changePassword();
				}
			});
			
			this.element.find('form.changePasswordForm input[name="newpassword"]').popover(DEFAULT_PASSWORD_POPOVER_SETTINGS);
		},

		changeEmail: function(email){
			var self = this;

			self.accountData.attr({
					emailLoading: true
				});

			ProfileModel.changeEmail(email).then(function(user){
				self.accountData.attr({
					emailLoading: false, 
					emailSaveSuccess: true,
					user: user
				});
			}).fail(function(xhr){
				self.accountData.attr({
					emailLoading: false, 
					emailSaveFail: true,
					emailErrorMessage: Helpers.getErrMsg(xhr, 'Unable to update your email. Please contact the Administrator.'),
				});
			});
		},

		changePassword : function(){
			var self = this;
			var oldpassword = Helpers.retrieveDataFromForm('.changePasswordForm','oldpassword');
			var newpassword = Helpers.retrieveDataFromForm('.changePasswordForm','newpassword');
			this.accountData.attr({
				passwordLoading: true, 
				passwordSaveSuccess: false,
				passwordSaveFail: false,
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
		
		//github
		'.settings-github .usernameForm .submit click': function(){
			var self= this;
			
			// get data
			var githubName = Helpers.retrieveDataFromForm('.modifyGithubName',	'GithubName');
			
			this.githubData.attr('loaded', false);
			// TODO check!
			this.githubPromise.then(function(data){

				// update
				new GithubModel({
						Name: githubName,
						Id: data.attr('Id'),
					})
					.save()
					.done(function(githubDataNew){
						self.githubData.attr(githubDataNew.attr(), true);
						self.githubData.attr('Name', githubName);
						self.githubData.attr('loaded', true);
					});
			});
			
			return false;
		},

		'.settings-github .usernameForm .cancel click': function(){
			$('.settings-github .modifyGithubName').hide();
			$('.settings-github .githubName').css('display', 'inline-block');
		},
		
		'.settings-github .showModifyGithubName click': function(){
			$('.settings-github .githubName').hide();
			$('.settings-github .modifyGithubName').removeClass('hide').show();
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
			var keyData = this.keyData;
			var githubData = this.githubData;
			var data = this.data;
			
			var subTitle = keyData.PublicKey ? 
					'Please note that this will overwrite your current SSH key pair.' : null;
			
			this.askPassword(subTitle).then(function(password){
				keyData.attr("loading",true);
            	SafeModel.create(password).then(function(safe){
					// setup keyData
					keyData.user.attr({
						PublicKey: safe.PublicKey,
						PrivateKey: safe.PrivateKey,
						PublicKeyBase64: btoa(safe.PublicKey),
						PrivateKeyBase64: btoa(safe.PrivateKey)
					});
					
					// setup github data
					if (githubData)
						githubData.attr('CertPub', safe.PublicKey);
					
					// setup global data
					data.attr({
						sshKeyNotComplete: false,
						githubNotComplete: true //we just create a new keys so it cannot be already on github
					});
				}).fail(function(){
            		bootbox.alert("<i class='fa fa-warning'></i> Error during ssh keys creation.");
            	}).always(function(){
            		keyData.attr("loading",false);
            	});
            });
		},

		'.settings-key .deletePublicKeyBtn click': function(){
			var keyData = this.keyData;
			var githubData = this.githubData;
			
			var subTitle = this.keyData.PublicKey ? 
					'Please note that this will overwrite your current SSH key pair from your VMs.' : null;
			
			this.askPassword(subTitle).then(function(password){
                keyData.attr("loading",true);
                SafeModel.delete(encodeURIComponent(password)).then(function(safe){
					if (githubData)
						githubData.attr({
							HasSSHKey: false,
							CertPub: null
						});
					
					if(keyData)
			    		keyData.attr({
			    			PublicKey: null,
			    			PrivateKey: null,
			    			PublicKeyBase64: null,
			    			PrivateKeyBase64: null,
			    		});
				}).fail(function(){
					bootbox.alert("<i class='fa fa-warning'></i> Error during ssh keys delete.");
				}).always(function(){
					keyData.attr("loading",false);
				});
			});
		},

		'.settings-apikey .showApiKeyBtn click': function(){
			var self = this;
			self.element.find('.apiKeyHidden').addClass('hidden');
			self.element.find('.apiKeyVisible').removeClass('hidden');
		},

		'.settings-apikey .hideApiKeyBtn click': function(){
			var self = this;
			self.element.find('.apiKeyVisible').addClass('hidden');
			self.element.find('.apiKeyHidden').removeClass('hidden');
		},

		'.settings-apikey .generateApiKey click': function(){
			var apikeyData = this.apikeyData;
			var data = this.data;
			var element = this.element;
			
			this.askPassword().then(function(password){
			    apikeyData.attr("loading",true);
			    ProfileModel.generateApiKey(password).then(function(apikey){
			    	data.attr({
						apiKeyNotComplete: false
					});

					element.find('.apiKeyVisible').addClass('hidden');
					element.find('.apiKeyHidden').removeClass('hidden');
					apikeyData.user.attr("ApiKey",apikey.Response);

				}).fail(function(){
					bootbox.alert("<i class='fa fa-warning'></i> Error during API key generation.");
				}).always(function(){
					apikeyData.attr("loading",false);
				});
			});
			
		},
		
		'.settings-apikey .getApiKey click': function(){
			var apikeyData = this.apikeyData;
			var element = this.element;
			
			this.askPassword().then(function(password){
				apikeyData.attr("loading",true);
				
				ProfileModel.getApiKey(encodeURIComponent(password)).then(function(apikey){
					
					element.find('.apiKeyVisible').addClass('hidden');
					element.find('.apiKeyHidden').removeClass('hidden');
					apikeyData.user.attr("ApiKey",apikey.Response);
				
				}).fail(function(){
					bootbox.alert("<i class='fa fa-warning'></i> Error during API key generation.");
				}).always(function(){
					apikeyData.attr("loading",false);
				});

			});

		},
		

		'.settings-apikey .revokeApiKeyBtn click': function(){
			var apikeyData = this.apikeyData;
			
			this.askPassword().then(function(password){
				apikeyData.attr("loading",true);
                ProfileModel.revokeApiKey(encodeURIComponent(password)).then(function(){
					apikeyData.user.attr("ApiKey",null);
				}).fail(function(){
					bootbox.alert("<i class='fa fa-warning'></i> Error during API key removal.");
				}).always(function(){
					apikeyData.attr("loading",false);
				});
				
			});
		},
		
		/* catalogue */
		loadCatalogueIndex: function(){
			var self = this;
			var catalogueData = this.catalogueData;

			this.isLoginPromise.then(function(user){
				catalogueData.attr('loading', true);
				ProfileModel.getCatalogueIndex().then(function(list){
					if (list && list.length)
						catalogueData.attr('list', list);
					catalogueData.attr('loading', false);
				}).fail(function(xhr){
					self.catalogueData.attr({
						loading: false,
						errorMessage: Helpers.getErrMsg(xhr, 'Error to load the catalogue index.')
					})
				});
			});

		},
		
		'.settings-catalogue .createCatalogueIndex click': function(){
			var self = this;
			
			this.catalogueData.attr('loading', true);
			ProfileModel.createCatalogueIndex().then(function(){
				self.loadCatalogueIndex();
			}).fail(function(xhr){
				self.catalogueData.attr({
					loading: false,
					errorMessage: Helpers.getErrMsg(xhr, 'Error to create the catalogue index.')
				})
			});
		},

		/* storage */
		loadStorage: function(){
			var self = this;
			var storageData = this.storageData;

			this.isLoginPromise.then(function(user){
				storageData.attr('loading', true);
				ProfileModel.getRepository().then(function(list){
					if (list && list.length)
						storageData.attr('list', list);
					storageData.attr('loading', false);
				}).fail(function(xhr){
					self.storageData.attr({
						loading: false,
						errorMessage: Helpers.getErrMsg(xhr, 'Error to load the Storage.')
					})
				});
			});

		},
		
		'.settings-storage .createStorage click': function(){
			var self = this;
			
			this.storageData.attr('loading', true);
			ProfileModel.createRepository().then(function(){
				self.loadStorage();
			}).fail(function(xhr){
				self.storageData.attr({
					loading: false,
					errorMessage: Helpers.getErrMsg(xhr, 'Error to create the Storage.')
				})
			});
		},

		/* features */
		loadFeatures: function(){
			var self = this;
			var featuresData = this.featuresData;

			this.isLoginPromise.then(function(user){
				featuresData.attr('loading', true);
				ProfileModel.getFeatures().then(function(list){
					if (list && list.length)
						featuresData.attr('list', list);
					featuresData.attr('loading', false);
				}).fail(function(xhr){
					self.featuresData.attr({
						loading: false,
						errorMessage: Helpers.getErrMsg(xhr, 'Error to load the user features.')
					})
				});;
			});

		},
		
		'.settings-features .createFeatures click': function(){
			var self = this;
			
			this.featuresData.attr('loading', true);
			ProfileModel.createFeatures().then(function(){
				self.loadFeatures();
			}).fail(function(xhr){
				self.featuresData.attr({
					loading: false,
					errorMessage: Helpers.getErrMsg(xhr, 'Error to create the user features.')
				})
			});
		},
		
		askPassword: function(subTitle){
			var deferred = new can.Deferred(); // askPassword is a deferred function
			
			// check if token is available: if yes, use token as password
			var token = $.cookie('t2-sso-externalTokenAccess');
			if (token)
				deferred.resolve(token);
			
			// otherwise ask password from the modal form				
			else {
				var $dialog;
				var title = 'This action requires your password.'
					+ (subTitle ? '<br/><small><i>'+subTitle+'</i></small>' : '');
				
				var message = "<div class='container-fluid'>"
					+ "<form class='askPasswordForm'>"
					+ "<div class='form-group'>" 
					+ "<label for='password'>Password</label>"
					+ "<input type='password' class='form-control' name='password' placeholder='Password'>"
					+ "</div>"
					+ "</form>"
					+ "</div>";
				
				var submitFormCallback = function(){
					var password = $dialog.find('input[name="password"]').val();
					if (password){
						try{
							deferred.resolve(password); // password sent
						} catch(e){
							window.e=e;
							console.log(e.stack);
							return false;
						}
					}
					else {
						bootbox.alert("<i class='fa fa-warning'></i> Password is empty.");
						return false;
					}
				};
				
				$dialog = bootbox.dialog({
					title: title,
					message: message,
					buttons: {
						success: {
							label: "OK",
							className: "btn-default",
							callback: submitFormCallback
						}
					}
				});
				
				$dialog.find('form').submit(function(){
					submitFormCallback();
					$dialog.modal('hide');
					return false;
				});
			}
			
			return deferred;
		},

	}
);
	
return new SettingsControl(Config.mainContainer, {});
	
});
