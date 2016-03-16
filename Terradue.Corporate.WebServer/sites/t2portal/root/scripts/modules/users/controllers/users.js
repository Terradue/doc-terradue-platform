
define([
	'jquery',
	'can',
	'bootbox',
	//'utils/baseControl',
	'utils/crudBaseModule/crudBaseControl',
	'config',
	'utils/helpers',
	'modules/users/models/user',
	'modules/users/models/plans',
	//'canpromise'
], function($, can, bootbox, CrudBaseControl, Config, Helpers, UserModel, PlansModel){
	
//	var UsersControl = can.Control({
//		init: function($element, options){
//			console.log("usersControl.init");
//			this.$element = $element;
//		},
//		index: function(){
//			console.log("App.controllers.Users.index");
//			var self = this;
//			UserModel.findAll({}, function(users){
//				window.users = users; $el = self.$element;
//				self.$element.html(can.view("modules/user/views/users.html", users));
//			});
//		},
//	});
	
	UsersControl = CrudBaseControl({
		init2: function(){
		},
		
		entitySelected: function(entity){
			var self = this;
			this.selectedUser = entity;
			this.plansStatus = new can.Observe({ user: this.selectedUser });
			self.element.find('.plansList').html(can.view('modules/users/views/plans.html', this.plansStatus));
			
			if (this.plans)
				this.plansStatus.attr('plans', this.plans);
			else
				PlansModel.findAll().then(function(plans){
					self.plansStatus.attr('plans', plans);
					self.plans = plans;
				});
		},
		
		'.upgradePlanBtn click': function(){
			var self = this,
				selectedPlanId = this.element.find('input[name="planRadio"]:checked').val(),
				planSearch = $.grep(this.plans, function(plan){
					return (plan.Value==selectedPlanId);
				}),
				plan = (planSearch.length ? planSearch[0] : null);
			
			if (plan && this.selectedUser){
				// save plan
				this.plansStatus.attr({
					planUpgradedLoading: true, planUpgradedSuccessName:null, planUpgradedFailMessage:null
				});
				
				PlansModel.upgrade({
 					Id: this.selectedUser.Id,
 					Level: selectedPlanId,
 				}).then(function(){
					self.plansStatus.attr('planUpgradedSuccessName', plan.Key);
 				}).fail(function(xhr){
 					errXhr=xhr; // for debug
 					self.plansStatus.attr('planUpgradedFailMessage', Helpers.getErrMsg(xhr, 'Generic Error'));
 				});
			}
		},
		
	});
	
	return new UsersControl(Config.mainContainer, {
		Model: UserModel,
		entityName: 'user',
		view: '/scripts/modules/users/views/users.html',
	});

//	return new UsersControl(Config.mainContainer, {});
	
});
