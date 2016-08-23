
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		update: 'PUT /'+Config.api+'/user',

		changeEmail: function(email){
			return $.ajax('/'+Config.api+'/user/email', {
				type : "PUT",
				dataType : "json",
				data : {Email:email}
			});
		},

		generateApiKey: function(password){
			return $.ajax('/'+Config.api+'/user/apikey', {
				type : "PUT",
				dataType : "json",
				format : "json",
				data : {
					Password:password
				}
			});
		},

		revokeApiKey: function(password){
			return $.ajax('/'+Config.api+'/user/apikey?format=json&password='+password, {
				type : "DELETE",
				dataType : "json"
			});
		}
	}, {});
	
});
