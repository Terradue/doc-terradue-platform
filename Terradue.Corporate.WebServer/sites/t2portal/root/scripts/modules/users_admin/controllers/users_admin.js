
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
	'datePicker'
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

		'.upgradePlanBtn click': function(data){
			var self = this,
				selectedPlanId = this.element.find('input[name="planRadio"]:checked').val(),
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
				
				PlansModel.upgrade({
 					Id: userid,
 					Plan: plan.Key,
 				}).then(function(){
					self.userData.attr('planUpgradedSuccessName', plan.Key);
 				}).fail(function(xhr){
 					errXhr=xhr; // for debug
 					self.userData.attr('planUpgradedFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
 				});
			}
		}
		
	});
	
	return new UsersAdminControl(Config.mainContainer, {
		Model: UsersAdminModel,
		entityName: 'users',
		view: '/scripts/modules/users_admin/views/users_admin.html',
		insertAtBeginning: true,
		loginDeferred: App.Login.isLoggedDeferred,
		adminAccess: true
	});
	
});
