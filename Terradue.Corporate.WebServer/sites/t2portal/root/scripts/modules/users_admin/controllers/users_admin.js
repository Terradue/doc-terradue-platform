
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
			PlansModel.findAll().then(function(plans){
				self.plans = plans;
			});
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
			var search = this.entities.filter(function(entity){return entity.Id==id});

			if (search.length){
				var user = search[0];
				self.planData = new can.Observe({});
				self.planData.attr({
					user: user,
					plans: self.plans
				});
				self.view({
						url: 'modules/users_admin/views/plans.html',
						data: this.planData
				});
			}
		},

		'.upgradePlanBtn click': function(data){
			var self = this,
				selectedPlanId = this.element.find('input[name="planRadio"]:checked').val(),
				planSearch = $.grep(this.plans, function(plan){
					return (plan.Value==selectedPlanId);
				}),
				plan = (planSearch.length ? planSearch[0] : null),
				userid = data.data('user');
			
			if (plan && userid){
				// save plan
				this.planData.attr({
					planUpgradedLoading: true, planUpgradedSuccessName:null, planUpgradedFailMessage:null
				});
				
				PlansModel.upgrade({
 					Id: userid,
 					Plan: plan.Key,
 				}).then(function(){
					self.planData.attr('planUpgradedSuccessName', plan.Key);
 				}).fail(function(xhr){
 					errXhr=xhr; // for debug
 					self.planData.attr('planUpgradedFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
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
