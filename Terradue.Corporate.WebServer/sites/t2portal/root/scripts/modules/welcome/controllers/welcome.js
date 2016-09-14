
define([
	'jquery',
	'can',
	'bootbox',
	'utils/baseControl',
	'configurations/settings',
	'utils/helpers',
	//'canpromise',
	'messenger',
	'jasnyBootstrap',//'bootstrapFileUpload',
	'jqueryValidate',
	'ajaxFileUpload',
	'droppableTextarea',
	'jqueryCopyableInput',
	'latinise'
], function($, can, bootbox, BaseControl, Config, Helpers){
	
	var WelcomeControl = BaseControl(
		{
			defaults: { fade: 'slow' },
		},{
			
			// init
			init: function (element, options) {
				console.log("welcomeControl.init");
				var self = this;
				self.isLoginPromise = App.Login.isLoggedDeferred;
				self.profileData = new can.Observe({
					loading: true
				});
				self.view({
					url: 'modules/welcome/views/welcome.html',
					data: self.profileData
				});
			},

			// actions
			index: function(options) {
				var self = this;
				console.log("App.controllers.Welcome");

				// first wait user is ready
				this.isLoginPromise.then(function(user){
					var usernameDefault = (user.Username == null || user.Username == user.Email);
					var accountEnabled = user.AccountStatus == 4;
					var profilecomplete = user.FirstName && user.LastName && user.Affiliation && user.Country;//default profile set ?
					profilecomplete = profilecomplete && (user.AccountStatus == 4);//email validated ?
					//profilecomplete = profilecomplete && user.PublicKey;//ssh key added ?
					var userPlanGtExplorer = (user.Plan == "Explorer" || user.Plan == "Scaler" || user.Plan == "Premium");
					var apikeyNull = user.ApiKey == null;
					self.profileData.attr({
						user: user,
						loading: false,
						usernameNotSet: usernameDefault,
						emailNotValidated: !accountEnabled,
						profileNotComplete: !profilecomplete,
						apikeyNotComplete: userPlanGtExplorer && apikeyNull
					});

				}).fail(function(){
					self.accessDenied();
				});
			},
			
			// overwrite accessDenied default function
			accessDenied: function(){
				document.location = '/t2api/oauth'
			}

			
		}
	);
		
	return new WelcomeControl(Config.mainContainer, {});
	
});
