

define(['can', 'config'], function(can, Config){
	
	return can.Model({
		create: 'POST /'+Config.api+'/user/passwordreset',
		
		resetPassword: function(userData){
			return $.ajax('/'+Config.api+'/user/password/reset', {
				method: 'PUT',
				data: userData
			});
		},

		updatePassword: function(oldpassword,newpassword){
			return $.ajax('/'+Config.api+'/user/password', {
				method: 'PUT',
				data: {
					oldpassword: oldpassword,
					newpassword: newpassword
				}
			});
		}  
		
	}, {});
	
});
