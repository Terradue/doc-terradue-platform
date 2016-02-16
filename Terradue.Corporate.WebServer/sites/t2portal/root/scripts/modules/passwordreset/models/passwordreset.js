

define(['can', 'config'], function(can, Config){
	
	return can.Model({
		create: 'PUT /'+Config.api+'/user/passwordreset',
		
		resetPassword: function(userData){
			return $.ajax('/'+Config.api+'/user/password', {
				method: 'PUT',
				data: userData
			});
		} 
		
	}, {});
	
});
