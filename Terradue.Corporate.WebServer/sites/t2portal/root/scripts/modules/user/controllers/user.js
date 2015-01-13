
define(['can', 'configurations/user', 'modules/user/models/user'], function(can, Config, UserModel){
	
	var UsersControl = can.Control({
		init: function($element, options){
			console.log("usersControl.init");
			this.$element = $element;
		},
		index: function(){
			console.log("App.controllers.Users.index");
			var self = this;
			UserModel.findAll({}, function(users){
				window.users = users; $el = self.$element;
				self.$element.html(can.view("modules/user/views/users.html", users));
			});
		},
	});

	return new UsersControl(Config.mainContainer, {});
	
});
