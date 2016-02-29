
define(['jquery','can', 'config'], function($, can, Config){
	
	return {
		create: function(password){
			return $.ajax('/'+Config.api+'/user/safe', {
				type : "POST",
				dataType : "json",
				data : {Password:password}
			});
		},

		delete: function(password){
			return $.ajax('/'+Config.api+'/user/safe?password='+password, {
				type : "DELETE",
				dataType : "json"
			});
		},
				
		get: function(password){
			return $.ajax('/'+Config.api+'/user/safe/private', {
				type : "PUT",
				dataType : "json",
				data : {Password:password}
			});
		}
	};
	
});
