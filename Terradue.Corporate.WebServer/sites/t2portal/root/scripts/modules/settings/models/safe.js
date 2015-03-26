
define(['jquery','can', 'config'], function($, can, Config){
	
	return {
		create: function(password){
			return $.ajax('/'+Config.api+'/user/safe', {
				type : "POST",
				dataType : "json",
				data : {Password:password}
			});
		},
				
		get: function(password){
			return $.ajax('/'+Config.api+'/user/safe/private', {
				type : "PUT",
				dataType : "json",
				data : {Password:password}
			});
		},

		recreate: function(password){
			return $.ajax('/'+Config.api+'/user/safe', {
				type : "PUT",
				dataType : "json",
				data : {Password:password}
			});
		}
	};
	
});
