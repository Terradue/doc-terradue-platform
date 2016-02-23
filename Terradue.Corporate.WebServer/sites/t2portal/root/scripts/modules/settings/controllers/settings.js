
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
	'zeroClipboard',
	'canpromise',
	'messenger',
	'jasnyBootstrap',//'bootstrapFileUpload',
	'jqueryValidate',
	'ajaxFileUpload',
	'droppableTextarea',
	'jqueryCopyableInput'
], function($, can, bootbox, BaseControl, Config, Helpers, ProfileModel, CertificateModel, OneConfigModel, GithubModel, OneUserModel, SafeModel, PlansModel, ZeroClipboard){
	
	window.ZeroClipboard = ZeroClipboard;
	
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
				this.isLoginPromise = App.Login.isLoggedDeferred;
				this.githubPromise = GithubModel.findOne();
				this.configPromise = $.get('/'+Config.api+'/config?format=json');

				this.isLoginPromise.then(function(user){
					var downloadData = new can.Observe({});
					downloadData.attr('PublicKeyBase64', btoa(user.PublicKey));
					self.githubPromise.then(function(githubData){
						self.data.attr({
							user: user,
							github: githubData,
							download: downloadData,
							isPending: (user.AccountStatus==1),
							emailConfirmOK: user.AccountStatus>1 && self.params.emailConfirm=='ok',
							showSafe: (user.DomainId!=0 && user.AccountStatus!=1),
							showGithub: (user.DomainId!=0 && user.AccountStatus!=1),
							showCloud: (user.DomainId!=0 && user.AccountStatus!=1),
							profileNotComplete: !(user.FirstName && user.LastName && user.Affiliation && user.Country)

						});
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
				})
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
					self.profileData.attr({
						user: user,
						isPending: (user.AccountStatus==1 && self.params.registered!='ok'),
						isNewPending: (user.AccountStatus==1 && self.params.registered=='ok'),
						emailConfirmOK: user.AccountStatus>1 && self.params.emailConfirm=='ok',
						profileNotComplete: !(user.FirstName && user.LastName && user.Affiliation && user.Country)
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
						isPending: (user.AccountStatus==1 && self.params.registered!='ok'),
						isNewPending: (user.AccountStatus==1 && self.params.registered=='ok'),
						emailConfirmOK: user.AccountStatus>1 && self.params.emailConfirm=='ok',
						profileNotComplete: !(user.FirstName && user.LastName && user.Affiliation && user.Country)
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

					if (self.params.code && self.params.state && self.params.state=='geohazardstep'){
						GithubModel.getGithubToken(this.params.code, function(){
						},function(){
							bootbox.alert("<i class='fa fa-warning'></i> Error during put your GitHub token.");
						});
					}

					self.githubPromise.then(function(githubData){
						self.githubData = githubData;
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
				self.isLoginPromise.then(function(userData){
					var downloadData = new can.Observe({});
					downloadData.attr('PublicKeyBase64', btoa(userData.PublicKey));
					self.view({
						url: 'modules/settings/views/key.html',
						selector: Config.subContainer,
						dependency: self.indexDependency(),
						data: {
							user: userData,
							download: downloadData
						},
						fnLoad: function(){
							self.initSubmenu('key');
							
							self.element.find('.copyPrivateKeyBtn').copyableInput(userData.PublicKey, {
								isButton: true,
							});
						}
					});
				});
			},

//			plan: function(options) {
//				var self = this;
//				console.log("App.controllers.Settings.plan");
//				self.isLoginPromise.then(function(userData){
//					self.planPromise.then(function(plansData){
//						self.view({
//							url: 'modules/settings/views/plan.html',
//							selector: Config.subContainer,
//							dependency: self.indexDependency(),
//							data: {
//								user: userData,
//								plans: plansData,
//								planStatus: self.planStatus
//							},
//							fnLoad: function(){
//								self.initSubmenu('plan');
//							}
//						});
//					});
//				});
//			},

//			initSshKeyArea:function(){
//				var githubData = this.githubData;
//					
//				function updateView(){
//					var $dta = $('#myDroppableTextarea');
//					if ($dta.length && $dta.is(':empty')){
//						$dta.droppableTextarea({
//							limitByte: 1000,
//							limitByteMessage: 'The file is too big, are you sure you\'ve dropped your public key?',
//							placeholder: 'ssh-rsa ...',
//							changeCallback: function(text){
//								if (!text || !text.startsWith('ssh-rsa ')){
//									$('.rsaValidation.alert').removeClass('alert-success').html('<i class="icon-exclamation-sign"></i> Insert a valid rsa public key.');
//									$('.settings-github .addPublicKeyFromTextarea').attr('disabled', 'disabled');
//								}
//								else{
//									$('.settings-github .addPublicKeyFromTextarea').removeAttr('disabled');
//									$('.rsaValidation.alert').addClass('alert-success').html('<i class="icon-check-sign"></i> Valid public key inserted.');
//								}
//							}
//						});
//					}
//				};
//				updateView();			
//				githubData.bind('change', function(){
//					updateView();
//				});
//
//				if (this.params.code && this.params.state && this.params.state=='geohazardstep'){
//					$('.githubKeyPanel').mask('wait');
//					GithubModel.getGithubToken(this.params.code, function(){
//						$('.githubKeyPanel').unmask();
//						self.addPublicKey();
//					},function(){
//						$('.githubKeyPanel').unmask();
//						bootbox.alert("<i class='fa fa-warning'></i> Error during put your GitHub token.");
//					});
//				}
//			},
			
			
			
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
			
			'.settings-profile .submit click': function(){
				// get data
				var self= this,
					usr = Helpers.retrieveDataFromForm('.settings-profile form',
						['FirstName','LastName','Affiliation','Country','EmailNotification']);
				
				// update
				App.Login.User.current.attr(usr); 
				// save
				self.profileData.attr({saveSuccess: false, saveFail: false});
				new ProfileModel(App.Login.User.current.attr())
					.save()
					.then(function(createdUser){
						self.profileData.attr({
							saveSuccess: true,
							profileNotComplete: !(createdUser.FirstName && createdUser.LastName && createdUser.Affiliation && createdUser.Country)
						});
						self.data.attr({
							profileNotComplete: !(createdUser.FirstName && createdUser.LastName && createdUser.Affiliation && createdUser.Country)
						});

					}).fail(function(xhr){
						self.profileData.attr({saveFail: true, saveFailMessage: Helpers.getErrMsg(xhr)});
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

			'.settings-github .addPublicKeyToGithub click': function(){
				var sshPublicKey = $('#myDroppableTextarea textarea').val();
				if (!sshPublicKey)
					bootbox.alert('Your public key is empty');
				else if (!sshPublicKey.startsWith('ssh-rsa '))
					bootbox.alert('Your public key is not well-formed');
				else{
					var secondSpaceIndex = sshPublicKey.substring(8).indexOf(' ');
					if (secondSpaceIndex!=-1)
						sshPublicKey = sshPublicKey.substring(0, secondSpaceIndex+8);
					
					this.addPublicKey(sshPublicKey);
				}
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

					$('.githubKeyPanel').mask('wait');
					GithubModel.postSshKey()
					.then(function(){
						self.githubData.attr('HasSSHKey', 'true');
					})
					.fail(function(xhr){
						if (!xhr.responseJSON)
							xhr.responseJSON = JSON.parse(xhr.responseText)
							if (xhr.responseJSON.ResponseStatus.Message == "Invalid token"){
								// get github client id
								var githubClientId = serviceConfig['Github-client-id'];
								if (!githubClientId)
									return;
								
								// redirect to github
								window.location = 'https://github.com/login/oauth/authorize?client_id='+githubClientId
									+'&scope=write:public_key,repo&state=geohazardstep&redirect_uri=' + document.location.href
								
							}
					});
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
				}, function(xhr){
					console.error(xhr);
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
					}, function(xhr){
						console.error(xhr);
					});
				});
			},

			/* safe */
			'.settings-key .generateSafe click': function(){
				var self = this;
				var message = "<div class='container-fluid'>"
							+ "<form class='createSafeForm'>"
							+ "<div class='form-group'>" 
							+ "<label for='password'>Password</label>"
							+"<input type='password' class='form-control' name='password' id='safePassword' placeholder='Password'>"
							+ "</div>"
							+ "<div class='form-group'>"
							+ "<label for='passwordRepeat'>Password confirmation</label>"
							+ "<input type='password' class='form-control' id='safePasswordRepeat' name='passwordRepeat' placeholder='Password confirmation'>"
							+ "</div>"
							+ "</form>"
							+ "</div>";
				bootbox.dialog({
					title: "Please enter your new ssh key password",
					message: message,
					buttons: {
	                    success: {
	                        label: "OK",
	                        className: "btn-default",
	                        callback: function (a,b,c) {
	                            var password = $('#safePassword').val();
	                            var passwordRepeat = $('#safePasswordRepeat').val();
	                            if (password!=passwordRepeat || password==''){
	                            	bootbox.alert("<i class='fa fa-warning'></i> Password and Password Confirmation must be the same.");
	                            	//self.element.find('.createSafeForm .text-error').show();
	                            	return false;
	                            };
	                            
	                            SafeModel.create(password).then(function(safe){
							    	self.data.user.attr("PublicKey",safe.PublicKey);
							    	self.data.user.attr("PrivateKey",safe.PrivateKey);
							    	self.data.download.attr("PublicKeyBase64",btoa(safe.PublicKey));
							    	self.data.download.attr("PrivateKeyBase64",btoa(safe.PrivateKey));
							    	$('.noKey').mask();
									$('.hasKey').unmask();
								}).fail(function(){
									bootbox.alert("<i class='fa fa-warning'></i> Error during safe creation.");
								});
                        	}
                    	}
                    }
				});
			},

			'.settings-safe .getPrivateKeyBtn click': function(){
				var self = this;
				bootbox.prompt({
					title: "Please enter your Safe password",
	  				inputType: "password",	
	  				callback: function(result) {             
					  if (result === null) {                                             
					    
					  } else {
					    SafeModel.get(result).then(function(safe){
					    	self.data.user.attr("PublicKey",safe.PublicKey);
					    	self.data.user.attr("PrivateKey",safe.PrivateKey);
					    	$('.noKey').mask();
							$('.hasKey').unmask();

						}).fail(function(xhr){
							bootbox.alert("<i class='fa fa-warning'></i> Error during safe creation: " + Helpers.getErrMsg(xhr));
						});
					  }
					}
				});
			},

			/*plan*/
			'.plan-profile .upgradePlanBtn click': function(){
				var self = this,
				selectedPlanId = this.element.find('input[name="planRadio"]:checked').val();
				self.isLoginPromise.then(function(userData){
					self.planPromise.then(function(plansData){
						var planSearch = $.grep(plansData, function(plan){
							return (plan.Value==selectedPlanId);
						}),
						plan = (planSearch.length ? planSearch[0] : null);
				
						if (plan && userData){
							// save plan
							self.planStatus.attr({
								planUpgradedLoading: true, planUpgradedSuccessName:null, planUpgradedFailMessage:null
							});
							
							PlansModel.upgrade({
								Id: userData.Id,
			 					Level: selectedPlanId
			 				}).then(function(){
								self.planStatus.attr('planUpgradedSuccessName', plan.Key);
			 				}).fail(function(xhr){
			 					errXhr=xhr; // for debug
			 					self.planStatus.attr('planUpgradedFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
			 				});
						}
					});
				});
			}

		}
	);
		
	return new SettingsControl(Config.mainContainer, {});
	
});
