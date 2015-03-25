
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
	'jasnyBootstrap',//'bootstrapFileUpload',
	'jqueryValidate',
	'ajaxFileUpload',
	'droppableTextarea'
], function($, can, bootbox, BaseControl, Config, Helpers, ProfileModel, CertificateModel, OneConfigModel, GithubModel, OneUserModel){
	var SettingsControl = BaseControl(
		{
			defaults: { fade: 'slow' },
		},{
			
			// init
			init: function (element, options) {
				var self = this;
				console.log("settingsControl.init");
				
				this.params = Helpers.getUrlParameters();
				this.data = new can.Observe({});
				this.isLoginPromise = App.Login.isLoggedDeferred;
				this.githubPromise = GithubModel.findOne();
				this.configPromise = $.get('/'+Config.api+'/config?format=json');
				
				this.isLoginPromise.then(function(user){
					self.data.attr({
						user: user,
						isPending: (user.AccountStatus==1),
						emailConfirmOK: user.AccountStatus>1 && self.params.emailConfirm=='ok',
						showGithub: (user.DomainId!=0 && user.AccountStatus!=1),
						showCloud: (user.DomainId!=0 && user.AccountStatus!=1),
					});
				}).fail(function(){
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
			
			cloud: function(options) {
				var self = this;
				console.log("App.controllers.Settings.cloud");
				var cloudData = new can.Observe({});
				this.isLoginPromise.then(function(user){
					OneConfigModel.findAll().then(function(_oneSettings){
						oneSettings = Helpers.keyValueArrayToJson(_oneSettings);
						self.view({
							url: 'modules/settings/views/cloud.html',
							selector: Config.subContainer,
							data: cloudData,
							dependency: self.indexDependency(),
							fnLoad: function(){
								self.initSubmenu('cloud');
							}
						});
						OneUserModel.findOne().then(function(oneUser){
							cloudData.attr({
								oneSettings: oneSettings,
								oneUser: oneUser,
								sunstoneOk: user.CertSubject == oneUser.Password,
								user: user,
								onePasswordOk: oneUser.Password,
								oneCertOk: user.CertSubject
							})						
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
							dependency: self.indexDependency(),
							data: {
								user: userData,
								github: githubData,
							},
							fnLoad: function(){
								self.initSubmenu('github');
								self.initSshKeyArea();
							}
						});
					});
				});
			},

			freeTrial: function(options) {
				var self = this;
				console.log("App.controllers.Settings.freeTrial");
				self.isLoginPromise.then(function(userData){
					self.view({
						url: 'modules/settings/views/freeTrial.html',
						selector: Config.subContainer,
						dependency: self.indexDependency(),
						data: {
							user: userData,
						},
						fnLoad: function(){
							self.initSubmenu('freeTrial');
						}
					});
				});
			},

			earlyBird: function(options) {
				var self = this;
				console.log("App.controllers.Settings.earlyBird");
				self.isLoginPromise.then(function(userData){
					self.view({
						url: 'modules/settings/views/earlyBird.html',
						selector: Config.subContainer,
						dependency: self.indexDependency(),
						data: {
							user: userData,
						},
						fnLoad: function(){
							self.initSubmenu('earlyBird');
						}
					});
				});
			},

			partnerProgram: function(options) {
				var self = this;
				console.log("App.controllers.Settings.partnerProgram");
				self.isLoginPromise.then(function(userData){
					self.view({
						url: 'modules/settings/views/partnerProgram.html',
						selector: Config.subContainer,
						dependency: self.indexDependency(),
						data: {
							user: userData,
						},
						fnLoad: function(){
							self.initSubmenu('partnerProgram');
						}
					});
				});
			},

			plan: function(options) {
				var self = this;
				console.log("App.controllers.Settings.plan");
				self.isLoginPromise.then(function(userData){
					self.view({
						url: 'modules/settings/views/plan.html',
						selector: Config.subContainer,
						dependency: self.indexDependency(),
						data: {
							user: userData,
						},
						fnLoad: function(){
							self.initSubmenu('plan');
						}
					});
				});
			},

			initSshKeyArea:function(){
				var githubData = this.githubData;
					
				function updateView(){
					var $dta = $('#myDroppableTextarea');
					if ($dta.length && $dta.is(':empty')){
						$dta.droppableTextarea({
							limitByte: 1000,
							limitByteMessage: 'The file is too big, are you sure you\'ve dropped your public key?',
							placeholder: 'ssh-rsa ...',
							changeCallback: function(text){
								if (!text || !text.startsWith('ssh-rsa ')){
									$('.rsaValidation.alert').removeClass('alert-success').html('<i class="icon-exclamation-sign"></i> Insert a valid rsa public key.');
									$('.settings-github .addPublicKeyFromTextarea').attr('disabled', 'disabled');
								}
								else{
									$('.settings-github .addPublicKeyFromTextarea').removeAttr('disabled');
									$('.rsaValidation.alert').addClass('alert-success').html('<i class="icon-check-sign"></i> Valid public key inserted.');
								}
							}
						});
					}
				};
				updateView();			
				githubData.bind('change', function(){
					updateView();
				});

				if (this.params.code && this.params.state && this.params.state=='geohazardstep'){
					$('.githubKeyPanel').mask('wait');
					GithubModel.getGithubToken(this.params.code, function(){
						$('.githubKeyPanel').unmask();
						self.addPublicKey();
					},function(){
						$('.githubKeyPanel').unmask();
						bootbox.alert("<i class='fa fa-warning'></i> Error during put your GitHub token.");
					});
				}
			},
			
			
			
			// profile
			
			showPendingActivation: function(){
				this.element.find('.pendingActivation').show();
			},

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
					
//					App.Login.init(document, { showLoginMenu: true, pendingActivationOk: true });
//					self.tokenOk = true;
//					self.init(self.element, self.options);
//					can.route.removeAttr('token');
//					bootbox.alert('<h3><strong>Thank you.</strong></h3><h4>Your email address has been successfully validated.</h4>', function(){
//					});
					
//					$('nav>.topPageAlert')
//						.html(can.view("modules/settings/views/pendingActivationSuccess.html"))
//						.show('blind');
					
				}).fail(function(xhr){
					//App.Login.showPendingActivation();
					self.errorView({}, 'Unable to get the token.', Helpers.getErrMsg(xhr), true);
				});
			},
			
			'.settings-profile .submit click': function(){
				// get data
				var self= this,
					usr = Helpers.retrieveDataFromForm('.settings-profile form',
						['FirstName','LastName','Email','Affiliation','Country','EmailNotification']);
				
				// update
				App.Login.User.current.attr(usr); 
				// save
				self.profileData.attr({saveSuccess: false, saveFail: false});
				new ProfileModel(App.Login.User.current.attr())
					.save()
					.then(function(createdNews){
						self.profileData.attr('saveSuccess', true);
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

			'.githubKeyPanel > .showSshKey click': function(el){
				var $pre = $(".githubKeyPanel > .certPub");
				if ($pre.is(':visible')){
					$pre.hide('blind');
					el.html('Show SSH Key <i class="icon-caret-right"></i>');
				} else{
					$pre.show('blind');
					el.html('Hide SSH Key <i class="icon-caret-down"></i>');
				}
				return false;
			},

			'.settings-github .addPublicKeyFromTextarea click': function(){
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
				$('.settings-github .modifyGithubName').show();
			},
			
			'.settings-github .githubKeyPanel .addPublicKey click': 'addPublicKey',

			addPublicKey: function(sshPublicKey){
				var self = this;
				
				this.configPromise.then(function(_serviceConfig){
					var serviceConfig = Helpers.keyValueArrayToJson(_serviceConfig, 'Key', 'Value');

					$('.githubKeyPanel').mask('wait');
					GithubModel.postSshKey(sshPublicKey, function(){
						$('.githubKeyPanel').unmask();
						self.githubData.attr('HasSSHKey', 'true');
					}, function(xhr){
						$('.githubKeyPanel').unmask();
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
			}

		}
	);
		
	return new SettingsControl(Config.mainContainer, {});
	
});
