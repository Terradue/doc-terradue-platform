
define([
	'jquery',
	'can',
	'bootbox',
	'utils/crudBaseModule/crudBaseControl',
	'config',
	'utils/helpers',
	'modules/users_admin/models/users_admin',
	'modules/users_admin/models/plans',
	'messenger',
	'summernote',
	'datePicker',
	'dataTables'
], function($, can, bootbox, CrudBaseControl, Config, Helpers, UsersAdminModel, PlansModel){
	
	var UsersAdminControl = CrudBaseControl({}, {
		
		onIndex: function(element, options){
			var self = this;
		},
		
		onEntitySelected: function(users){
			Helpers.scrollTop();
		},
		
		onCreateClick: function(){
			Helpers.scrollTop();
		},

		details: function(data){
			console.log(data);
			var self = this;
			self.id = data.id;
			
			this.userData = new can.Observe({});
			
			// get user info
			UsersAdminModel.findOne({id:self.id}).then(function(user){
				self.userData.attr('user', user);
				UsersAdminModel.getRepositories(self.id).then(function(repositories){
					self.userData.user.attr('repositories', repositories);	
				}).fail(function(){
					self.errorView({}, 'Unable to get user repositories', 'The user doesn\'t exist or you can\'t access this page.', true);
				});
				if(user.RegistrationOrigin != null){
					var originIconUrl = "";
					switch(user.RegistrationOrigin){
						case "GEP":
						originIconUrl = "https://geohazards-tep.eo.esa.int/styles/img/logo-geohazard.png";
						break;
						case "HEP":
						originIconUrl = "https://hydrology-tep.eo.esa.int/styles/img/logo-hydro.png";
						break;
						case "UTEP":
						originIconUrl = "https://urban-tep.eo.esa.int/styles/img/icons/logo_tep_urban.png";
						break;
					}
					self.userData.user.attr('RegistrationOrigin', originIconUrl);
				}
			}).fail(function(){
				self.errorView({}, 'Unable to get user info', 'The user doesn\'t exist or you can\'t access this page.', true);
			});
			
			// get plans
			PlansModel.findAll().then(function(plans){
				self.userData.attr('plans', plans);
			}).fail(function(){
				self.errorView({}, 'Unable to get plans info', null, true);
			});

			// load view
			this.view({
				url: 'modules/users_admin/views/details.html',
				data: this.userData
			});
		},



		'.list-group-item click': function(el){
			var self = this;
			el.parent().find('a').removeClass('active');
			el.addClass('active');
		},

		'.upgradePlanBtn click': function(data){
			var self = this,
				selectedPlanId = this.element.find('a[class="user-plan list-group-item active"]').data('plan'),
				plans = this.userData.plans,
				planSearch = $.grep(plans, function(plan){
					return (plan.Id==selectedPlanId);
				}),
				plan = (planSearch.length ? planSearch[0] : null),
				userid = data.data('user');
			
			if (plan && userid){
				// save plan

				bootbox.confirm('Upgrade user <b>'+self.userData.attr('user').Username+'</b> to plan <b>'+plan.Name+'</b>.<br/>Are you sure?', function(confirmed){
					if (confirmed){
						self.userData.attr({
							planUpgradedLoading: true, planUpgradedSuccessName:null, planUpgradedFailMessage:null
						});
						PlansModel.upgrade({
		 					Id: userid,
		 					Plan: plan.Name,
		 				}).then(function(user){
		 					self.userData.attr('user', user);
							self.userData.attr('planUpgradedSuccessName', plan.Name);
		 				}).fail(function(xhr){
		 					errXhr=xhr; // for debug
		 					self.userData.attr('planUpgradedFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
		 				});
		 			}
		 		});
			}
		},

		'.createCatalogueIndex click': function(data){
			var self = this,
				userid = data.data('user');
			
			if (userid){

				bootbox.confirm('Create catalogue index for user <b>'+self.userData.attr('user').Username+'</b>.<br/>Are you sure?', function(confirmed){
					if (confirmed){
						self.userData.attr('catalogueLoading', true);
						self.userData.attr('catalogueFailMessage', null);
						UsersAdminModel.createCatalogueIndex({
		 					Id: userid
		 				}).then(function(data){
		 					self.userData.attr('catalogueLoading', false);
		 					if(data)
								self.userData.user.attr('HasCatalogueIndex', true);
							else
								self.userData.attr('catalogueFailMessage', "Unable to create the index");
		 				}).fail(function(xhr){
		 					errXhr=xhr; // for debug
		 					self.userData.attr('catalogueLoading', false);
		 					self.userData.attr('catalogueFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
		 				});
		 			}
		 		});
			}
		},

		'.createLdapDomain click': function(data){
			var userData = this.userData,
				userid = data.data('user');
			
			if (userid)
				bootbox.confirm('Create Ldap domain for user <b>' + userData.attr('user').Username + '</b>.<br/>Are you sure?', function(confirmed){
					if (confirmed){
						userData.attr('domainLoading', true);

						UsersAdminModel.createLdapDomain({
		 					Id: userid
		 				}).then(function(){
							userData.user.attr('HasLdapDomain', true);
							userData.user.attr('ArtifactoryDomainExists', true);
							userData.attr('domainLoading', false);
		 				}).fail(function(xhr){
		 					errXhr=xhr; // for debug
		 					userData.attr('storageFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
		 					userData.attr('domainLoading', false);
		 				});
					}
		 		});
		},

		'.createArtifactoryDomain click': function(data){
			var userData = this.userData,
				userid = data.data('user');
			
			if (userid)
				bootbox.confirm('Create Storage domain for user <b>' + userData.attr('user').Username + '</b>.<br/>Are you sure?', function(confirmed){
					if (confirmed){
						userData.attr('domainLoading', true);

						UsersAdminModel.createArtifactoryDomain({
		 					Id: userid
		 				}).then(function(){
		 					userData.user.attr('HasLdapDomain', true);
							userData.user.attr('ArtifactoryDomainExists', true);
							userData.attr('domainLoading', false);
		 				}).fail(function(xhr){
		 					errXhr=xhr; // for debug
		 					userData.attr('storageFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
		 					userData.attr('domainLoading', false);
		 				});
					}
		 		});
		},

		'.createRepository click': function(data){
			var userData = this.userData,
				userid = data.data('user');
			
			if (userid)
				bootbox.confirm('Create Storage repository for user <b>' + userData.attr('user').Username + '</b>.<br/>Are you sure?', function(confirmed){
					if (confirmed){
						userData.attr('repositoryLoading', true);
						UsersAdminModel.createRepository({
							id: userid
						}).then(function(repositories){
							userData.user.attr('repositories', repositories);
							userData.attr('repositoryLoading', false);
						}).fail(function(xhr){
							errXhr=xhr; // for debug
							userData.attr('storageFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
							userData.attr('repositoryLoading', false);
						});
					}
		 		});
		},

		'.validateEmail click': function(data){
			var self = this;
			var userid = data.data('user');
			var currentUser = self.userData.attr('user');

			bootbox.confirm('Validate account for user <b>'+currentUser.Username+'</b>.<br/>Are you sure?', function(confirmed){
				if (confirmed){
					currentUser.attr('AccountStatus',4);
					self.userData.attr({
						validateAccountLoading: true, validateAccountFailMessage:null
					});
					currentUser.save().then(function(user){
						self.userData.attr({
							validateAccountLoading: false, validateAccountFailMessage:null
						});
						self.userData.attr('user', user);
					}).fail(function(xhr){
						currentUser.attr('AccountStatus',1);
						self.userData.attr({
							validateAccountLoading: false, validateAccountFailMessage: Helpers.getErrMsg(xhr, 'Generic Error')
						});
					});
				}
			});

		},

		'.giveAdminRights click': function(data){
			var self = this;
			var userid = data.data('user');
			var currentUser = self.userData.attr('user');

			bootbox.confirm('Set user <b>'+currentUser.Username+'</b> as Administrator.<br/>Are you sure?', function(confirmed){
				if (confirmed){
					currentUser.attr('Level',4);
					self.userData.attr({
						userAdminLoading: true, userAdminFailMessage:null
					});
					currentUser.save().then(function(user){
						self.userData.attr({
							userAdminLoading: false, userAdminFailMessage:null
						});
						self.userData.attr('user', user);
					}).fail(function(xhr){
						currentUser.attr('Level',3);
						self.userData.attr({
							userAdminLoading: false, userAdminFailMessage: Helpers.getErrMsg(xhr, 'Generic Error')
						});
					});
				}
			});

		},

		'.removeAdminRights click': function(data){
			var self = this;
			var userid = data.data('user');
			var currentUser = self.userData.attr('user');

			bootbox.confirm('Set user <b>'+currentUser.Username+'</b> as normal user.<br/>Are you sure?', function(confirmed){
				if (confirmed){
					currentUser.attr('Level',3);
					self.userData.attr({
						userAdminLoading: true, userAdminFailMessage:null
					});
					currentUser.save().then(function(user){
						self.userData.attr({
							userAdminLoading: false, userAdminFailMessage:null
						});
						self.userData.attr('user', user);
					}).fail(function(xhr){
						currentUser.attr('Level',4);
						self.userData.attr({
							userAdminLoading: false, userAdminFailMessage: Helpers.getErrMsg(xhr, 'Generic Error')
						});
					});
				}
			});

		},

		'.updateT2Username click': function(data){
			var self = this;
			self.userData.attr('updatet2usernameloading', true);
			var currentUser = self.userData.attr('user');
			currentUser.attr('Username', data.parent().find('input').val());
			UsersAdminModel.updateT2username(currentUser).then(function(usert2){
				self.userData.user.attr('Username',usert2.Username);
				self.userData.attr('updatet2usernameloading', false);
				self.userData.attr('updatet2usernamesuccess', true);
			}).fail(function(xhr){
				self.userData.attr('updatet2usernameloading', false);	
			});
		}
		
	});
	
	return new UsersAdminControl(Config.mainContainer, {
		Model: UsersAdminModel,
		entityName: 'users',
		view: '/scripts/modules/users_admin/views/users_admin.html',
		insertAtBeginning: true,
		loginDeferred: App.Login.isLoggedDeferred,
		adminAccess: true,
		msgDelete: 'Are you sure you want to delete this user?<br/>The user will be removed from LDAP and from the Cloud Controller.',
		dataTable: true
	});
	
});
