
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
			var id = data.id;
			
			this.userData = new can.Observe({});
			
			// get user info
			UsersAdminModel.findOne({id:id}).then(function(user){
				self.userData.attr('user', user);
			}).fail(function(){
				this.errorView({}, 'Unable to get user info', 'The user doesn\'t exist or you can\'t access this page.', true);
			});
			
			// get plans
			PlansModel.findAll().then(function(plans){
				self.userData.attr('plans', plans);
			}).fail(function(){
				this.errorView({}, 'Unable to get plans info', null, true);
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
					return (plan.Value==selectedPlanId);
				}),
				plan = (planSearch.length ? planSearch[0] : null),
				userid = data.data('user');
			
			if (plan && userid){
				// save plan
				this.userData.attr({
					planUpgradedLoading: true, planUpgradedSuccessName:null, planUpgradedFailMessage:null
				});

				bootbox.confirm('Upgrade user <b>'+self.userData.attr('user').Username+'</b> to plan <b>'+plan.Key+'</b>.<br/>Are you sure?', function(confirmed){
					if (confirmed)
						PlansModel.upgrade({
		 					Id: userid,
		 					Plan: plan.Key,
		 				}).then(function(){
							self.userData.attr('planUpgradedSuccessName', plan.Key);
		 				}).fail(function(xhr){
		 					errXhr=xhr; // for debug
		 					self.userData.attr('planUpgradedFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
		 				});
		 		});
			}
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
